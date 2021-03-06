using ScriptableFramework.Variables;
using VRSF.Core.Controllers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRSF.Interactions;
using VRSF.Core.Inputs;
using VRSF.Core.Events;
using VRSF.Core.Utils;
using VRSF.Core.Raycast;

namespace VRSF.UI
{
    /// <summary>
    /// Create a new VRScrollRect based on the ScrollRect for Unity, but usable in VR with the Scriptable Framework
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class VRScrollRect : ScrollRect
    {
        #region PUBLIC_VARIABLES
        [Header("The Direction of this ScrollRect.")]
        [Tooltip("Will be override by the Scrollbar direction if there's at least one.")]
        [SerializeField] public EUIDirection Direction = EUIDirection.TopToBottom;

        [Tooltip("If you want to set the collider yourself, set this value to false.")]
        [SerializeField] public bool SetColliderAuto = true;
        #endregion


        #region PRIVATE_VARIABLES
        private InputVariableContainer _inputContainer;

        private BoolVariable _rightTriggerDown;
        private BoolVariable _leftTriggerDown;

        Transform _minPosBar;
        Transform _maxPosBar;

        ERayOrigin _handHoldingHandle = ERayOrigin.NONE;

        Dictionary<ERayOrigin, RaycastHitVariable> _raycastHitDictionary;

        IUISetupScrollable _scrollableSetup;

        private bool _boxColliderSetup;

        /// <summary>
        /// true when the events for ObjectWasClicked or Hovered were registered.
        /// </summary>
        private bool _eventWereRegistered;
        #endregion


        #region MONOBEHAVIOUR_METHODS
        protected override void Awake()
        {
            base.Awake();

            if (Application.isPlaying)
            {
                SetupUIElement();

                // We setup the BoxCollider size and center
                StartCoroutine(SetupBoxCollider());
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_eventWereRegistered)
            {
                if (verticalScrollbar != null)
                    verticalScrollbar.onValueChanged.RemoveAllListeners();

                if (horizontalScrollbar != null)
                    horizontalScrollbar.onValueChanged.RemoveAllListeners();

                ObjectWasClickedEvent.UnregisterListener(CheckRectClick);
                _eventWereRegistered = false;
            }
        }

        private void Update()
        {
            if (Application.isPlaying && _boxColliderSetup)
            {
                CheckClickDown();

                if (_handHoldingHandle != ERayOrigin.NONE)
                {
                    if (vertical && verticalScrollbar)
                        verticalScrollbar.value = _scrollableSetup.MoveComponent(_handHoldingHandle, _minPosBar, _maxPosBar, _raycastHitDictionary);

                    if (horizontal && horizontalScrollbar)
                        horizontalScrollbar.value = _scrollableSetup.MoveComponent(_handHoldingHandle, _minPosBar, _maxPosBar, _raycastHitDictionary);
                }
            }
        }
        #endregion


        #region PRIVATE_METHODS
        private void SetupUIElement()
        {
            _inputContainer = InputVariableContainer.Instance;

            _rightTriggerDown = _inputContainer.RightClickBoolean.Get("TriggerIsDown");
            _leftTriggerDown = _inputContainer.LeftClickBoolean.Get("TriggerIsDown");

            // We setup the references to the ScrollRect elements
            SetScrollRectReferences();

            // We override the directio selected in the inspector by the scrollbar direction if we use one
            // The vertical direction will always have top priority on the horizontal direction
            if (vertical && verticalScrollbar != null)
            {
                Direction = UnityUIToVRSFUI.ScrollbarDirectionToUIDirection(verticalScrollbar.direction);
                verticalScrollbar.onValueChanged.AddListener(delegate { OnValueChangedCallback(); });
            }
            else if (horizontal && horizontalScrollbar != null)
            {
                Direction = UnityUIToVRSFUI.ScrollbarDirectionToUIDirection(horizontalScrollbar.direction);
                horizontalScrollbar.onValueChanged.AddListener(delegate { OnValueChangedCallback(); });
            }

            _scrollableSetup = new VRUIScrollableSetup(Direction);
            
            ObjectWasClickedEvent.Listeners += CheckRectClick;
            _eventWereRegistered = true;

            // We initialize the _RaycastHitDictionary
            _raycastHitDictionary = new Dictionary<ERayOrigin, RaycastHitVariable>
            {
                { ERayOrigin.RIGHT_HAND, InteractionVariableContainer.Instance.RightHit },
                { ERayOrigin.LEFT_HAND, InteractionVariableContainer.Instance.LeftHit },
                { ERayOrigin.CAMERA, InteractionVariableContainer.Instance.GazeHit },
            };

            // We setup the Min and Max pos transform
            _scrollableSetup.CheckMinMaxGameObjects(transform, Direction);
            _scrollableSetup.SetMinMaxPos(ref _minPosBar, ref _maxPosBar, GetComponent<Transform>());
        }

        /// <summary>
        /// Event called when the user is clicking on something
        /// </summary>
        /// <param name="clickEvent">The event raised when something is clicked</param>
        void CheckRectClick(ObjectWasClickedEvent clickEvent)
        {
            if (clickEvent.ObjectClicked == transform && _handHoldingHandle == ERayOrigin.NONE)
            {
                _handHoldingHandle = clickEvent.RayOrigin;
            }
        }

        /// <summary>
        /// Depending on the hand holding the trigger, call the CheckClickStillDown with the right boolean
        /// </summary>
        void CheckClickDown()
        {
            switch (_handHoldingHandle)
            {
                case ERayOrigin.CAMERA:
                    // _scrollableSetup.CheckClickStillDown(ref _handHoldingHandle, _inputContainer.GazeIsCliking.Value);
                    break;
                case ERayOrigin.LEFT_HAND:
                    _scrollableSetup.CheckClickStillDown(ref _handHoldingHandle, _leftTriggerDown.Value);
                    break;
                case ERayOrigin.RIGHT_HAND:
                    _scrollableSetup.CheckClickStillDown(ref _handHoldingHandle, _rightTriggerDown.Value);
                    break;
            }
        }

        /// <summary>
        /// Start a coroutine that wait for the second frame to set the BoxCollider
        /// </summary>
        /// <returns></returns>
        IEnumerator<WaitForEndOfFrame> SetupBoxCollider()
        {
            yield return new WaitForEndOfFrame();

            if (SetColliderAuto)
            {
                BoxCollider box = GetComponent<BoxCollider>();
                box = VRUIBoxColliderSetup.CheckBoxColliderSize(box, GetComponent<RectTransform>());
                box.center = Vector3.zero;

                if (vertical)
                {
                    var barCollider = verticalScrollbar.GetComponent<BoxCollider>();
                    float x = (box.size.x - barCollider.size.x);
                    box.size = new Vector3(x, box.size.y, box.size.z);
                    box.center = new Vector3(-barCollider.size.x / 2, box.center.y, box.center.z + 0.1f);
                }

                if (horizontal)
                {
                    var barCollider = horizontalScrollbar.GetComponent<BoxCollider>();
                    float y = (box.size.y - barCollider.size.y);
                    box.size = new Vector3(box.size.x, y, box.size.z);
                    box.center = new Vector3(box.center.x, barCollider.size.y / 2, box.center.z + 0.1f);
                }
            }

            _scrollableSetup.CheckContentStatus(viewport, content, vertical, horizontal);
            _boxColliderSetup = true;
        }

        /// <summary>
        /// Set the scrollRect references (scrollbar, content and viewport) by looking in the DeepChildren
        /// </summary>
        void SetScrollRectReferences()
        {
            // get VRScrollBarVertical if we use the scrollrect as vertical
            try
            {
                if (vertical && verticalScrollbar == null)
                    verticalScrollbar = transform.FindDeepChild("VRScrollbarVertical").GetComponent<VRScrollBar>();
            }
            catch { /* No vertical Scrollbar was found. */ }

            // get VRScrollBarHorizontal if we use the scrollrect as horizontal
            try
            {
                if (horizontal && horizontalScrollbar == null)
                    horizontalScrollbar = transform.FindDeepChild("VRScrollbarHorizontal").GetComponent<VRScrollBar>();
            }
            catch { /* No horizontal Scrollbar was found. */ }

            // get the viewport
            try { if (viewport == null) viewport = transform.FindDeepChild("Viewport").GetComponent<RectTransform>(); }
            catch { /* No Viewport was found.*/ }


            // get the content
            try { if (content == null) content = transform.FindDeepChild("Content").GetComponent<RectTransform>(); }
            catch { /* No Content was found.*/ }
        }

        private void OnValueChangedCallback()
        {
            _scrollableSetup.CheckContentStatus(viewport, content, vertical, horizontal);
        }
        #endregion
    }
}