﻿using ScriptableFramework.Variables;
using Unity.Entities;
using UnityEngine;
using VRSF.Core.Controllers;
using VRSF.Core.SetupVR;

namespace VRSF.Core.Inputs
{
    /// <summary>
    /// System common for the Simulator, Vive and Rift, capture the basic inputs for the right controllers
    /// </summary>
    public class CrossplatformRightInputCaptureSystem : ComponentSystem
    {
        private struct Filter
        {
            public CrossplatformInputCapture InputCapture;
        }

        protected override void OnCreateManager()
        {
            OnSetupVRReady.Listeners += CheckDevice;
            base.OnCreateManager();
        }

        protected override void OnUpdate()
        {
            // If we doesn't use the controllers, we don't check for the inputs.
            if (ControllersParametersVariable.Instance.UseControllers)
            {
                foreach (var e in GetEntities<Filter>())
                {
                    if (e.InputCapture.IsSetup)
                        // We check the Input for the Right controller
                        CheckRightControllerInput(e.InputCapture);
                }
            }
        }

        protected override void OnDestroyManager()
        {
            OnSetupVRReady.Listeners -= CheckDevice;
            base.OnDestroyManager();
        }

        #region PRIVATE_METHODS
        /// <summary>
        /// Handle the Left Controller input and put them in the Events
        /// </summary>
        private void CheckRightControllerInput(CrossplatformInputCapture inputCapture)
        {
            #region TRIGGER
            BoolVariable tempClick = inputCapture.RightParameters.ClickBools.Get("TriggerIsDown");
            BoolVariable tempTouch = inputCapture.RightParameters.TouchBools.Get("TriggerIsTouching");

            // Check Click Events
            if (!tempClick.Value && Input.GetAxis("RightTriggerClick") > 0.95f)
            {
                tempClick.SetValue(true);
                tempTouch.SetValue(false);
                new ButtonClickEvent(EHand.RIGHT, EControllersButton.TRIGGER);
            }
            else if (tempClick.Value && Input.GetAxis("RightTriggerClick") < 0.95f)
            {
                tempClick.SetValue(false);
                new ButtonUnclickEvent(EHand.RIGHT, EControllersButton.TRIGGER);
            }
            // Check Touch Events if user is not clicking
            else if (Input.GetButtonDown("RightTriggerTouch"))
            {
                tempTouch.SetValue(true);
                new ButtonTouchEvent(EHand.RIGHT, EControllersButton.TRIGGER);
            }
            else if (Input.GetButtonUp("RightTriggerTouch"))
            {
                tempTouch.SetValue(false);
                new ButtonUntouchEvent(EHand.RIGHT, EControllersButton.TRIGGER);
            }
            #endregion TRIGGER

            #region TOUCHPAD
            inputCapture.RightParameters.ThumbPosition.SetValue(new Vector2(Input.GetAxis("HorizontalRight"), Input.GetAxis("VerticalRight")));

            tempClick = inputCapture.RightParameters.ClickBools.Get("ThumbIsDown");
            tempTouch = inputCapture.RightParameters.TouchBools.Get("ThumbIsTouching");

            // Check Click Events
            if (Input.GetButtonDown("RightThumbClick"))
            {
                tempClick.SetValue(true);
                new ButtonClickEvent(EHand.RIGHT, EControllersButton.THUMBSTICK);
            }
            else if (Input.GetButtonUp("RightThumbClick"))
            {
                tempClick.SetValue(false);
                new ButtonUnclickEvent(EHand.RIGHT, EControllersButton.THUMBSTICK);
            }
            // Check Touch Events if user is not clicking
            else if (!tempClick.Value && Input.GetButtonDown("RightThumbTouch"))
            {
                tempTouch.SetValue(true);
                new ButtonTouchEvent(EHand.RIGHT, EControllersButton.THUMBSTICK);
            }
            else if (Input.GetButtonUp("RightThumbTouch"))
            {
                tempTouch.SetValue(false);
                new ButtonUntouchEvent(EHand.RIGHT, EControllersButton.THUMBSTICK);
            }
            #endregion TOUCHPAD

            #region GRIP
            tempClick = inputCapture.RightParameters.ClickBools.Get("GripIsDown");

            // Check Click Events
            if (!tempClick.Value && Input.GetAxis("RightGripClick") > 0.95f)
            {
                tempClick.SetValue(true);
                new ButtonClickEvent(EHand.RIGHT, EControllersButton.GRIP);
            }
            else if (tempClick.Value && Input.GetAxis("RightGripClick") == 0)
            {
                tempClick.SetValue(false);
                new ButtonUnclickEvent(EHand.RIGHT, EControllersButton.GRIP);
            }
            #endregion GRIP
        }

        private void CheckDevice(OnSetupVRReady info)
        {
            this.Enabled = VRSF_Components.DeviceLoaded != EDevice.SIMULATOR;
        }
        #endregion PRIVATE_METHODS
    }
}