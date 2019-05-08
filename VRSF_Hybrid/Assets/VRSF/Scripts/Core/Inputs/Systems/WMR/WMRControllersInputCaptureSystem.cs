﻿using Unity.Entities;
using UnityEngine;
using VRSF.Core.Controllers;
using VRSF.Core.SetupVR;

namespace VRSF.Core.Inputs
{
    public class WMRControllersInputCaptureSystem : ComponentSystem
    {
        private struct Filter
        {
            public WMRControllersInputCaptureComponent WMRControllersInput;
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
                    {
                        // We check the Input for the Right controller
                        CheckRightControllerInput(e.WMRControllersInput);

                        // We check the Input for the Left controller
                        CheckLeftControllerInput(e.WMRControllersInput);
                    }
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
        /// Handle the Right Controller input and put them in the Events
        /// </summary>
        private void CheckRightControllerInput(WMRControllersInputCaptureComponent inputCapture)
        {
            #region MENU
            if (Input.GetButtonDown("WMRMenuRight"))
            {
                inputCapture.RightMenuClick.SetValue(true);
                new ButtonUnclickEvent(EHand.RIGHT, EControllersButton.MENU);
            }
            else if (Input.GetButtonUp("WMRGripClickRight"))
            {
                inputCapture.RightMenuClick.SetValue(false);
                new ButtonUnclickEvent(EHand.RIGHT, EControllersButton.MENU);
            }
            #endregion MENU
        }

        /// <summary>
        /// Handle the Left Controller input and put them in the Events
        /// </summary>
        private void CheckLeftControllerInput(WMRControllersInputCaptureComponent inputCapture)
        {
            #region MENU
            if (Input.GetButtonDown("WMRMenuLeft"))
            {
                inputCapture.LeftMenuClick.SetValue(true);
                new ButtonUnclickEvent(EHand.LEFT, EControllersButton.MENU);
            }
            else if (Input.GetButtonUp("WMRGripClickLeft"))
            {
                inputCapture.LeftMenuClick.SetValue(false);
                new ButtonUnclickEvent(EHand.LEFT, EControllersButton.MENU);
            }
            #endregion MENU
        }

        private void CheckDevice(OnSetupVRReady info)
        {
            this.Enabled = VRSF_Components.DeviceLoaded == EDevice.WMR;
        }
        #endregion PRIVATE_METHODS
    }
}