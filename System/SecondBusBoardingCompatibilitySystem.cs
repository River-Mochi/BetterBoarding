// <copyright file="SecondBusBoardingCompatibilitySystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/SecondBusBoardingCompatibilitySystem.cs
// Purpose: Defers system ordering until all mods are loaded and prevents ownership conflicts.

namespace BetterBoarding
{
    using System;
    using System.Reflection;
    using CS2Shared.RiverMochi;
    using Game;
    using Game.Simulation;
    using Unity.Entities;

    internal static class SecondBusBoardingCompatibility
    {
        private const string kConcurrentAssemblyName = "ConcurrentBusBoarding";

        public static bool ConcurrentBusBoardingDetected { get; private set; }

        public static bool OrderingAvailable { get; private set; } = true;

        public static bool EffectiveEnabled =>
            BoardingRuntimeSettings.AllowSecondBusBoarding &&
            !ConcurrentBusBoardingDetected &&
            OrderingAvailable;

        public static bool DetectConflict()
        {
            bool detected = false;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                if (string.Equals(
                    assemblies[i].GetName().Name,
                    kConcurrentAssemblyName,
                    StringComparison.Ordinal))
                {
                    detected = true;
                    break;
                }
            }

            if (ConcurrentBusBoardingDetected == detected)
            {
                return false;
            }

            ConcurrentBusBoardingDetected = detected;
            if (detected)
            {
                LogUtils.Info(
                    Mod.s_Log,
                    () =>
                        "Concurrent Bus Boarding detected; Better Boarding's " +
                        "Allow Second Bus Boarding feature is inactive to prevent a stop-slot conflict.");
            }
            else
            {
                LogUtils.Info(
                    Mod.s_Log,
                    () => "Allow Second Bus Boarding compatibility check: no conflict detected.");
            }

            return true;
        }

        public static void RefreshAdmissionSystem(World world)
        {
            SecondBusBoardingAdmissionSystem? admission =
                world.GetExistingSystemManaged<SecondBusBoardingAdmissionSystem>();
            bool wasAdmissionEnabled = admission?.Enabled ?? false;
            if (admission != null)
            {
                admission.Enabled = EffectiveEnabled;
            }

            SecondBusBoardingDistributionSystem? distribution =
                world.GetExistingSystemManaged<SecondBusBoardingDistributionSystem>();
            SecondBusBoardingHoldSystem? hold =
                world.GetExistingSystemManaged<SecondBusBoardingHoldSystem>();
            if (EffectiveEnabled && !wasAdmissionEnabled)
            {
                if (distribution != null)
                {
                    distribution.Enabled = true;
                }

                if (hold != null)
                {
                    hold.Enabled = true;
                }

                admission?.RestartStatisticsCollection();
                distribution?.RestartStatisticsCollection();
            }

            if (!EffectiveEnabled)
            {
                // This runs only for an Options change or a newly detected conflict. Pay the
                // one-time dependency completion here so no synthetic Boarding state survives
                // into the next bus-AI tick.
                distribution?.ReleaseAllActiveSessions();
                if (distribution != null)
                {
                    distribution.Enabled = false;
                }

                if (hold != null)
                {
                    hold.Enabled = false;
                }
            }
        }

        public static void Reset()
        {
            ConcurrentBusBoardingDetected = false;
            OrderingAvailable = true;
        }

