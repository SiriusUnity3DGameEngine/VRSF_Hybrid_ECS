﻿using UnityEngine;
using UnityEngine.SceneManagement;
using VRSF.Inputs;
using VRSF.MoveAround.Teleport.Components;
using VRSF.MoveAround.Teleport.Interfaces;
using VRSF.Utils;
using VRSF.Utils.Components;
using VRSF.Utils.Components.ButtonActionChoser;
using VRSF.Utils.Systems.ButtonActionChoser;

namespace VRSF.MoveAround.Teleport.Systems
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
            public TeleportGeneralComponent TeleportGeneral;
            public SceneObjectsComponent SceneObjects;
        }

        private Controllers.ControllersParametersVariable _controllersVariable;

        #region ComponentSystem_Methods
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            _controllersVariable = Controllers.ControllersParametersVariable.Instance;
            foreach (var e in GetEntities<Filter>())
            {
                SetupListenersResponses(e);
            }
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();
            foreach (var e in GetEntities<Filter>())
            {
                RemoveListeners(e);
            }
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
        #endregion ComponentSystem_Methods


        #region PUBLIC_METHODS

        #region Listeners_Setup
        public override void SetupListenersResponses(object entity)
        {
            var e = (Filter)entity;
            if ((e.BAC_Comp.InteractionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK)
            {
                e.BAC_Comp.OnButtonStartClicking.AddListener(delegate { OnStartInteractingCallback(e); });
                e.BAC_Comp.OnButtonStopClicking.AddListener(delegate { OnStopInteractingCallback(e); });
            }

            if ((e.BAC_Comp.InteractionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH)
            {
                e.BAC_Comp.OnButtonStartTouching.AddListener(delegate { OnStartInteractingCallback(e); });
                e.BAC_Comp.OnButtonStopTouching.AddListener(delegate { OnStopInteractingCallback(e); });
            }
        }

        public override void RemoveListeners(object entity)
        {
            var e = (Filter)entity;
            if ((e.BAC_Comp.InteractionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK)
            {
                e.BAC_Comp.OnButtonStartClicking.RemoveAllListeners();
                e.BAC_Comp.OnButtonStopClicking.RemoveAllListeners();
            }

            if ((e.BAC_Comp.InteractionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH)
            {
                e.BAC_Comp.OnButtonStartTouching.RemoveAllListeners();
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
            
            if (SBSCalculationsHelper.UserIsOnNavMesh(e, out Vector3 newUsersPos, _controllersVariable.GetExclusionsLayer(e.BAC_Comp.ButtonHand)))
                VRSF_Components.SetCameraRigPosition(newUsersPos);

            e.TeleportGeneral.CurrentTeleportState = ETeleportState.None;
        }
        #endregion

        #endregion


        #region PRIVATE_METHODS
        /// <summary>
        /// Callback for when the user start to interact with the button link to this feature.
        /// Take account of the BAC Timer if the component is attached to this Entity.
        /// </summary>
        /// <param name="e"></param>
        private void OnStartInteractingCallback(Filter e)
        {
            if (e.BAC_Comp.BACTimer != null)
            {
                TeleportUserSystem.SetTeleportState(e.TeleportGeneral, e.SceneObjects, ETeleportState.Selecting);
            }
            // If the user is not aimaing at anything OR is not aiming at the UI, we try to teleport the user
            else if (!e.RayComp.RaycastHitVar.RaycastHitIsOnUI())
            {
                TeleportUser(e);
            }
        }

        /// <summary>
        /// Callback for when the user stop to interact with the button link to this feature.
        /// Take account of the BAC Timer if the component is attached to this Entity.
        /// </summary>
        /// <param name="e"></param>
        private void OnStopInteractingCallback(Filter e)
        {
            if (UserCanTeleport())
                TeleportUser(e);

            bool UserCanTeleport()
            {
                // If we use a BACTimer (if not, teleported on Start Interacting) AND that the timer is ready AND
                // If the user is not aiming at the UI, we try to teleport the user
                return e.BAC_Comp.BACTimer != null && BACTimerUpdateSystem.TimerIsReady(e.BAC_Comp.BACTimer) && !e.RayComp.RaycastHitVar.RaycastHitIsOnUI();
            }
        }

        /// <summary>
        /// Reactivate the System when switching to another Scene.
        /// </summary>
        /// <param name="oldScene">The previous scene before switching</param>
        private void OnSceneUnloaded(Scene oldScene)
        {
            foreach (var e in GetEntities<Filter>())
            {
                SetupListenersResponses(e);
            }
        }
        #endregion
    }
}