// <copyright file="LatePassengerProximityPolicy.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/LatePassengerProximityPolicy.cs
// Purpose: Conservative proximity gate for releasing riders who are not at their stop.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Game.Common;
    using Game.Creatures;
    using Game.Prefabs;
    using Game.Routes;
    using Game.Vehicles;
    using Unity.Entities;
    using Unity.Mathematics;

    internal static class LatePassengerProximityPolicy
    {
        // This is deliberately larger than an ordinary platform. A rider in or near a
        // station is protected even if the consist occupies only part of the platform.
        private const float kRailClearlyFarMeters = 200f;
        private const float kBusTramClearlyFarMeters = 75f;
        private const int kMaxOwnerHops = 16;

        // Valid only for one boarding controller during one assist pass. Vehicles and
        // stop ownership can change, so callers must not keep this across updates.
        internal readonly struct StopContext
        {
            public StopContext(Entity waypoint, Entity stop, Entity station, float3 position)
            {
                Waypoint = waypoint;
                Stop = stop;
                Station = station;
                Position = position;
            }

            public Entity Waypoint { get; }

            public Entity Stop { get; }

            public Entity Station { get; }

            public float3 Position { get; }
        }

        public static bool TryCreateStopContext(
            EntityManager manager,
            Entity controller,
            bool requireStationOwner,
            out StopContext context)
        {
            context = default;
            if (!manager.Exists(controller) || !manager.HasComponent<Target>(controller))
            {
                return false;
            }

            Entity waypoint = manager.GetComponentData<Target>(controller).m_Target;
            if (waypoint == Entity.Null ||
                !manager.Exists(waypoint) ||
                !manager.HasComponent<Connected>(waypoint) ||
                !manager.HasComponent<Game.Routes.Position>(waypoint))
            {
                return false;
            }

            Entity stop = manager.GetComponentData<Connected>(waypoint).m_Connected;
            if (stop == Entity.Null || !manager.Exists(stop))
            {
                return false;
            }

            Entity station = FindStationOwner(manager, stop);
            if (requireStationOwner && station == Entity.Null)
            {
                // Unknown rail topology must not turn into a false far-away verdict.
                return false;
            }

            float3 position =
                manager.GetComponentData<Game.Routes.Position>(waypoint).m_Position;
            context = new StopContext(waypoint, stop, station, position);
            return true;
        }

        // The live pass only needs a yes/no decision. Protect known stop/station riders
        // before collecting car positions; the on-demand report still uses TryMeasure
        // to obtain complete distances.
        public static bool IsClearlyFarWithCachedStop(
            EntityManager manager,
            Entity controller,
            Entity passenger,
            StopContext context,
            bool isRail,
            List<float3> carPositions,
            ref bool carsCached,
            ref bool carsValid)
        {
            if (!manager.Exists(passenger) ||
                !manager.HasComponent<HumanCurrentLane>(passenger) ||
                !manager.HasComponent<Game.Objects.Transform>(passenger))
            {
                return false;
            }

            HumanCurrentLane lane = manager.GetComponentData<HumanCurrentLane>(passenger);
            if (lane.m_QueueEntity == context.Stop ||
                lane.m_QueueEntity == context.Waypoint)
            {
                return false;
            }

            if (manager.HasComponent<Creature>(passenger))
            {
                Entity queuedAt = manager.GetComponentData<Creature>(passenger).m_QueueEntity;
                if (queuedAt == context.Stop || queuedAt == context.Waypoint)
                {
                    return false;
                }
            }

            float3 passengerPosition =
                manager.GetComponentData<Game.Objects.Transform>(passenger).m_Position;
            float threshold = isRail ? kRailClearlyFarMeters : kBusTramClearlyFarMeters;
            float thresholdSquared = threshold * threshold;
            if (!(math.distancesq(passengerPosition, context.Position) > thresholdSquared))
            {
                return false;
            }

            if (context.Station != Entity.Null &&
                IsOwnedBy(manager, lane.m_Lane, context.Station))
            {
                return false;
            }

            if (!carsCached)
            {
                carsCached = true;
                carsValid = TryCacheCarPositions(manager, controller, carPositions);
            }

            if (!carsValid)
            {
                return false;
            }

            for (int i = 0; i < carPositions.Count; i++)
            {
                // The negated comparison also fails closed for an invalid NaN position.
                if (!(math.distancesq(passengerPosition, carPositions[i]) > thresholdSquared))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryCacheCarPositions(
            EntityManager manager,
            Entity controller,
            List<float3> carPositions)
        {
            carPositions.Clear();
            if (manager.HasBuffer<LayoutElement>(controller))
            {
                DynamicBuffer<LayoutElement> layout =
                    manager.GetBuffer<LayoutElement>(controller, isReadOnly: true);
                if (layout.Length > 0)
                {
                    for (int i = 0; i < layout.Length; i++)
                    {
                        Entity car = layout[i].m_Vehicle;
                        if (!manager.Exists(car) ||
                            !manager.HasComponent<Game.Objects.Transform>(car))
                        {
                            carPositions.Clear();
                            return false;
                        }

                        carPositions.Add(
                            manager.GetComponentData<Game.Objects.Transform>(car).m_Position);
                    }

                    return true;
                }
            }

            if (!manager.HasComponent<Game.Objects.Transform>(controller))
            {
                return false;
            }

            carPositions.Add(
                manager.GetComponentData<Game.Objects.Transform>(controller).m_Position);
            return true;
        }

        public static TransportType GetTransportType(EntityManager manager, Entity controller)
        {
            if (!manager.Exists(controller) ||
                !manager.HasComponent<PrefabRef>(controller))
            {
                return TransportType.None;
            }

            Entity prefab = manager.GetComponentData<PrefabRef>(controller).m_Prefab;
            if (!manager.HasComponent<PublicTransportVehicleData>(prefab))
            {
                return TransportType.None;
            }

            return manager.GetComponentData<PublicTransportVehicleData>(prefab).m_TransportType;
        }

        public static bool IsClearlyFar(
            float stopDistance,
            float carDistance,
            bool onStationLane)
        {
            return !onStationLane &&
                stopDistance > kRailClearlyFarMeters &&
                carDistance > kRailClearlyFarMeters;
        }

        public static bool TryMeasure(
            EntityManager manager,
            Entity controller,
            Entity passenger,
            out float stopDistance,
            out float carDistance,
            out bool onStationLane)
        {
            return TryMeasure(
                manager,
                controller,
                passenger,
                requireStationOwner: true,
                out stopDistance,
                out carDistance,
                out onStationLane);
        }

        private static bool TryMeasure(
            EntityManager manager,
            Entity controller,
            Entity passenger,
            bool requireStationOwner,
            out float stopDistance,
            out float carDistance,
            out bool onStationLane)
        {
            stopDistance = float.NaN;
            carDistance = float.NaN;
            onStationLane = false;

            if (!manager.Exists(controller) ||
                !manager.Exists(passenger) ||
                !manager.HasComponent<Target>(controller) ||
                !manager.HasComponent<HumanCurrentLane>(passenger) ||
                !manager.HasComponent<Game.Objects.Transform>(passenger))
            {
                return false;
            }

            Entity waypoint = manager.GetComponentData<Target>(controller).m_Target;
            if (waypoint == Entity.Null ||
                !manager.Exists(waypoint) ||
                !manager.HasComponent<Connected>(waypoint) ||
                !manager.HasComponent<Game.Routes.Position>(waypoint))
            {
                return false;
            }

            Entity stop = manager.GetComponentData<Connected>(waypoint).m_Connected;
            if (stop == Entity.Null || !manager.Exists(stop))
            {
                return false;
            }

            Entity station = FindStationOwner(manager, stop);
            if (requireStationOwner && station == Entity.Null)
            {
                // Unknown/custom stop topology: do not guess that a rider is far away.
                return false;
            }

            HumanCurrentLane lane = manager.GetComponentData<HumanCurrentLane>(passenger);
            onStationLane = lane.m_QueueEntity == stop ||
                lane.m_QueueEntity == waypoint ||
                (station != Entity.Null && IsOwnedBy(manager, lane.m_Lane, station));
            if (!onStationLane && manager.HasComponent<Creature>(passenger))
            {
                Entity queuedAt = manager.GetComponentData<Creature>(passenger).m_QueueEntity;
                onStationLane = queuedAt == stop || queuedAt == waypoint;
            }

            float3 passengerPosition =
                manager.GetComponentData<Game.Objects.Transform>(passenger).m_Position;
            float3 stopPosition =
                manager.GetComponentData<Game.Routes.Position>(waypoint).m_Position;
            stopDistance = math.distance(passengerPosition, stopPosition);

            float nearestCarDistance = float.MaxValue;
            if (manager.HasBuffer<LayoutElement>(controller))
            {
                DynamicBuffer<LayoutElement> layout =
                    manager.GetBuffer<LayoutElement>(controller, isReadOnly: true);
                if (layout.Length == 0 &&
                    manager.HasComponent<Game.Objects.Transform>(controller))
                {
                    float3 controllerPosition =
                        manager.GetComponentData<Game.Objects.Transform>(controller).m_Position;
                    nearestCarDistance = math.distance(passengerPosition, controllerPosition);
                }

                for (int i = 0; i < layout.Length; i++)
                {
                    Entity car = layout[i].m_Vehicle;
                    if (!manager.Exists(car) ||
                        !manager.HasComponent<Game.Objects.Transform>(car))
                    {
                        // One unmeasurable car could be beside the rider. Do not
                        // claim the rider is far from the entire consist.
                        return false;
                    }

                    float3 carPosition =
                        manager.GetComponentData<Game.Objects.Transform>(car).m_Position;
                    nearestCarDistance = math.min(
                        nearestCarDistance,
                        math.distance(passengerPosition, carPosition));
                }
            }
            else if (manager.HasComponent<Game.Objects.Transform>(controller))
            {
                float3 carPosition =
                    manager.GetComponentData<Game.Objects.Transform>(controller).m_Position;
                nearestCarDistance = math.distance(passengerPosition, carPosition);
            }

            if (nearestCarDistance == float.MaxValue)
            {
                return false;
            }

            carDistance = nearestCarDistance;
            return true;
        }

        private static Entity FindStationOwner(EntityManager manager, Entity stop)
        {
            Entity current = stop;
            for (int i = 0; i < kMaxOwnerHops && current != Entity.Null; i++)
            {
                if (!manager.Exists(current))
                {
                    return Entity.Null;
                }

                if (manager.HasComponent<Game.Buildings.TransportStation>(current))
                {
                    return current;
                }

                if (!manager.HasComponent<Owner>(current))
                {
                    return Entity.Null;
                }

                Entity next = manager.GetComponentData<Owner>(current).m_Owner;
                if (next == current)
                {
                    return Entity.Null;
                }

                current = next;
            }

            return Entity.Null;
        }

        private static bool IsOwnedBy(
            EntityManager manager,
            Entity entity,
            Entity owner)
        {
            Entity current = entity;
            for (int i = 0; i < kMaxOwnerHops && current != Entity.Null; i++)
            {
                if (current == owner)
                {
                    return true;
                }

                if (!manager.Exists(current) ||
                    !manager.HasComponent<Owner>(current))
                {
                    return false;
                }

                Entity next = manager.GetComponentData<Owner>(current).m_Owner;
                if (next == current)
                {
                    return false;
                }

                current = next;
            }

            return false;
        }
    }
}
