﻿#if UNITY_EDITOR
using UnityEditor;
using VRSF.Core.Controllers;
using VRSF.Core.Gaze;
using VRSF.Core.Inputs;
using VRSF.Utils.ButtonActionChoser;

namespace VRSF.Core.Utils.Editor
{
    /// <summary>
    /// Handle the Options in the Inspector for the class that extend ButtonActionChoser 
    /// </summary>
    [CustomEditor(typeof(BACGeneralComponent), true)]
    public class BACComponentsEditor : UnityEditor.Editor
    {
        // EMPTY
        #region PUBLIC_VARIABLES

        #endregion


        #region PRIVATE_VARIABLES
        private GazeParametersVariable _gazeParameters;
        private ControllersParametersVariable _controllersParameters;

        // let us know if we are showing the left and right thumb position parameters
        private bool _leftThumbPosIsShown;
        private bool _rightThumbPosIsShown;

        // The reference to the target
        private BACGeneralComponent _buttonActionChoser;
        private BACCalculationsComponent _bacCalculations;

        private bool _showUnityEvents = false;

        // The References for the UnityEvents
        private SerializedProperty _onButtonStartTouchingProperty;
        private SerializedProperty _onButtonStopTouchingProperty;
        private SerializedProperty _onButtonIsTouchingProperty;

        private SerializedProperty _onButtonStartClickingProperty;
        private SerializedProperty _onButtonStopClickingProperty;
        private SerializedProperty _onButtonIsClickingProperty;
        #endregion


        #region PUBLIC_METHODS
        public virtual void OnEnable()
        {
            // We set the buttonActionChoser reference
            _buttonActionChoser = (BACGeneralComponent)target;
            _bacCalculations = _buttonActionChoser.GetComponent<BACCalculationsComponent>();

            _gazeParameters = GazeParametersVariable.Instance;
            _controllersParameters = ControllersParametersVariable.Instance;

            _onButtonStartTouchingProperty = serializedObject.FindProperty("OnButtonStartTouching");
            _onButtonStopTouchingProperty = serializedObject.FindProperty("OnButtonStopTouching");
            _onButtonIsTouchingProperty = serializedObject.FindProperty("OnButtonIsTouching");

            _onButtonStartClickingProperty = serializedObject.FindProperty("OnButtonStartClicking");
            _onButtonStopClickingProperty = serializedObject.FindProperty("OnButtonStopClicking");
            _onButtonIsClickingProperty = serializedObject.FindProperty("OnButtonIsClicking");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.DrawDefaultInspector();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Button Action Choser Parameters", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();

            // We check that the user has set a good value for the Interaction Type. if not, we don't display the rest of the parameters.
            if (!DisplayInteractionTypeParameters()) return;

            EditorGUILayout.Space();

            // We check that the user has set a good value for the Interaction Type. if not, we don't display the rest of the parameters.
            if (!DisplayHandParameters()) return;

            EditorGUILayout.Space();

            if (_gazeParameters.UseGaze && _controllersParameters.UseControllers)
            {
                EditorGUILayout.LabelField("If you want to use the Gaze Button. Specified in Window/VRSF/Gaze Parameters.", EditorStyles.miniBoldLabel);
                _buttonActionChoser.UseGazeButton = EditorGUILayout.Toggle("Use Gaze Button", _buttonActionChoser.UseGazeButton);
            }
            else
            {
                _buttonActionChoser.UseGazeButton = false;
            }

            if (_buttonActionChoser.UseGazeButton)
            {
                DisplayGazeInfo();
            }
            else
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("The button you wanna use for this feature", EditorStyles.miniBoldLabel);
                _buttonActionChoser.ActionButton = (EControllersButton)EditorGUILayout.EnumPopup("Button to use", _buttonActionChoser.ActionButton);

                EditorGUILayout.Space();

                switch (_buttonActionChoser.InteractionType)
                {
                    case EControllerInteractionType.TOUCH:
                        HandleTouchDisplay();
                        break;

                    case EControllerInteractionType.CLICK:
                        HandleClickDisplay();
                        break;

                    case EControllerInteractionType.ALL:
                        HandleTouchDisplay();
                        EditorGUILayout.Space();
                        HandleClickDisplay();
                        break;

                    default:
                        _bacCalculations.ParametersAreInvalid = true;
                        _buttonActionChoser.ActionButton = EControllersButton.NONE;
                        EditorGUILayout.HelpBox("Please chose a valid Interaction Type.", MessageType.Error);
                        break;
                }
            }

