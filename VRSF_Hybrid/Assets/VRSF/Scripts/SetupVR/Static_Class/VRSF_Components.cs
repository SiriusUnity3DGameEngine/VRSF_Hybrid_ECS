﻿using UnityEngine;
using VRSF.Controllers;

namespace VRSF.Utils
{
    /// <summary>
    /// Contains all the static references for the VRSF Objects
    /// </summary>
	public static class VRSF_Components
    {
        #region PUBLIC_VARIABLES
        [Header("A reference to the CameraRig object")]
        public static GameObject CameraRig;

        [Header("A reference to the Controllers")]
        public static GameObject RightController;
        public static GameObject LeftController;

        [Header("A reference to the Camera")]
        public static GameObject VRCamera;

        [Header("The name of the SDK that has been loaded. Not necessary if you're using a Starting Screen.")]
        public static EDevice DeviceLoaded = EDevice.NULL;
        #endregion


        #region PRIVATE_VARIABLES
        private static bool _setupVRIsReady = false;
        #endregion


        #region PUBLIC_METHODS
        /// <summary>
        /// Setup the VRSF Objects based on the Scripts Container in scene
        /// </summary>
        /// <param name="scriptsContainer">The Transform to copy</param>
        /// <param name="newInstance">The new Instance of the VRSF Object</param>
        public static void SetupTransformFromContainer(Transform scriptsContainer, ref GameObject newInstance)
        {
            if (newInstance.tag.Contains("CameraRig"))
                SetCameraRigPositionOnStart(scriptsContainer.position);

            // We copy the transform values of the scriptsContainer to the newInstance
            newInstance.transform.localScale = scriptsContainer.localScale;
            newInstance.transform.rotation = scriptsContainer.rotation;

            // We set the script container as child of the new Instance object
            scriptsContainer.SetParent(newInstance.transform);
            scriptsContainer.transform.localPosition = Vector3.zero;
            scriptsContainer.transform.localRotation = Quaternion.identity;
            scriptsContainer.transform.localScale = Vector3.one;


            /// <summary>
            /// Method to set the CameraRig position when launching the app.
            /// </summary>
            /// <param name="newWorldPos">The new Pos where the user should be in World coordinate</param>
            void SetCameraRigPositionOnStart(Vector3 newWorldPos)
            {
                CameraRig.transform.position = DeviceLoaded == EDevice.OPENVR ? 
                    (newWorldPos - VRCamera.transform.localPosition) : newWorldPos;
            }
        }

        /// <summary>
        /// Method to set the CameraRig position by taking account of the SDK loaded
        /// We suggest you to give a position situated on a plane, as we're adding the height of the user in Y axis
        /// when setYPos is true and we're not using OpenVR.
        /// </summary>
        /// <param name="newPos">The new Pos where the user should be in World coordinate</param>
        /// <param name="setYPos">Wheter we have to change the Y position</param>
        public static void SetCameraRigPosition(Vector3 newPos, bool setYPos = true, float userHeight = 1.8f)
        {
            //if (setYPos && DeviceLoaded == EDevice.SIMULATOR)
            //    newPos.y += userHeight;

            CameraRig.transform.position = setYPos ? newPos : new Vector3(newPos.x, CameraRig.transform.position.y, newPos.z);
        }
        #endregion
        

        #region GETTERS_SETTERS
        public static bool SetupVRIsReady
        {
            get
            {
                // If we use the controller, we check the object in this class. If not, we only check the VRCamera and the CameraRig. 
                return ControllersParametersVariable.Instance.UseControllers ?
                    (CameraRig != null && VRCamera != null && LeftController != null && RightController != null) :
                    (CameraRig != null && VRCamera != null);
            }


            set => _setupVRIsReady = value;
        }
        #endregion
    }
}