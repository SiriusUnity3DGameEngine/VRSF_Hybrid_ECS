using UnityEngine;
using VRSF.Core.Inputs;
using VRSF.Core.Raycast;
using VRSF.Core.SetupVR;
using VRSF.Core.Utils;
using VRSF.Core.Utils.ButtonActionChoser;

namespace VRSF.MoveAround.Teleport
{
    /// <summary>
    /// Using the ButtonActionChoser, this System allow the user to teleport where the Raycast of his controller is pointing
    /// </summary>
    public class LongRangeTeleportSystem : BACListenersSetupSystem, ITeleportSystem
    {
        struct Filter : ITeleportFilter
        {
            public LongRangeTeleportComponent LRT_Comp;
            public BACGeneralComponent BAC_Comp;
            public TeleportGeneralComponent TeleportGeneral;
            public SceneObjectsComponent SceneObjects;
        }

        #region ComponentSystem_Methods
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected override void OnCreateManager()
        {
            OnSetupVRReady.Listeners += Init;
            base.OnCreateManager();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            foreach (var e in GetEntities<Filter>())
            {
                RemoveListeners(e);
            }
            OnSetupVRReady.Listeners -= Init;
        }
        #endregion ComponentSystem_Methods


        #region PUBLIC_METHODS

        #region Listeners_Setup
        public override void SetupListenersResponses(object entity)
        {
            var e = (Filter)entity;

            if (e.TeleportGeneral.StartInteractingAction == null && e.TeleportGeneral.IsInteractingAction == null && e.TeleportGeneral.StopInteractingAction == null)
            {
                e.TeleportGeneral.StartInteractingAction = delegate { OnStartInteractingCallback(e); };
                e.TeleportGeneral.IsInteractingAction = delegate { OnIsInteractingCallback(e); };
                e.TeleportGeneral.StopInteractingAction = delegate { OnStopInteractingCallback(e); };

                if ((e.BAC_Comp.InteractionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK)
                {
                    e.BAC_Comp.OnButtonStartClicking.AddListenerExtend(e.TeleportGeneral.StartInteractingAction);
                    e.BAC_Comp.OnButtonIsClicking.AddListenerExtend(e.TeleportGeneral.IsInteractingAction);
                    e.BAC_Comp.OnButtonStopClicking.AddListenerExtend(e.TeleportGeneral.StopInteractingAction);
                }

                if ((e.BAC_Comp.InteractionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH)
                {
                    e.BAC_Comp.OnButtonStartTouching.AddListenerExtend(e.TeleportGeneral.StartInteractingAction);
                    e.BAC_Comp.OnButtonIsTouching.AddListenerExtend(e.TeleportGeneral.IsInteractingAction);
                    e.BAC_Comp.OnButtonStopTouching.AddListenerExtend(e.TeleportGeneral.StopInteractingAction);
                }
            }
        }

        public override void RemoveListeners(object entity)
        {
            var e = (Filter)entity;

            if ((e.BAC_Comp.InteractionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK)
            {
                e.BAC_Comp.OnButtonStartClicking.RemoveListenerExtend(e.TeleportGeneral.StartInteractingAction);
                e.BAC_Comp.OnButtonIsClicking.RemoveListenerExtend(e.TeleportGeneral.IsInteractingAction);
                e.BAC_Comp.OnButtonStopClicking.RemoveListenerExtend(e.TeleportGeneral.StopInteractingAction);
            }

            if ((e.BAC_Comp.InteractionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH)
            {
                e.BAC_Comp.OnButtonStartTouching.RemoveListenerExtend(e.TeleportGeneral.StartInteractingAction);
                e.BAC_Comp.OnButtonIsTouching.RemoveListenerExtend(e.TeleportGeneral.IsInteractingAction);
                e.BAC_Comp.OnButtonStopTouching.RemoveListenerExtend(e.TeleportGeneral.StopInteractingAction);
            }
        }
        #endregion Listeners_Setup


        #region Teleport_Interface
        /// <summary>
        /// Method to call from StopTOuching or StopClicking Callback, set the user position to where he's poiting
        /// </summary>
        public void TeleportUser(ITeleportFilter teleportFilter)
        {
            Filter entity = (Filter)teleportFilter;
            new OnTeleportUser(entity.TeleportGeneral, entity.SceneObjects);
        }
        #endregion

        #endregion


        #region PRIVATE_METHODS
        /// <summary>
        /// Method to call from the StartTouching or StartClicking Method, set the Loading Slider values if used.
        /// </summary>
        private void OnStartInteractingCallback(Filter entity)
        {
            // We set the current state as Selecting
            TeleportUserSystem.SetTeleportState(ETeleportState.Selecting, entity.TeleportGeneral);
        }

        /// <summary>
        /// To call from the IsClicking or IsTouching event
        /// </summary>
        private void OnIsInteractingCallback(Filter e)
        {
            // TODO : Check that : I think the mistake is coming from here. The CanTeleport is reset at false in one of the if/else below, and block the teleportUserSystem

            e.LRT_Comp._LoadingTimer += Time.deltaTime;
            
            // If the raycast is hitting something and it's not a UI Element
            if (e.TeleportGeneral.RaycastHitVar.RaycastHitIsNotOnUI())
            {
                TeleportNavMeshHelper.Linecast(e.TeleportGeneral.RayVar.Value.origin, e.TeleportGeneral.RaycastHitVar.Value.point, out bool endOnNavmesh,
                                           e.TeleportGeneral.ExcludedLayers, out TeleportGeneralComponent.PointToGoTo, out Vector3 norm, e.SceneObjects._TeleportNavMesh);

                TeleportGeneralComponent.CanTeleport = e.LRT_Comp.UseLoadingTimer ? (endOnNavmesh && e.LRT_Comp._LoadingTimer > e.LRT_Comp.LoadingTime) : endOnNavmesh;
            }
            else
            {
                TeleportGeneralComponent.CanTeleport = false;
            }
        }


        /// <summary>
        /// Handle the Teleport when the user is releasing the button.
        /// </summary>
        private void OnStopInteractingCallback(Filter entity)
        {
            if (TeleportGeneralComponent.CanTeleport)
                TeleportUser(entity);
            else
                TeleportUserSystem.SetTeleportState(ETeleportState.None, entity.TeleportGeneral);
        }

        /// <summary>
        /// Reactivate the System
        /// </summary>
        private void Init(OnSetupVRReady info)
        {
            foreach (var e in GetEntities<Filter>())
            {
                SetupListenersResponses(e);
            }
        }
        #endregion
    }
}