            CheckThumbPos();

            DisplayInteractionEvents();

            EditorUtility.SetDirty(_buttonActionChoser);

            serializedObject.ApplyModifiedProperties();
        }

        private void HandleTouchDisplay()
        {
            switch (_buttonActionChoser.ActionButton)
            {
                case EControllersButton.A_BUTTON:
                case EControllersButton.B_BUTTON:
                case EControllersButton.X_BUTTON:
                case EControllersButton.Y_BUTTON:
                case EControllersButton.THUMBREST:
                    DisplayOculusWarning();
                    _bacCalculations.ParametersAreInvalid = false;
                    _leftThumbPosIsShown = false;
                    _rightThumbPosIsShown = false;
                    break;
                    
                case EControllersButton.THUMBSTICK:
                    DisplayThumbPosition(EControllerInteractionType.TOUCH, _buttonActionChoser.ButtonHand);
                    break;

                case EControllersButton.NONE:
                    _bacCalculations.ParametersAreInvalid = true;
                    _leftThumbPosIsShown = false;
                    _rightThumbPosIsShown = false;
                    EditorGUILayout.HelpBox("Touch : Please chose a valid Action Button.", MessageType.Error);
                    break;

                default:
                    _bacCalculations.ParametersAreInvalid = true;
                    _leftThumbPosIsShown = false;
                    _rightThumbPosIsShown = false;
                    DisplayTouchError();
                    break;
            }
        }

        private void HandleClickDisplay()
        {
            switch (_buttonActionChoser.ActionButton)
            {
                case EControllersButton.A_BUTTON:
                case EControllersButton.B_BUTTON:
                case EControllersButton.X_BUTTON:
                case EControllersButton.Y_BUTTON:
                    DisplayOculusWarning();
                    _bacCalculations.ParametersAreInvalid = false;
                    _leftThumbPosIsShown = false;
                    _rightThumbPosIsShown = false;
                    break;

                case EControllersButton.MENU:
                    if (_buttonActionChoser.ButtonHand == EHand.RIGHT)
                    {
                        DisplayViveWarning();
                        _bacCalculations.ParametersAreInvalid = false;
                        _leftThumbPosIsShown = false;
                        _rightThumbPosIsShown = false;
                    }
                    break;

                case EControllersButton.WHEEL_BUTTON:
                    DisplaySimulatorWarning();
                    _bacCalculations.ParametersAreInvalid = false;
                    _leftThumbPosIsShown = false;
                    _rightThumbPosIsShown = false;
                    break;

                case EControllersButton.THUMBREST:
                    DisplayClickError();
                    _bacCalculations.ParametersAreInvalid = true;
                    _leftThumbPosIsShown = false;
                    _rightThumbPosIsShown = false;
                    break;

                case EControllersButton.THUMBSTICK:
                    DisplayThumbPosition(EControllerInteractionType.CLICK, _buttonActionChoser.ButtonHand);
                    break;

                case EControllersButton.NONE:
                    _bacCalculations.ParametersAreInvalid = true;
                    _leftThumbPosIsShown = false;
                    _rightThumbPosIsShown = false;
                    EditorGUILayout.HelpBox("Click : Please chose a valid Action Button.", MessageType.Error);
                    break;

                default:
                    _bacCalculations.ParametersAreInvalid = false;
                    _leftThumbPosIsShown = false;
                    _rightThumbPosIsShown = false;
                    break;
            }
        }

        private void DisplayOculusWarning()
        {
            EditorGUILayout.HelpBox("This feature will only be available for the Oculus Touch Controllers, " +
                "as the A, B, X, Y button and the Thumbrests doesn't exist on the Vive Controllers.", MessageType.Warning);
        }

