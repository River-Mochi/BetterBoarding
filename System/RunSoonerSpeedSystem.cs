// <copyright file="RunSoonerSpeedSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/RunSoonerSpeedSystem.cs
// Purpose: Boost only active bus/tram/train passengers that are hurrying to board.

namespace BetterBoarding
{
    using Game;
    using Game.Common;
    using Game.Creatures;
    using Game.Simulation;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    public sealed class RunSoonerSpeedSystem : GameSystemBase
    {
        // Only boost when vanilla is already allowing full running speed.
        // this avoids overriding braking, queues, blockers, or stop/connection behavior.
        private const float kFullRunThreshold = 0.95f;

        private const CreatureLaneFlags kNoBoostLaneFlags =
            CreatureLaneFlags.EndOfPath |
            CreatureLaneFlags.EndReached |
            CreatureLaneFlags.TransformTarget |
            CreatureLaneFlags.ParkingSpace |
            CreatureLaneFlags.Obsolete |
            CreatureLaneFlags.Connection |
            CreatureLaneFlags.WaitSignal |
            CreatureLaneFlags.FindLane |
            CreatureLaneFlags.Stuck |
            CreatureLaneFlags.Area |
            CreatureLaneFlags.Hangaround |
            CreatureLaneFlags.WaitPosition |
            CreatureLaneFlags.EmergeUnspawned;

        private EntityQuery m_PassengerQuery;
        private SimulationSystem? m_SimulationSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();

            // CurrentVehicle keeps this query much smaller than an all-citizen query.
            // UpdateFrame lets us match vanilla HumanNavigation/HumanMove cadence.
            m_PassengerQuery = SystemAPI.QueryBuilder()
                .WithAll<
                    Human,
                    CurrentVehicle,
                    HumanNavigation,
                    HumanCurrentLane,
                    Blocker,
                    Game.Prefabs.PrefabRef,
                    UpdateFrame>()
                .WithNone<Deleted, Destroyed, Temp, Overridden>()
                .Build();

            RequireForUpdate(m_PassengerQuery);
        }

        protected override void OnUpdate()
        {
            if (!BoardingRuntimeSettings.RunSoonerSpeedBoostEnabled)
            {
                Enabled = false;
                return;
            }

            uint updateFrame = (m_SimulationSystem?.frameIndex ?? 0) % 16;

            // Vanilla HumanNavigationSystem and HumanMoveSystem use this same rotating
            // 16-way filter, so only the passengers being moved this frame are touched.
            m_PassengerQuery.ResetFilter();
            m_PassengerQuery.SetSharedComponentFilter(new UpdateFrame(updateFrame));

            JobHandle handle = new RunSoonerSpeedJob
            {
                m_HumanType =
                    SystemAPI.GetComponentTypeHandle<Human>(isReadOnly: true),

                m_CurrentVehicleType =
                    SystemAPI.GetComponentTypeHandle<CurrentVehicle>(isReadOnly: true),

                m_NavigationType =
                    SystemAPI.GetComponentTypeHandle<HumanNavigation>(isReadOnly: false),

                m_CurrentLaneType =
                    SystemAPI.GetComponentTypeHandle<HumanCurrentLane>(isReadOnly: true),

                m_BlockerType =
                    SystemAPI.GetComponentTypeHandle<Blocker>(isReadOnly: true),

                m_PrefabRefType =
                    SystemAPI.GetComponentTypeHandle<Game.Prefabs.PrefabRef>(isReadOnly: true),

                m_ControllerData =
                    SystemAPI.GetComponentLookup<Controller>(isReadOnly: true),

                m_PublicTransportData =
                    SystemAPI.GetComponentLookup<Game.Vehicles.PublicTransport>(isReadOnly: true),

                m_PrefabRefData =
                    SystemAPI.GetComponentLookup<Game.Prefabs.PrefabRef>(isReadOnly: true),

                m_PublicTransportVehicleData =
                    SystemAPI.GetComponentLookup<Game.Prefabs.PublicTransportVehicleData>(isReadOnly: true),

                m_PrefabHumanData =
                    SystemAPI.GetComponentLookup<Game.Prefabs.HumanData>(isReadOnly: true),

                m_SpeedFactor = BoardingRuntimeSettings.PassengerRunSpeedFactor,
            }.ScheduleParallel(m_PassengerQuery, Dependency);

            Dependency = handle;
        }

        [BurstCompile]
        private struct RunSoonerSpeedJob : IJobChunk
        {
            [ReadOnly]
            public ComponentTypeHandle<Human> m_HumanType;

            [ReadOnly]
            public ComponentTypeHandle<CurrentVehicle> m_CurrentVehicleType;

            public ComponentTypeHandle<HumanNavigation> m_NavigationType;

            [ReadOnly]
            public ComponentTypeHandle<HumanCurrentLane> m_CurrentLaneType;

            [ReadOnly]
            public ComponentTypeHandle<Blocker> m_BlockerType;

            [ReadOnly]
            public ComponentTypeHandle<Game.Prefabs.PrefabRef> m_PrefabRefType;

            [ReadOnly]
            public ComponentLookup<Controller> m_ControllerData;

            [ReadOnly]
            public ComponentLookup<Game.Vehicles.PublicTransport> m_PublicTransportData;

            [ReadOnly]
            public ComponentLookup<Game.Prefabs.PrefabRef> m_PrefabRefData;

            [ReadOnly]
            public ComponentLookup<Game.Prefabs.PublicTransportVehicleData> m_PublicTransportVehicleData;

            [ReadOnly]
            public ComponentLookup<Game.Prefabs.HumanData> m_PrefabHumanData;