        public static void MarkOrderingUnavailable()
        {
            OrderingAvailable = false;
        }
    }

    /// <summary>
    /// Registers the two slot-management passes on the first simulation update. By then every
    /// mod's OnLoad has completed, so an All Aboard car-AI replacement and the separate
    /// Concurrent Bus Boarding mod can be detected reliably without compile-time references.
    /// </summary>
    public sealed partial class SecondBusBoardingCompatibilitySystem : GameSystemBase
    {
        private const uint kConflictRecheckFrames = 1024u;

        private static UpdateSystem? s_UpdateSystem;

        private SimulationSystem? m_SimulationSystem;

        private bool m_SystemsRegistered;

        private uint m_LastConflictCheckFrame;

        public static void Configure(UpdateSystem updateSystem)
        {
            s_UpdateSystem = updateSystem;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
        }

        protected override void OnUpdate()
        {
            uint frame = m_SimulationSystem?.frameIndex ?? 0u;
            if (!m_SystemsRegistered)
            {
                SecondBusBoardingCompatibility.DetectConflict();
                RegisterBoardingSystems();
                m_SystemsRegistered = true;
                m_LastConflictCheckFrame = frame;
                SecondBusBoardingCompatibility.RefreshAdmissionSystem(World);
                return;
            }

            if (frame - m_LastConflictCheckFrame < kConflictRecheckFrames)
            {
                return;
            }

            m_LastConflictCheckFrame = frame;
            if (SecondBusBoardingCompatibility.DetectConflict())
            {
                SecondBusBoardingCompatibility.RefreshAdmissionSystem(World);
            }
        }

        protected override void OnDestroy()
        {
            s_UpdateSystem = null;
            base.OnDestroy();
        }

        private void RegisterBoardingSystems()
        {
            UpdateSystem? updateSystem = s_UpdateSystem;
            if (updateSystem == null)
            {
                SecondBusBoardingCompatibility.MarkOrderingUnavailable();
                LogUtils.Warn("Second-bus boarding registration skipped: UpdateSystem is unavailable.");
                return;
            }

            Type? replacement = Type.GetType(
                "AllAboard.System.Patched.PatchedTransportCarAISystem, AllAboard",
                throwOnError: false);

            if (replacement == null)
            {
                updateSystem.UpdateBefore<SecondBusBoardingAdmissionSystem, TransportCarAISystem>(
                    SystemUpdatePhase.GameSimulation);
                updateSystem.UpdateAfter<SecondBusBoardingDistributionSystem, TransportCarAISystem>(
                    SystemUpdatePhase.GameSimulation);
                LogUtils.Info(
                    Mod.s_Log,
                    () => "Second-bus boarding ordered around the native bus AI.");
                return;
            }

            try
            {
                MethodInfo before = FindRelativeUpdateMethod(nameof(UpdateSystem.UpdateBefore));
                MethodInfo after = FindRelativeUpdateMethod(nameof(UpdateSystem.UpdateAfter));
                object[] phase = { SystemUpdatePhase.GameSimulation };

                before.MakeGenericMethod(
                        typeof(SecondBusBoardingAdmissionSystem),
                        replacement)
                    .Invoke(updateSystem, phase);
                after.MakeGenericMethod(
                        typeof(SecondBusBoardingDistributionSystem),
                        replacement)
                    .Invoke(updateSystem, phase);

                LogUtils.Info(
                    Mod.s_Log,
                    () => "Second-bus boarding ordered around All Aboard's replacement bus AI.");
            }
            catch (Exception ex)
            {
                // A reflection invoke can fail after one side was already registered. Registering
                // a native fallback here could run a full pass twice, so fail closed instead.
                SecondBusBoardingCompatibility.MarkOrderingUnavailable();
                LogUtils.Warn(
                    Mod.s_Log,
                    () =>
                        "All Aboard ordering failed; Allow Second Bus Boarding is inactive: " +
                        $"{ex.GetType().Name}: {ex.Message}",
                    ex);
            }
        }

        private static MethodInfo FindRelativeUpdateMethod(string methodName)
        {
            MethodInfo[] methods = typeof(UpdateSystem).GetMethods(
                BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name == methodName &&
                    method.IsGenericMethodDefinition &&
                    method.GetGenericArguments().Length == 2)
                {
                    return method;
                }
            }

            throw new MissingMethodException(typeof(UpdateSystem).FullName, methodName);
        }
    }
}