        private void DisplayViveWarning()
        {
            EditorGUILayout.HelpBox("This feature will only be available for the Vive Controllers, " +
                "as the Right Menu button cannot be use with the Oculus Touch Controllers.", MessageType.Warning);
        }

        private void DisplaySimulatorWarning()
        {
            EditorGUILayout.HelpBox("The Wheel Button will only be available for the Simulator, " +
                "as the Wheel Button is on the Mouse.", MessageType.Warning);
        }

        private void DisplayTouchError()
        {
            EditorGUILayout.HelpBox("This Button cannot be use for the Touch Interaction, as it is not available " +
                "on the Vive Controller and Oculus Touch Controllers.", MessageType.Error);
        }

        private void DisplayClickError()
        {
            EditorGUILayout.HelpBox("This Button cannot be use for the Click Interaction, as it is not available " +
                "on the Vive Controller and Oculus Touch Controllers.", MessageType.Error);
        }


        private void DisplayThumbPosition(EControllerInteractionType interactionType, EHand hand)
        {
            switch (interactionType)
            {
                case EControllerInteractionType.CLICK:
                    switch (hand)
                    {
                        case EHand.LEFT:
                            EditorGUILayout.LabelField("Left Thumb Position to use for this feature", EditorStyles.miniBoldLabel);
                            _buttonActionChoser.LeftClickThumbPosition = (EThumbPosition)EditorGUILayout.EnumFlagsField("Thumb Click Position", _buttonActionChoser.LeftClickThumbPosition);
                            _leftThumbPosIsShown = true;
                            break;

                        case EHand.RIGHT:
                            EditorGUILayout.LabelField("Right Thumb Position to use for this feature", EditorStyles.miniBoldLabel);
                            _buttonActionChoser.RightClickThumbPosition = (EThumbPosition)EditorGUILayout.EnumFlagsField("Thumb Click Position", _buttonActionChoser.RightClickThumbPosition);
                            _rightThumbPosIsShown = true;
                            break;
                    }
                    _buttonActionChoser.ClickThreshold = EditorGUILayout.Slider("Click Detection Threshold", _buttonActionChoser.ClickThreshold, 0.0f, 1.0f);
                    break;

                case EControllerInteractionType.TOUCH:
                    switch (hand)
                    {
                        case EHand.LEFT:
                            EditorGUILayout.LabelField("Left Thumb Position to use for this feature", EditorStyles.miniBoldLabel);
                            _buttonActionChoser.LeftTouchThumbPosition = (EThumbPosition)EditorGUILayout.EnumFlagsField("Thumb Touch Position", _buttonActionChoser.LeftTouchThumbPosition);
                            _leftThumbPosIsShown = true;
                            break;

                        case EHand.RIGHT:
                            EditorGUILayout.LabelField("Right Thumb Position to use for this feature", EditorStyles.miniBoldLabel);
                            _buttonActionChoser.RightTouchThumbPosition = (EThumbPosition)EditorGUILayout.EnumFlagsField("Thumb Touch Position", _buttonActionChoser.RightTouchThumbPosition);
                            _rightThumbPosIsShown = true;
                            break;

                    }
                    _buttonActionChoser.TouchThreshold = EditorGUILayout.Slider("Touch Detection Threshold", _buttonActionChoser.TouchThreshold, 0.0f, 1.0f);
                    break;
            }
        }

        private void DisplayGazeInfo()
        {
            _bacCalculations.ParametersAreInvalid = false;
            EditorGUILayout.HelpBox("You can set the button to use for the Gaze in the Gaze Parameters window (Window/VRSF/Gaze Parameters.", MessageType.Info);
        }


        private bool DisplayInteractionTypeParameters()
        {
            EditorGUILayout.LabelField("Type of Interaction with the Controller", EditorStyles.miniBoldLabel);
            _buttonActionChoser.InteractionType = (EControllerInteractionType)EditorGUILayout.EnumFlagsField("Interaction Type", _buttonActionChoser.InteractionType);

            if (_buttonActionChoser.InteractionType == EControllerInteractionType.NONE)
            {
                _bacCalculations.ParametersAreInvalid = true;
                EditorGUILayout.HelpBox("The Interaction type cannot be NONE.", MessageType.Error);
                return false;
            }
            return true;
        }