            [ReadOnly]
            public int m_SpeedFactor;

            public void Execute(
                in ArchetypeChunk chunk,
                int unfilteredChunkIndex,
                bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                _ = unfilteredChunkIndex;
                _ = useEnabledMask;
                _ = chunkEnabledMask;

                NativeArray<Human> humans =
                    chunk.GetNativeArray(ref m_HumanType);

                NativeArray<CurrentVehicle> currentVehicles =
                    chunk.GetNativeArray(ref m_CurrentVehicleType);

                NativeArray<HumanNavigation> navigations =
                    chunk.GetNativeArray(ref m_NavigationType);

                NativeArray<HumanCurrentLane> currentLanes =
                    chunk.GetNativeArray(ref m_CurrentLaneType);

                NativeArray<Blocker> blockers =
                    chunk.GetNativeArray(ref m_BlockerType);

                NativeArray<Game.Prefabs.PrefabRef> prefabRefs =
                    chunk.GetNativeArray(ref m_PrefabRefType);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Human human = humans[i];

                    // BetterBoarding starts this vanilla Run state sooner.
                    if ((human.m_Flags & HumanFlags.Run) == 0)
                    {
                        continue;
                    }

                    CurrentVehicle currentVehicle = currentVehicles[i];

                    // Once the cim starts entering, is ready, or is exiting,
                    // movement is back entirely under vanilla control.
                    if (currentVehicle.m_Vehicle == Entity.Null ||
                        (currentVehicle.m_Flags &
                            (CreatureVehicleFlags.Ready |
                             CreatureVehicleFlags.Entering |
                             CreatureVehicleFlags.Exiting)) != 0)
                    {
                        continue;
                    }

                    Entity assignedVehicle = currentVehicle.m_Vehicle;
                    Entity controllerVehicle = assignedVehicle;

                    // Multi-car trains/trams store CurrentVehicle against an individual
                    // car, while boarding state belongs to the controller vehicle.
                    if (m_ControllerData.TryGetComponent(
                            assignedVehicle,
                            out Controller controller) &&
                        controller.m_Controller != Entity.Null)
                    {
                        controllerVehicle = controller.m_Controller;
                    }

                    if (!m_PublicTransportData.TryGetComponent(
                            controllerVehicle,
                            out Game.Vehicles.PublicTransport publicTransport))
                    {
                        continue;
                    }

                    if ((publicTransport.m_State & PublicTransportFlags.Boarding) == 0 ||
                        (publicTransport.m_State &
                            (PublicTransportFlags.Evacuating |
                             PublicTransportFlags.PrisonerTransport |
                             PublicTransportFlags.Refueling)) != 0)
                    {
                        continue;
                    }

                    if (!TryGetTransportType(
                            controllerVehicle,
                            assignedVehicle,
                            out Game.Prefabs.TransportType transportType) ||
                        (transportType != Game.Prefabs.TransportType.Bus &&
                         transportType != Game.Prefabs.TransportType.Tram &&
                         transportType != Game.Prefabs.TransportType.Train))
                    {
                        continue;
                    }

                    HumanCurrentLane currentLane = currentLanes[i];

                    // Do not override vanilla's special lane/stop/queue behavior.
                    if (currentLane.m_Lane == Entity.Null ||
                        (currentLane.m_Flags & kNoBoostLaneFlags) != 0 ||
                        currentLane.m_QueueEntity != Entity.Null ||
                        currentLane.m_QueueArea.radius > 0f)
                    {
                        continue;
                    }

                    Blocker blocker = blockers[i];
                    if (blocker.m_Blocker != Entity.Null)
                    {
                        continue;
                    }

                    Game.Prefabs.PrefabRef prefabRef = prefabRefs[i];
                    if (prefabRef.m_Prefab == Entity.Null ||
                        !m_PrefabHumanData.TryGetComponent(
                            prefabRef.m_Prefab,
                            out Game.Prefabs.HumanData humanData) ||
                        humanData.m_RunSpeed <= 0f)
                    {
                        continue;
                    }

                    HumanNavigation navigation = navigations[i];

                    if (navigation.m_TargetActivity != 0)
                    {
                        continue;
                    }

                    // Vanilla has already done its collision, queue and braking work.
                    // Only boost when it still permits essentially full running speed.
                    if (navigation.m_MaxSpeed <
                        humanData.m_RunSpeed * kFullRunThreshold)
                    {
                        continue;
                    }

                    navigation.m_MaxSpeed =
                        humanData.m_RunSpeed * m_SpeedFactor;

                    navigations[i] = navigation;
                }
            }

            private bool TryGetTransportType(
                Entity controllerVehicle,
                Entity assignedVehicle,
                out Game.Prefabs.TransportType transportType)
            {
                if (TryGetTransportType(controllerVehicle, out transportType))
                {
                    return true;
                }

                return TryGetTransportType(assignedVehicle, out transportType);
            }

            private bool TryGetTransportType(
                Entity vehicle,
                out Game.Prefabs.TransportType transportType)
            {
                transportType = Game.Prefabs.TransportType.None;

                if (!m_PrefabRefData.TryGetComponent(
                        vehicle,
                        out Game.Prefabs.PrefabRef prefabRef) ||
                    prefabRef.m_Prefab == Entity.Null ||
                    !m_PublicTransportVehicleData.TryGetComponent(
                        prefabRef.m_Prefab,
                        out Game.Prefabs.PublicTransportVehicleData transportData))
                {
                    return false;
                }

                transportType = transportData.m_TransportType;
                return true;
            }
        }
    }
}
