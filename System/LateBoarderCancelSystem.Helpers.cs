// <copyright file="LateBoarderCancelSystem.Helpers.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/LateBoarderCancelSystem.Helpers.cs
// Purpose: Helper methods for tool safety, passenger checks, run assists, and cancellation edits.

namespace BoardingNow
{
    using Game; // GameSystemBase
    using Game.Common; // Deleted, Destroyed, Overridden
    using Game.Creatures; // Human, CurrentVehicle, group checks
    using Game.Pathfind; // PathOwner, PathElement
    using Game.Tools; // ToolBaseSystem
    using Game.Vehicles; // CargoTransport, LayoutElement, Passenger
    using System; // Math
    using System.Collections.Generic; // HashSet
    using Unity.Entities; // DynamicBuffer, Entity, EntityCommandBuffer
    using PrefabRef = Game.Prefabs.PrefabRef; // prefab lookup
    using PublicTransportVehicleData = Game.Prefabs.PublicTransportVehicleData; // transport type source
    using TransportType = Game.Prefabs.TransportType; // bus/train/etc.

    public partial class LateBoarderCancelSystem : GameSystemBase
    {
        // Vanilla sets HumanFlags.Run at departure time. This beta assist starts that
        // same behavior a little earlier for road transit passengers who are already assigned.
        private const uint RunSoonerLeadFrames = 512u;

        private bool IsGroupPassenger(Entity passenger)
        {
            // Groups/families have leader/member rules, so the safe beta behavior stays solo-only.
            return EntityManager.HasComponent<GroupMember>(passenger) ||
                   EntityManager.HasBuffer<GroupCreature>(passenger);
        }

        private bool IsPlayerUsingTool()
        {
            if (m_ToolSystem == null || m_DefaultToolSystem == null)
            {
                return false;
            }

            ToolBaseSystem? activeTool = m_ToolSystem.activeTool;
            return activeTool != null && activeTool != m_DefaultToolSystem;
        }

