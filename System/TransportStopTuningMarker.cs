// <copyright file="TransportStopTuningMarker.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/TransportStopTuningMarker.cs
// Purpose: Marker component recording the current tuned stop values.

namespace BoardingNow
{
    using Unity.Entities;

    /// <summary>
    /// Runtime marker proving a prefab has Boarding Now values applied.
    /// Returning sliders to 1x removes it.
    /// </summary>
    public struct TransportStopTuningMarker : IComponentData
    {
        public float m_LoadingFactor;

        public float m_BoardingTime;
    }
}
