// <copyright file="SecondBusBoardingSession.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/SecondBusBoardingSession.cs
// Purpose: Transient state and shared ECS lookups for optional two-bus stop service.

namespace BetterBoarding
{
    using Game.Common;
    using Game.Creatures;
    using Game.Objects;
    using Game.Prefabs;
    using Game.Routes;
    using Game.Tools;
    using Game.Vehicles;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using VehiclePublicTransport = Game.Vehicles.PublicTransport;

    /// <summary>
    /// Runtime-only state for the one following bus Better Boarding may serve beside
    /// the native lead bus. The component is provisioned disabled so starting and
    /// ending a session never requires a simulation-phase structural change.
    /// </summary>
    internal struct SecondBusBoardingSession : IComponentData, IEnableableComponent
    {
        public Entity Stop;

        public Entity LeadBus;

        public Entity Route;

        public uint AdmittedFrame;

        public uint LastRatchetFrame;

        public int LastPassengerCount;

        public int ConcurrentBoarded;

        public int ConcurrentAlighted;

        public int LeadActiveUpdates;

        public int FollowerSlotPresentations;

        public float ClosestWaitingDistance;

        public byte HasPresentedBoardingWindow;

        public byte SawPassengerCountChange;

        public byte SawWaitingPassenger;

        public byte ReleaseRequested;
    }

    internal static class SecondBusBoardingPolicy
    {
        // Fixed first-version policy: enough room for two ordinary buses plus a small gap,
        // without turning an entire street or station into one boarding area.
        public const float MaximumPairDistance = 32f;

        public const float MaximumFollowerSpeed = 1f;

        public const uint BoardingRatchetFrames = 16u;

        // The feature hands the follower back to vanilla after a bounded concurrent window.
        // A short finishing grace lets any cim already climbing aboard become Ready first.
        public const uint SessionWindowFrames = 512u;

        public const uint HardReleaseFrames = 768u;

        public static bool IsPairClose(float3 leadPosition, float3 followerPosition)
        {
            return math.distancesq(leadPosition, followerPosition) <=
                MaximumPairDistance * MaximumPairDistance;
        }

        public static bool IsFollowerSlow(float3 velocity)
        {
            return math.lengthsq(velocity) <=
                MaximumFollowerSpeed * MaximumFollowerSpeed;
        }
    }

    /// <summary>
    /// Job-side component access shared by admission and distribution. Keeping these
    /// reads and writes in scheduled jobs avoids forcing the main thread to complete
    /// the city-wide vehicle and resident jobs every time a bus reaches a stop.
    /// </summary>
    internal struct SecondBusBoardingLookups
    {
        [ReadOnly]
        public EntityStorageInfoLookup Entities;

        [ReadOnly]
        public ComponentLookup<Deleted> Deleted;

        [ReadOnly]
        public ComponentLookup<Temp> Temp;

        [ReadOnly]
        public ComponentLookup<PrefabRef> PrefabRef;

        [ReadOnly]
        public ComponentLookup<PublicTransportVehicleData> PublicTransportVehicleData;

        [ReadOnly]
        public ComponentLookup<Connected> Connected;

        [ReadOnly]
        public ComponentLookup<CurrentRoute> CurrentRoute;

        [ReadOnly]
        public ComponentLookup<Transform> Transform;

        [ReadOnly]
        public ComponentLookup<Moving> Moving;

        [ReadOnly]
        public ComponentLookup<CurrentVehicle> CurrentVehicle;

        [ReadOnly]
        public BufferLookup<Passenger> Passengers;

        public ComponentLookup<VehiclePublicTransport> PublicTransport;

        public ComponentLookup<Target> Target;

        public ComponentLookup<BoardingVehicle> BoardingVehicle;