        private bool DisplayHandParameters()
        {
            EditorGUILayout.LabelField("Hand using this feature", EditorStyles.miniBoldLabel);
            _buttonActionChoser.ButtonHand = (EHand)EditorGUILayout.EnumPopup("Hand", _buttonActionChoser.ButtonHand);

            if (_buttonActionChoser.ButtonHand == EHand.NONE)
            {
                _bacCalculations.ParametersAreInvalid = true;
                EditorGUILayout.HelpBox("The Hand cannot be NONE.", MessageType.Error);
                return false;
            }
            return true;
        }


        private void CheckThumbPos()
        {
            if (_leftThumbPosIsShown)
            {
                if ((_buttonActionChoser.InteractionType == EControllerInteractionType.CLICK && _buttonActionChoser.LeftClickThumbPosition == EThumbPosition.NONE) ||
                    (_buttonActionChoser.InteractionType == EControllerInteractionType.TOUCH && _buttonActionChoser.LeftTouchThumbPosition == EThumbPosition.NONE) ||
                    (_buttonActionChoser.InteractionType == EControllerInteractionType.ALL &&
                        (_buttonActionChoser.LeftTouchThumbPosition == EThumbPosition.NONE || _buttonActionChoser.LeftClickThumbPosition == EThumbPosition.NONE)))
                {
                    _bacCalculations.ParametersAreInvalid = true;
                    EditorGUILayout.HelpBox("Please chose a valid Thumb Position.", MessageType.Error);
                }
                else
                {
                    _bacCalculations.ParametersAreInvalid = false;
                }
            }
            else if (_rightThumbPosIsShown)
            {
                if ((_buttonActionChoser.InteractionType == EControllerInteractionType.CLICK && _buttonActionChoser.RightClickThumbPosition == EThumbPosition.NONE) ||
                    (_buttonActionChoser.InteractionType == EControllerInteractionType.TOUCH && _buttonActionChoser.RightTouchThumbPosition == EThumbPosition.NONE) ||
                    (_buttonActionChoser.InteractionType == EControllerInteractionType.ALL &&
                        (_buttonActionChoser.RightTouchThumbPosition == EThumbPosition.NONE || _buttonActionChoser.RightClickThumbPosition == EThumbPosition.NONE)))
                {
                    _bacCalculations.ParametersAreInvalid = true;
                    EditorGUILayout.HelpBox("Please chose a valid Thumb Position.", MessageType.Error);
                }
                else
                {
                    _bacCalculations.ParametersAreInvalid = false;
                }
            }
        }


        private void DisplayInteractionEvents()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            _showUnityEvents = EditorGUILayout.ToggleLeft("Display UnityEvents for Click/Touch Events", _showUnityEvents);

            if (_showUnityEvents)
            {
                if ((_buttonActionChoser.InteractionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH)
                {
                    EditorGUILayout.Space();
                    DisplayTouchEvents();
                }
                if ((_buttonActionChoser.InteractionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK)
                {
                    EditorGUILayout.Space();
                    DisplayClickEvents();
                }
            }
        }


        private void DisplayTouchEvents()
        {
            EditorGUILayout.PropertyField(_onButtonStartTouchingProperty);
            EditorGUILayout.PropertyField(_onButtonStopTouchingProperty);
            EditorGUILayout.PropertyField(_onButtonIsTouchingProperty);
        }


        private void DisplayClickEvents()
        {
            EditorGUILayout.PropertyField(_onButtonStartClickingProperty);
            EditorGUILayout.PropertyField(_onButtonStopClickingProperty);
            EditorGUILayout.PropertyField(_onButtonIsClickingProperty);
        }
        #endregion


        // EMPTY
        #region PRIVATE_METHODS

        #endregion


        // EMPTY
        #region GETTERS_SETTERS

        #endregion
    }
}
#endif