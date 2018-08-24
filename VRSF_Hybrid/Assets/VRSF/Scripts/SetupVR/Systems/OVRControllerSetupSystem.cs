﻿using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRSF.Controllers;
using VRSF.Inputs.Components;

namespace VRSF.Utils.Systems
{
    /// <summary>
    /// System used to set the only controller used by the GearVR and the Oculus GO depending on 
    /// whether the user is left handed or right handed
    /// </summary>
    public class OVRControllerSetupSystem : ComponentSystem
    {
        struct GearVRFilter
        {
            public GearVRControllersInputCaptureComponent GearVRInputs;
        }

        struct OculusGOFilter
        {
            public OculusGoControllersInputCaptureComponent OculusGoInputs;
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected override void OnUpdate()
        {
            if (VRSF_Components.SetupVRIsReady)
            {
                Transform controller = null;

                foreach (var entity in GetEntities<GearVRFilter>())
                {
                    controller = VRSF_Components.RightController.transform.Find("GearVrController");
                }

                foreach (var entity in GetEntities<OculusGOFilter>())
                {
                    controller = VRSF_Components.RightController.transform.Find("OculusGoController");
                }

                if (controller != null && OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote))
                {
                    controller.SetParent(VRSF_Components.LeftController.transform.Find("LeftControllerScripts"));
                    VRSF_Components.RightController.SetActive(false);
                    ControllersParametersVariable.Instance.UsePointerRight = false;
                    this.Enabled = false;
                }
                else if (controller != null && OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote))
                {
                    // Remote is already under the Right Controller
                    VRSF_Components.LeftController.SetActive(false);
                    ControllersParametersVariable.Instance.UsePointerLeft = false;
                    this.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Called when the entity manager destroy the entities.
        /// In this case, put back the default value of the UsePointerLeft/Right from the ControllersParametersVariable, as its a 
        /// Scriptable Object.
        /// </summary>
        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            foreach (var entity in GetEntities<GearVRFilter>())
            {
                if (OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote))
                {
                    ControllersParametersVariable.Instance.UsePointerRight = entity.GearVRInputs._UsingOtherHandPointer;
                }
                else
                {
                    ControllersParametersVariable.Instance.UsePointerLeft = entity.GearVRInputs._UsingOtherHandPointer;
                }
            }

            foreach (var entity in GetEntities<OculusGOFilter>())
            {
                if (OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote))
                {
                    ControllersParametersVariable.Instance.UsePointerRight = entity.OculusGoInputs._UsingOtherHandPointer;
                }
                else
                {
                    ControllersParametersVariable.Instance.UsePointerLeft = entity.OculusGoInputs._UsingOtherHandPointer;
                }
            }
        }


        private void OnSceneUnloaded(Scene oldScene)
        {
            this.Enabled = true;
        }
    }
}