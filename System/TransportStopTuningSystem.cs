// <copyright file="TransportStopTuningSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/TransportStopTuningSystem.cs
// Purpose: Applies boarding-speed slider values to public transport stop prefabs.

namespace BetterBoarding
{
    using System;
    using Colossal.Serialization.Entities;
    using CS2Shared.RiverMochi;
    using Game;
    using Game.Common;
    using Game.Prefabs;
    using Game.SceneFlow;
    using Game.Simulation;
    using Game.Tools;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;

    public partial class TransportStopTuningSystem : GameSystemBase
    {
        private const float FloatEpsilon = 0.0001f;

        private EntityQuery m_StopPrefabQuery;
        private PrefabSystem? m_PrefabSystem;
        private int m_AppliedRevision = -1;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_StopPrefabQuery = SystemAPI.QueryBuilder()
                .WithAll<PrefabData, TransportStopData>()
                .WithNone<Deleted, Destroyed, Temp, Overridden>()
                .Build();

            // This system runs as a one-shot pass when settings change.
            RequireForUpdate(m_StopPrefabQuery);
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (!IsRealGameLoad(purpose, mode))
            {
                return;
            }

            // City switch/load gives us a fresh prefab world. Re-run the one-shot pass even when
            // slider revisions did not change, and reset cached status text for the new city.
            m_AppliedRevision = -1;
            TransitWaitStatus.ResetForCityLoad();
            Enabled = true;
        }

        protected override void OnUpdate()
        {
            if (m_AppliedRevision == BoardingRuntimeSettings.StopTuningRevision)
            {
                WakeLateBoarderSystemIfNeeded();
                // Nothing changed since the last pass, so go idle.
                Enabled = false;
                return;
            }

            // Query first, then apply changes by entity so prefab tuning does not mutate during query enumeration.
            // This stays a one-shot main-thread pass because slider changes are rare.
            m_StopPrefabQuery.CompleteDependency();
            var entities = m_StopPrefabQuery.ToEntityArray(Allocator.Temp);
            var updatedPrefabs = 0;

            foreach (var prefabEntity in entities)
            {
                if (m_PrefabSystem == null ||
                    !m_PrefabSystem.TryGetPrefab<PrefabBase>(prefabEntity, out var prefab) ||
                    !prefab.TryGet<TransportStop>(out var authoringStop))
                {
                    continue;
                }

                if (!authoringStop.m_PassengerTransport)
                {
                    continue;
                }

                int speedFactor = GetSpeedFactor(authoringStop.m_TransportType);
                bool hasMarker = EntityManager.HasComponent<TransportStopTuningMarker>(prefabEntity);

                if (speedFactor == BBoardSettings.VanillaSpeedFactor && !hasMarker)
                {
                    // Strict vanilla/no-op path: leave untouched stop prefabs alone at 1x.
                    continue;
                }

                float speedMultiplier = speedFactor;
                // Authoring loading factor is stored as delta-from-1; runtime tuning uses the effective value.
                var baseEffectiveLoading = math.max(0f, 1f + authoringStop.m_LoadingFactor);
                var tunedStop = EntityManager.GetComponentData<TransportStopData>(prefabEntity);

                // Always recompute from prefab authoring values so repeated slider changes never double-scale.
                // Higher loading factor helps passengers board faster; lower boarding time shortens dwell.
                tunedStop.m_LoadingFactor = baseEffectiveLoading * speedMultiplier - 1f;
                tunedStop.m_BoardingTime = authoringStop.m_BoardingTime / speedMultiplier;

                if (speedFactor == BBoardSettings.VanillaSpeedFactor)
                {
                    // Restore only prefabs we previously marked, then remove our marker.
                    EntityManager.SetComponentData(prefabEntity, tunedStop);
                    EntityManager.RemoveComponent<TransportStopTuningMarker>(prefabEntity);
                    updatedPrefabs++;
                }
                else
                {
                    EntityManager.SetComponentData(prefabEntity, tunedStop);
                    updatedPrefabs++;

                    var marker = new TransportStopTuningMarker
                    {
                        // Marker lets us restore only prefabs Better Boarding previously touched.
                        m_LoadingFactor = tunedStop.m_LoadingFactor,
                        m_BoardingTime = tunedStop.m_BoardingTime
                    };

                    if (hasMarker)
                    {
                        var currentMarker = EntityManager.GetComponentData<TransportStopTuningMarker>(prefabEntity);
                        if (math.abs(currentMarker.m_LoadingFactor - marker.m_LoadingFactor) > FloatEpsilon ||
                            math.abs(currentMarker.m_BoardingTime - marker.m_BoardingTime) > FloatEpsilon)
                        {
                            EntityManager.SetComponentData(prefabEntity, marker);
                        }
                    }
                    else
                    {
                        EntityManager.AddComponentData(prefabEntity, marker);
                    }
                }
            }

            entities.Dispose();
            m_AppliedRevision = BoardingRuntimeSettings.StopTuningRevision;
            WakeLateBoarderSystemIfNeeded();
            Enabled = false;
            // One log line per retune is debug info and should not spam during normal play.
            LogUtils.Info(
                Mod.s_Log,
                () =>
                    $"StopTuning applied: {updatedPrefabs} transport stop prefabs" + Environment.NewLine +
                    $"Options Settings: {BoardingRuntimeSettings.DescribeForLog()}");
        }

        private void WakeLateBoarderSystemIfNeeded()
        {
            if (!BoardingRuntimeSettings.CancelLateBoarders)
            {
                return;
            }

            try
            {
                // If the checkbox was enabled before loading a city, the live setter could not wake
                // the simulation system yet. This one-shot prefab pass runs after load, so it is a
                // safe place to activate the late-cim skip system for the loaded world.
                LateBoarderCancelSystem system =
                    World.GetExistingSystemManaged<LateBoarderCancelSystem>() ??
                    World.GetOrCreateSystemManaged<LateBoarderCancelSystem>();

                system.Enabled = true;
            }
            catch (Exception ex)
            {
                Mod.WarnOnce(
                    "FB_WAKE_LATE_BOARDER_FAILED",
                    () => $"{Mod.ModTag} Failed waking late-cim skip system: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static bool IsRealGameLoad(Purpose purpose, GameMode mode)
        {
            return mode == GameMode.Game &&
                (purpose == Purpose.NewGame || purpose == Purpose.LoadGame);
        }

        private static int GetSpeedFactor(TransportType transportType)
        {
            switch (transportType)
            {
                case TransportType.Bus:
                    return BoardingRuntimeSettings.BusBoardingSpeedFactor;
                case TransportType.Train:
                case TransportType.Tram:
                case TransportType.Subway:
                    return BoardingRuntimeSettings.RailBoardingSpeedFactor;
                case TransportType.Ship:
                case TransportType.Ferry:
                    return BoardingRuntimeSettings.WaterBoardingSpeedFactor;
                case TransportType.Airplane:
                    return BoardingRuntimeSettings.AirBoardingSpeedFactor;
                default:
                    // Unknown transport types should stay at the true vanilla baseline.
                    return BBoardSettings.VanillaSpeedFactor;
            }
        }
    }
}
