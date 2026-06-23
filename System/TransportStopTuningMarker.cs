// <copyright file="TransportStopTuningMarker.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// File: System/TransportStopTuningMarker.cs
// Purpose: Marker component recording the current tuned stop values.

namespace FastBoarding
{
    using Unity.Entities;

    /// <summary>
    /// Runtime marker proving a prefab has Fast Boarding values applied.
    /// Returning sliders to 1x removes it.
    /// </summary>
    public struct TransportStopTuningMarker : IComponentData
    {
        public float m_LoadingFactor;

        public float m_BoardingTime;
    }
}
