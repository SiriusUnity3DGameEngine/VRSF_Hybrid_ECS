﻿using ScriptableFramework.Variables;
using Unity.Entities;
using UnityEngine;
using VRSF.Controllers.Components;

namespace VRSF.Controllers.Systems
{
    public class PointerColorSystems : ComponentSystem
    {
        struct Filter
        {
            public ControllerPointerComponents colorPointerComp;
        }

        #region ComponentSystem_Methods
        // Update is called once per frame
        protected override void OnUpdate()
        {
            foreach (var e in GetEntities<Filter>())
            {
                // As the vive send errors if the controller are not seen on the first frame, we need to put that in the update method
                if (e.colorPointerComp.IsSetup)
                {
                    // If we use the controllers, we check their PointerStates
                    if (e.colorPointerComp.ControllersParameters.UseControllers)
                    {
                        e.colorPointerComp.ControllersParameters.RightPointerState =
                            CheckPointerState(e.colorPointerComp.InteractionContainer.IsOverSomethingRight, e.colorPointerComp.ControllersParameters.RightPointerState, e.colorPointerComp.RightHandPointer, EHand.RIGHT);

                        e.colorPointerComp.ControllersParameters.LeftPointerState =
                            CheckPointerState(e.colorPointerComp.InteractionContainer.IsOverSomethingLeft, e.colorPointerComp.ControllersParameters.LeftPointerState, e.colorPointerComp.LeftHandPointer, EHand.LEFT);
                    }

                    // If we use the Gaze, we check its PointerState
                    if (e.colorPointerComp.GazeParameters.UseGaze)
                    {
                        CheckGazeState(e.colorPointerComp);
                    }
                }
            }
        }
        #endregion ComponentSystem_Methods


        //EMPTY
        #region PUBLIC_METHODS
        #endregion


        #region PRIVATE_METHODS
        /// <summary>
        /// Check if the pointer is touching the UI
        /// </summary>
        /// <param name="isOver">If the Raycast is over something</param>
        /// <param name="pointerState">The current state of the pointer</param>
        /// <param name="pointer">The linerenderer to which the material is attached</param>
        /// <returns>The new state of the pointer</returns>
        private EPointerState CheckPointerState(BoolVariable isOver, EPointerState pointerState, LineRenderer pointer, EHand hand)
        {
            Color on = Color.white;
            Color off = Color.white;
            Color selectable = Color.white;

            GetControllerColor(hand, ref on, ref off, ref selectable);

            // If the pointer is supposed to be off
            if (pointerState == EPointerState.OFF)
            {
                pointer.material.color = off;
                return EPointerState.OFF;
            }
            // If the pointer is not over something and it's state is not On
            else if (!isOver.Value && pointerState != EPointerState.ON)
            {
                pointer.material.color = on;
                return EPointerState.ON;
            }
            // If the pointer is over something and it's state is not at Selectable
            else if (isOver.Value && pointerState != EPointerState.SELECTABLE)
            {
                pointer.material.color = selectable;
                return EPointerState.SELECTABLE;
            }
            return pointerState;
        }


        /// <summary>
        /// Get the color of the Hand pointers by getting the Controllers Parameters
        /// </summary>
        /// <param name="hand">the Hand to check</param>
        /// <param name="on">The color for the On State</param>
        /// <param name="off">The color for the Off State</param>
        /// <param name="selectable">The color for the Selectable State</param>
        private void GetControllerColor(EHand hand, ref Color on, ref Color off, ref Color selectable)
        {
            var controllersParam = ControllersParametersVariable.Instance;
            switch (hand)
            {
                case (EHand.RIGHT):
                    on = controllersParam.ColorMatOnRight;
                    off = controllersParam.ColorMatOffRight;
                    selectable = controllersParam.ColorMatSelectableRight;
                    break;

                case (EHand.LEFT):
                    on = controllersParam.ColorMatOnLeft;
                    off = controllersParam.ColorMatOffLeft;
                    selectable = controllersParam.ColorMatSelectableLeft;
                    break;

                default:
                    Debug.LogError("The hand wasn't specified, setting pointer color to white.");
                    break;
            }
        }

        /// <summary>
        /// Check the color of the gaze depending on the checkGazeStates bool
        /// </summary>
        private void CheckGazeState(ControllerPointerComponents comp)
        {
            // If we use different type of states
            if (comp.CheckGazeStates)
            {
                SetGazeColorState(comp);
            }
            else
            {
                if (comp.GazeBackground != null)
                    comp.GazeBackground.color = comp.GazeParameters.ReticleColor;
                if (comp.GazeBackground != null)
                    comp.GazeTarget.color = comp.GazeParameters.ReticleTargetColor;
            }
        }

        /// <summary>
        /// Set the color of the gaze depending on its state
        /// </summary>
        private void SetGazeColorState(ControllerPointerComponents comp)
        {
            // If the Gaze is supposed to be off
            if (comp.GazeParameters.GazePointerState == EPointerState.OFF)
            {
                if (comp.GazeBackground != null)
                    comp.GazeBackground.color = comp.GazeParameters.ColorOffReticleBackgroud;

                if (comp.GazeTarget != null)
                    comp.GazeTarget.color = comp.GazeParameters.ColorOffReticleTarget;
            }
            // If the Gaze is not over something and it's state is not On
            else if (!comp.InteractionContainer.IsOverSomethingGaze.Value && comp.GazeParameters.GazePointerState != EPointerState.ON)
            {
                if (comp.GazeBackground)
                    comp.GazeBackground.color = comp.GazeParameters.ColorOnReticleBackgroud;

                if (comp.GazeTarget != null)
                    comp.GazeTarget.color = comp.GazeParameters.ColorOnReticleTarget;

                comp.GazeParameters.GazePointerState = EPointerState.ON;
            }
            // If the Gaze is over something and it's state is not at Selectable
            else if (comp.InteractionContainer.IsOverSomethingGaze.Value && comp.GazeParameters.GazePointerState != EPointerState.SELECTABLE)
            {
                if (comp.GazeBackground != null)
                    comp.GazeBackground.color = comp.GazeParameters.ColorSelectableReticleBackgroud;

                if (comp.GazeTarget != null)
                    comp.GazeTarget.color = comp.GazeParameters.ColorSelectableReticleTarget;

                comp.GazeParameters.GazePointerState = EPointerState.SELECTABLE;
            }
        }
        #endregion PRIVATE_METHODS
    }
}