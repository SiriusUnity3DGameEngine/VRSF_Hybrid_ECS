﻿using System.Collections.Generic;
using UnityEngine;

namespace VRSF.MoveAround.Teleport.Interfaces
{
    /// <summary>
    /// Contains only getters, used in the TeleportBoundariesDisplayer script.
    /// </summary>
    public interface ITeleportComponent
    {
        bool UseBoundaries();

        List<Bounds> Boundaries();
    }
}