        private bool CanSafelyCancelPassenger(Entity vehicleEntity, Entity passenger)
        {
            if (IsGroupPassenger(passenger))
            {
                return false;
            }

            if (!EntityManager.HasComponent<Game.Creatures.Resident>(passenger) ||
                !EntityManager.HasComponent<Human>(passenger) ||
                !EntityManager.HasComponent<PathOwner>(passenger) ||
                !EntityManager.HasBuffer<PathElement>(passenger))
            {
                return false;
            }

            var pathOwner = EntityManager.GetComponentData<PathOwner>(passenger);
            var pathElements = EntityManager.GetBuffer<PathElement>(passenger);

            // Only inspect the remaining path from the current cursor forward.
            // Older elements before m_ElementIndex are already behind the cim.
            int startIndex = Math.Max(0, pathOwner.m_ElementIndex);

            // Only cancel when the remaining path still contains this exact vehicle.
            for (var i = startIndex; i < pathElements.Length; i++)
            {
                if (pathElements[i].m_Target == vehicleEntity)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsPastDepartureFrames(
            Entity vehicleEntity,
            Game.Vehicles.PublicTransport publicTransport,
            uint frame)
        {
            return IsPastDepartureFrames(GetLatestDepartureFrame(vehicleEntity, publicTransport), frame);
        }

        private static bool IsPastDepartureFrames(uint latestDepartureFrame, uint frame)
        {
            return latestDepartureFrame != 0 && frame > latestDepartureFrame;
        }

        private static bool IsPastLateCancelGraceFrame(uint latestDepartureFrame, uint frame)
        {
            return latestDepartureFrame != 0 &&
                   frame >= latestDepartureFrame &&
                   frame - latestDepartureFrame >= LatePassengerCancelGraceFrames;
        }

        private uint GetLatestDepartureFrame(
            Entity vehicleEntity,
            Game.Vehicles.PublicTransport publicTransport)
        {
            uint latestDepartureFrame = publicTransport.m_DepartureFrame;

            if (EntityManager.HasComponent<CargoTransport>(vehicleEntity))
            {
                CargoTransport cargoTransport = EntityManager.GetComponentData<CargoTransport>(vehicleEntity);
                if (cargoTransport.m_DepartureFrame > latestDepartureFrame)
                {
                    latestDepartureFrame = cargoTransport.m_DepartureFrame;
                }
            }

            return latestDepartureFrame;
        }

        private int QueueRoadTransitPassengersRunSooner(
            ref EntityCommandBuffer ecb,
            Entity vehicleEntity,
            TransportType transportType,
            Game.Vehicles.PublicTransport publicTransport,
            uint frame,
            uint latestDepartureFrame,
            ref int sampledRunSoonerPassengers)
        {
            if (!BoardingRuntimeSettings.CimsRunSoonerToCatchBuses ||
                (transportType != TransportType.Bus && transportType != TransportType.Tram) ||
                latestDepartureFrame == 0 ||
                frame >= latestDepartureFrame ||
                latestDepartureFrame - frame > RunSoonerLeadFrames)
            {
                return 0;
            }

            if ((publicTransport.m_State & PublicTransportFlags.Boarding) == 0 ||
                (publicTransport.m_State & (PublicTransportFlags.Evacuating | PublicTransportFlags.PrisonerTransport | PublicTransportFlags.Refueling)) != 0)
            {
                return 0;
            }

            if (EntityManager.HasBuffer<LoadingResources>(vehicleEntity) &&
                EntityManager.GetBuffer<LoadingResources>(vehicleEntity).Length > 0)
            {
                return 0;
            }

            // Layout support is needed for trams and harmless for buses.
            if (EntityManager.HasBuffer<LayoutElement>(vehicleEntity))
            {
                int queued = 0;
                DynamicBuffer<LayoutElement> layout = EntityManager.GetBuffer<LayoutElement>(vehicleEntity);
                for (int i = 0; i < layout.Length; i++)
                {
                    queued += QueueRunForPassengersOnVehicle(
                        ref ecb,
                        layout[i].m_Vehicle,
                        transportType,
                        ref sampledRunSoonerPassengers);
                }

                return queued;
            }

            return QueueRunForPassengersOnVehicle(
                ref ecb,
                vehicleEntity,
                transportType,
                ref sampledRunSoonerPassengers);
        }

        private int QueueRunForPassengersOnVehicle(
            ref EntityCommandBuffer ecb,
            Entity vehicleEntity,
            TransportType transportType,
            ref int sampledRunSoonerPassengers)
        {
            if (!EntityManager.Exists(vehicleEntity) ||
                !EntityManager.HasBuffer<Passenger>(vehicleEntity))
            {
                return 0;
            }

            int queued = 0;
            DynamicBuffer<Passenger> passengers = EntityManager.GetBuffer<Passenger>(vehicleEntity);
            for (int i = 0; i < passengers.Length; i++)
            {
                Entity passenger = passengers[i].m_Passenger;
                if (!EntityManager.Exists(passenger) ||
                    EntityManager.HasComponent<Deleted>(passenger) ||
                    EntityManager.HasComponent<Destroyed>(passenger) ||
                    EntityManager.HasComponent<Temp>(passenger) ||
                    EntityManager.HasComponent<Overridden>(passenger) ||
                    !EntityManager.HasComponent<CurrentVehicle>(passenger) ||
                    !EntityManager.HasComponent<Human>(passenger))
                {
                    continue;
                }

                CurrentVehicle currentVehicle = EntityManager.GetComponentData<CurrentVehicle>(passenger);
                if (currentVehicle.m_Vehicle != vehicleEntity ||
                    (currentVehicle.m_Flags & CreatureVehicleFlags.Ready) != 0)
                {
                    continue;
                }

                Human human = EntityManager.GetComponentData<Human>(passenger);
                if ((human.m_Flags & HumanFlags.Run) != 0)
                {
                    continue;
                }

                human.m_Flags |= HumanFlags.Run;
                ecb.SetComponent(passenger, human);
                queued++;

                if (sampledRunSoonerPassengers < MaxRunSoonerFollowUpSamplesPerUpdate &&
                    ShouldLogDiagnostics())
                {
                    TrackRunSoonerFollowUpSample(transportType, vehicleEntity, passenger);
                    sampledRunSoonerPassengers++;
                }
            }

            return queued;
        }

        private static void AccumulateCanceledCount(
            TransportType transportType,
            int count,
            ref int busCanceled,
            ref int trainCanceled,
            ref int tramCanceled,
            ref int subwayCanceled,
            ref int shipCanceled,
            ref int ferryCanceled,
            ref int airCanceled)
        {
            switch (transportType)
            {
                case TransportType.Bus:
                    busCanceled += count;
                    break;
                case TransportType.Train:
                    trainCanceled += count;
                    break;
                case TransportType.Tram:
                    tramCanceled += count;
                    break;
                case TransportType.Subway:
                    subwayCanceled += count;
                    break;
                case TransportType.Ship:
                    shipCanceled += count;
                    break;
                case TransportType.Ferry:
                    ferryCanceled += count;
                    break;
                case TransportType.Airplane:
                    airCanceled += count;
                    break;
            }
        }

        private static bool TryCountCanceledSample(
            TransportType transportType,
            ref int busSampleCount,
            ref int trainSampleCount,
            ref int tramSampleCount,
            ref int subwaySampleCount,
            ref int shipSampleCount,
            ref int ferrySampleCount,
            ref int airSampleCount)
        {
            switch (transportType)
            {
                case TransportType.Bus:
                    return TryIncrementSampleCount(ref busSampleCount);
                case TransportType.Train:
                    return TryIncrementSampleCount(ref trainSampleCount);
                case TransportType.Tram:
                    return TryIncrementSampleCount(ref tramSampleCount);
                case TransportType.Subway:
                    return TryIncrementSampleCount(ref subwaySampleCount);
                case TransportType.Ship:
                    return TryIncrementSampleCount(ref shipSampleCount);
                case TransportType.Ferry:
                    return TryIncrementSampleCount(ref ferrySampleCount);
                case TransportType.Airplane:
                    return TryIncrementSampleCount(ref airSampleCount);
                default:
                    return false;
            }
        }

        private static bool TryIncrementSampleCount(ref int sampleCount)
        {
            if (sampleCount >= MaxSampledCimsPerModePerUpdate)
            {
                return false;
            }

            sampleCount++;
            return true;
        }

        private void RecordCanceledCounts(
            int busCanceled,
            int trainCanceled,
            int tramCanceled,
            int subwayCanceled,
            int shipCanceled,
            int ferryCanceled,
            int airCanceled)
        {
            TransitWaitStatus.RecordLateBoardersCanceled(World, TransportType.Bus, busCanceled);
            TransitWaitStatus.RecordLateBoardersCanceled(World, TransportType.Train, trainCanceled);
            TransitWaitStatus.RecordLateBoardersCanceled(World, TransportType.Tram, tramCanceled);
            TransitWaitStatus.RecordLateBoardersCanceled(World, TransportType.Subway, subwayCanceled);
            TransitWaitStatus.RecordLateBoardersCanceled(World, TransportType.Ship, shipCanceled);
            TransitWaitStatus.RecordLateBoardersCanceled(World, TransportType.Ferry, ferryCanceled);
            TransitWaitStatus.RecordLateBoardersCanceled(World, TransportType.Airplane, airCanceled);
        }

        private bool QueuePassengerCancellation(ref EntityCommandBuffer ecb, Entity vehicleEntity, Entity passenger)
        {
            if (!EntityManager.HasComponent<CurrentVehicle>(passenger) ||
                !EntityManager.HasComponent<Game.Creatures.Resident>(passenger) ||
                !EntityManager.HasComponent<Human>(passenger) ||
                !EntityManager.HasComponent<PathOwner>(passenger) ||
                !EntityManager.HasBuffer<PathElement>(passenger))
            {
                return false;
            }

            var pathOwner = EntityManager.GetComponentData<PathOwner>(passenger);
            var pathElements = EntityManager.GetBuffer<PathElement>(passenger);

            // Match vanilla's "search from the current path cursor onward" behavior.
            int startIndex = Math.Max(0, pathOwner.m_ElementIndex);

            // -1 is a sentinel meaning "vehicle not found yet".
            // It is never used as an array index; the < 0 check below aborts first.
            int vehiclePathIndex = -1;
            for (var i = startIndex; i < pathElements.Length; i++)
            {
                if (pathElements[i].m_Target == vehicleEntity)
                {
                    vehiclePathIndex = i;
                    break;
                }
            }

            if (vehiclePathIndex < 0)
            {
                // Safety stop: if the vehicle disappeared from the remaining path, do nothing.
                return false;
            }

            ecb.RemoveComponent<CurrentVehicle>(passenger);

            // Clear "in vehicle" state so the cim is treated as no longer attached to this vehicle.
            var resident = EntityManager.GetComponentData<Game.Creatures.Resident>(passenger);
            resident.m_Flags &= ~ResidentFlags.InVehicle;
            resident.m_Timer = 0;
            ecb.SetComponent(passenger, resident);

            var human = EntityManager.GetComponentData<Human>(passenger);
            // Clear urgency from the missed boarding attempt before vanilla resumes control.
            human.m_Flags &= ~(HumanFlags.Run | HumanFlags.Emergency);
            ecb.SetComponent(passenger, human);

            // Replace the path buffer during ECB playback so vanilla can continue from the next leg.
            // Copy starts after the missed vehicle, so the missed leg is removed.
            DynamicBuffer<PathElement> newPath = ecb.SetBuffer<PathElement>(passenger);
            for (var i = vehiclePathIndex + 1; i < pathElements.Length; i++)
            {
                newPath.Add(pathElements[i]);
            }

            // Restart from the beginning of the trimmed buffer.
            pathOwner.m_ElementIndex = 0;
            ecb.SetComponent(passenger, pathOwner);
            return true;
        }

        private static void QueueVehiclePassengerBuffer(
            ref EntityCommandBuffer ecb,
            Entity vehicleEntity,
            DynamicBuffer<Passenger> passengers,
            HashSet<Entity> canceledPassengers)
        {
            // Rebuild the passenger buffer once so entries are not removed while iterating it.
            DynamicBuffer<Passenger> newPassengers = ecb.SetBuffer<Passenger>(vehicleEntity);
            for (var i = 0; i < passengers.Length; i++)
            {
                if (!canceledPassengers.Contains(passengers[i].m_Passenger))
                {
                    newPassengers.Add(passengers[i]);
                }
            }
        }

        private TransportType GetTransportType(Entity vehicleEntity)
        {
            if (!EntityManager.HasComponent<PrefabRef>(vehicleEntity))
            {
                return TransportType.None;
            }

            Entity prefab = EntityManager.GetComponentData<PrefabRef>(vehicleEntity).m_Prefab;
            if (!EntityManager.HasComponent<PublicTransportVehicleData>(prefab))
            {
                return TransportType.None;
            }

            return EntityManager.GetComponentData<PublicTransportVehicleData>(prefab).m_TransportType;
        }
    }
}
