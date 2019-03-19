﻿using UnityEngine;
using VRSF.Core.Controllers;
using VRSF.Core.Inputs;
using VRSF.Core.SetupVR;
using VRSF.Core.Raycast;
using VRSF.Core.Utils.ButtonActionChoser;

namespace VRSF.MoveAround.Teleport
{
    /// <summary>
    /// Using the ButtonActionChoser, this System allow the user to move Step by Step, ie in the direction of its laser to which this feature is linked.
    /// </summary>
    public class StepByStepSystem : BACListenersSetupSystem, ITeleportSystem
    {
        public struct Filter : ITeleportFilter
        {
            public StepByStepComponent SBS_Comp;
            public ScriptableRaycastComponent RayComp;
            public BACGeneralComponent BAC_Comp;
            public SceneObjectsComponent SceneObjects;
            public TeleportGeneralComponent TeleportGeneral;
        }

        #region ComponentSystem_Methods
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected override void OnCreateManager()
        {
            OnSetupVRReady.Listeners += Init;
            base.OnCreateManager();
        }

        protected override void OnDestroyManager()
        {
            OnSetupVRReady.Listeners -= Init;
            base.OnDestroyManager();
            foreach (var e in GetEntities<Filter>())
            {
                RemoveListeners(e);
            }
        }
        #endregion ComponentSystem_Methods


        #region PUBLIC_METHODS

        #region Listeners_Setup
        public override void SetupListenersResponses(object entity)
        {
            var e = (Filter)entity;
            if ((e.BAC_Comp.InteractionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK)
            {
                e.BAC_Comp.OnButtonStopClicking.AddListener(delegate { OnStopInteractingCallback(e); });
            }

            if ((e.BAC_Comp.InteractionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH)
            {
                e.BAC_Comp.OnButtonStopTouching.AddListener(delegate { OnStopInteractingCallback(e); });
            }
        }

        public override void RemoveListeners(object entity)
        {
            var e = (Filter)entity;
            if ((e.BAC_Comp.InteractionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK)
            {
                e.BAC_Comp.OnButtonStopClicking.RemoveAllListeners();
            }

            if ((e.BAC_Comp.InteractionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH)
            {
                e.BAC_Comp.OnButtonStopTouching.RemoveAllListeners();
            }
        }
        #endregion Listeners_Setup


        #region Teleport_Interface
        /// <summary>
        /// Method to call from StopTouching, teleport the user in the direction of its controller.
        /// </summary>
        public void TeleportUser(ITeleportFilter teleportFilter)
        {
            Filter e = (Filter)teleportFilter;
            
            if (SBSCalculationsHelper.UserIsOnNavMesh(e, out Vector3 newUsersPos, ControllersParametersVariable.Instance.GetExclusionsLayer(e.BAC_Comp.ButtonHand)))
                VRSF_Components.SetCameraRigPosition(newUsersPos, false);

            e.TeleportGeneral.CurrentTeleportState = ETeleportState.None;
        }
        #endregion

        #endregion


        #region PRIVATE_METHODS
        /// <summary>
        /// Callback for when the user stop to interact with the button link to this feature.
        /// Take account of the BAC Timer if the component is attached to this Entity.
        /// </summary>
        /// <param name="e"></param>
        private void OnStopInteractingCallback(Filter e)
        {
            if (!e.RayComp.RaycastHitVar.RaycastHitIsOnUI())
                TeleportUser(e);
        }

        /// <summary>
        /// Reactivate the System when we instantiate a new CameraRig
        /// </summary>
        private void Init(OnSetupVRReady setupVRReady)
        {
            foreach (var e in GetEntities<Filter>())
            {
                SetupListenersResponses(e);
            }
        }
        #endregion
    }
}