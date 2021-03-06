using VRSF.Core.Controllers;
using System.Collections.Generic;
using UnityEngine;
using VRSF.Core.Events;

namespace VRSF.UI
{
    /// <summary>
    /// Create a new VRToggle based on the Toggle for Unity, but usable in VR with the Scriptable Framework
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class VRToggle : UnityEngine.UI.Toggle
    {
        #region PUBLIC_VARIABLES
        [Tooltip("If you want to set the collider yourself, set this value to false.")]
        [SerializeField] public bool SetColliderAuto = true;

        [Tooltip("If this button can be click using a Raycast and the trigger of your controller.")]
        [SerializeField] public bool LaserClickable = true;

        [Tooltip("If this button can be click using the meshcollider of your controller.")]
        [SerializeField] public bool ControllerClickable = true;
        #endregion PUBLIC_VARIABLES


        #region PRIVATE_VARIABLES
        /// <summary>
        /// true when the events for ObjectWasClicked or Hovered were registered.
        /// </summary>
        private bool _eventWereRegistered;
        #endregion PRIVATE_VARIABLES


        #region MONOBEHAVIOUR_METHODS
        protected override void Awake()
        {
            base.Awake();

            if (Application.isPlaying)
            {
                if (LaserClickable)
                    SetupListener();

                if (ControllerClickable)
                    GetComponent<BoxCollider>().isTrigger = true;

                // We setup the BoxCollider size and center
                if (SetColliderAuto)
                    StartCoroutine(SetupBoxCollider());
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (ObjectWasClickedEvent.IsMethodAlreadyRegistered(CheckToggleClick))
                ObjectWasClickedEvent.UnregisterListener(CheckToggleClick);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ControllerClickable && interactable && other.gameObject.tag.Contains("ControllerBody"))
            {
                isOn = !isOn;
            }
        }
        #endregion MONOBEHAVIOUR_METHODS


        #region PRIVATE_METHODS
        private void SetupListener()
        {
            ObjectWasClickedEvent.RegisterListener(CheckToggleClick);
            _eventWereRegistered = true;
        }

        /// <summary>
        /// Event called when the button is clicked
        /// </summary>
        /// <param name="clickEvent">The event raised when an object is clicked</param>
        void CheckToggleClick(ObjectWasClickedEvent clickEvent)
        {
            if (interactable && clickEvent.ObjectClicked == transform)
            {
                isOn = !isOn;
            }
        }

        /// <summary>
        /// Setup the BoxCOllider size and center by colling the NotScrollableSetup method CheckBoxColliderSize.
        /// We use a coroutine and wait for the end of the first frame as the element cannot be correctly setup on the first frame
        /// </summary>
        /// <returns></returns>
        IEnumerator<WaitForEndOfFrame> SetupBoxCollider()
        {
            yield return new WaitForEndOfFrame();

            BoxCollider box = GetComponent<BoxCollider>();
            box = VRUIBoxColliderSetup.CheckBoxColliderSize(box, GetComponent<RectTransform>());
        }
        #endregion PRIVATE_METHODS
    }
}