        public static SecondBusBoardingLookups Create(SystemBase system)
        {
            return new SecondBusBoardingLookups
            {
                Entities = system.GetEntityStorageInfoLookup(),
                Deleted = system.GetComponentLookup<Deleted>(true),
                Temp = system.GetComponentLookup<Temp>(true),
                PrefabRef = system.GetComponentLookup<PrefabRef>(true),
                PublicTransportVehicleData =
                    system.GetComponentLookup<PublicTransportVehicleData>(true),
                Connected = system.GetComponentLookup<Connected>(true),
                CurrentRoute = system.GetComponentLookup<CurrentRoute>(true),
                Transform = system.GetComponentLookup<Transform>(true),
                Moving = system.GetComponentLookup<Moving>(true),
                CurrentVehicle = system.GetComponentLookup<CurrentVehicle>(true),
                Passengers = system.GetBufferLookup<Passenger>(true),
                PublicTransport = system.GetComponentLookup<VehiclePublicTransport>(false),
                Target = system.GetComponentLookup<Target>(false),
                BoardingVehicle = system.GetComponentLookup<BoardingVehicle>(false),
            };
        }

        public void Update(SystemBase system)
        {
            Entities.Update(system);
            Deleted.Update(system);
            Temp.Update(system);
            PrefabRef.Update(system);
            PublicTransportVehicleData.Update(system);
            Connected.Update(system);
            CurrentRoute.Update(system);
            Transform.Update(system);
            Moving.Update(system);
            CurrentVehicle.Update(system);
            Passengers.Update(system);
            PublicTransport.Update(system);
            Target.Update(system);
            BoardingVehicle.Update(system);
        }

        public bool IsUsable(Entity entity)
        {
            return entity != Entity.Null &&
                Entities.Exists(entity) &&
                !Deleted.HasComponent(entity) &&
                !Temp.HasComponent(entity);
        }

        public bool IsBus(Entity entity)
        {
            if (!IsUsable(entity) ||
                !PrefabRef.TryGetComponent(entity, out PrefabRef prefabRef) ||
                !IsUsable(prefabRef.m_Prefab) ||
                !PublicTransportVehicleData.TryGetComponent(
                    prefabRef.m_Prefab,
                    out PublicTransportVehicleData vehicleData))
            {
                return false;
            }

            return vehicleData.m_TransportType == TransportType.Bus;
        }

        public bool TryGetStop(Entity vehicle, out Entity stop)
        {
            stop = Entity.Null;
            if (!Target.TryGetComponent(vehicle, out Target target))
            {
                return false;
            }

            Entity targetEntity = target.m_Target;
            if (BoardingVehicle.HasComponent(targetEntity))
            {
                stop = targetEntity;
            }
            else if (Connected.TryGetComponent(targetEntity, out Connected connected) &&
                BoardingVehicle.HasComponent(connected.m_Connected))
            {
                stop = connected.m_Connected;
            }

            return IsUsable(stop) && BoardingVehicle.HasComponent(stop);
        }

        public bool AreSameRoute(Entity first, Entity second, out Entity route)
        {
            route = Entity.Null;
            if (!CurrentRoute.TryGetComponent(first, out CurrentRoute firstRoute) ||
                !CurrentRoute.TryGetComponent(second, out CurrentRoute secondRoute) ||
                firstRoute.m_Route == Entity.Null ||
                firstRoute.m_Route != secondRoute.m_Route)
            {
                return false;
            }

            route = firstRoute.m_Route;
            return IsUsable(route);
        }

        public int GetPassengerCount(Entity vehicle)
        {
            return Passengers.TryGetBuffer(vehicle, out DynamicBuffer<Passenger> passengers)
                ? passengers.Length
                : 0;
        }

        public bool ArePassengersReady(Entity vehicle)
        {
            if (!Passengers.TryGetBuffer(vehicle, out DynamicBuffer<Passenger> passengers))
            {
                return true;
            }

            for (int i = 0; i < passengers.Length; i++)
            {
                Entity passenger = passengers[i].m_Passenger;
                if (CurrentVehicle.TryGetComponent(passenger, out CurrentVehicle currentVehicle) &&
                    (currentVehicle.m_Flags & CreatureVehicleFlags.Ready) == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
