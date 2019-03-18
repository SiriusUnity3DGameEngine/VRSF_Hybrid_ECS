﻿using Unity.Entities;
using VRSF.Core.Controllers;
using VRSF.Core.Events;
using VRSF.Core.Interactions;
using VRSF.Core.Raycast;
using VRSF.Core.SetupVR;

namespace VRSF.Gaze.Interactions
{
    public class OnColliderGazeClickSystem : ComponentSystem
    {
        struct Filter
        {
            public OnColliderClickComponent OnClickComp;
            public ScriptableRaycastComponent PointerRaycast;
            public ScriptableSingletonsComponent ScriptableSingletons;
        }
        

        #region ComponentSystem_Methods
        protected override void OnUpdate()
        {
            foreach (var entity in GetEntities<Filter>())
            {
                if (entity.ScriptableSingletons._IsSetup && entity.ScriptableSingletons.GazeParameters.UseGaze &&
                    entity.PointerRaycast.CheckRaycast)
                {
                    CheckResetClick(entity);

                    if (entity.ScriptableSingletons.GazeParameters.UseGaze && !entity.ScriptableSingletons.InputsContainer.GazeIsCliking.Value && entity.ScriptableSingletons.InteractionsContainer.HasClickSomethingGaze.Value)
                    {
                        HandleClick(entity);
                    }
                }
            }
        }
        #endregion


        #region PRIVATE_METHODS
        /// <summary>
        /// Check if there's 
        /// </summary>
        void CheckResetClick(Filter entity)
        {
            if (entity.ScriptableSingletons.GazeParameters.UseGaze && !entity.ScriptableSingletons.InputsContainer.GazeIsCliking.Value && entity.ScriptableSingletons.InteractionsContainer.HasClickSomethingGaze.Value)
                entity.ScriptableSingletons.InteractionsContainer.HasClickSomethingGaze.SetValue(false);
        }

        /// <summary>
        /// Handle the raycastHits to check if one object was clicked
        /// </summary>
        /// <param name="hits">The list of RaycastHits to check</param>
        /// <param name="hasClicked">the BoolVariable to set if something got clicked</param>
        /// <param name="objectClicked">The GameEvent to raise with the transform of the hit</param>
        private void HandleClick(Filter entity)
        {
            //If nothing is hit, we set the isOver value to false
            if (entity.ScriptableSingletons.InteractionsContainer.GazeHit.IsNull)
            {
                entity.ScriptableSingletons.InteractionsContainer.HasClickSomethingGaze.SetValue(false);
            }
            else
            {
                entity.ScriptableSingletons.InteractionsContainer.HasClickSomethingGaze.SetValue(true);

                var objectClicked = entity.ScriptableSingletons.InteractionsContainer.GazeHit.Value.collider.transform;
                new ObjectWasClickedEvent(EHand.GAZE, objectClicked);
            }
        }
        #endregion PRIVATE_METHODS
    }
}