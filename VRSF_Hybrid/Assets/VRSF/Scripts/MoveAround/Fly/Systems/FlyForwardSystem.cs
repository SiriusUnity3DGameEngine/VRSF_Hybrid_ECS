﻿using Unity.Entities;
using VRSF.MoveAround.Components;
using VRSF.Utils.Components.ButtonActionChoser;

namespace VRSF.MoveAround.Systems
{
    /// <summary>
    /// If the user is interacting, calculate if we fly forward or backward
    /// </summary>
    public class FlyForwardSystem : ComponentSystem
    {
        struct Filter
        {
            public FlyParametersComponent FlyComponents;
            public ButtonActionChoserComponents ButtonComponents;
        }


        #region ComponentSystem_Methods
        protected override void OnUpdate()
        {
            foreach (var e in GetEntities<Filter>())
            {
                if (e.FlyComponents.IsInteracting)
                {
                    CalculateFlyForward(e);
                }
            }
        }
        #endregion ComponentSystem_Methods


        #region PRIVATE_METHODS
        /// <summary>
        /// Calculate if the user is flying forward or backward and init some values.
        /// </summary>
        private void CalculateFlyForward(Filter entity)
        {
            entity.FlyComponents.FlyForward = (entity.ButtonComponents.ThumbPos.Value.y >= 0.0f) ? true : false;

            // If user just started to press/touch the thumbstick
            if (!entity.FlyComponents.WantToFly)
            {
                entity.FlyComponents.TimeSinceStartFlying = 0.0f;
                entity.FlyComponents.WantToFly = true;
            }
        }
        #endregion PRIVATE_METHODS
    }
}