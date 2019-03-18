﻿using Unity.Entities;
using UnityEngine;
using VRSF.Core.Controllers;
using VRSF.Core.Gaze;
using VRSF.Gaze.Inputs;
using VRSF.Utils;

namespace VRSF.Core.Inputs
{
    /// <summary>
    /// Script attached to the SimulatorSDK Prefab.
    /// Set the GameEvent depending on the Keyboard and Mouse Inputs.
    /// You can find a layout of the current mapping in the Wiki of the Repository.
    /// </summary>
    public class SimulatorGazeInputCaptureSystem : ComponentSystem
    {
        /// <summary>
        /// The filter for the entity component.
        /// </summary>
        struct Filter
        {
            public SimulatorGazeInputCaptureComponent GazeInputCapture;
        }

        #region PRIVATE_VARIABLES
        // VRSF Parameters references
        private GazeParametersVariable _gazeParameters;
        private InputVariableContainer _inputContainer;
        #endregion PRIVATE_VARIABLES


        #region ComponentSystem_Methods
        /// <summary>
        /// Called after the scene was loaded, setup the entities variables
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            _gazeParameters = GazeParametersVariable.Instance;
            _inputContainer = InputVariableContainer.Instance;

            foreach (var entity in GetEntities<Filter>())
            {
                CheckGazeClickButton(entity.GazeInputCapture);
            }
        }

        protected override void OnUpdate()
        {
            // If we doesn't use the controllers, we don't check for the inputs.
            if (ControllersParametersVariable.Instance.UseControllers)
            {
                foreach (var entity in GetEntities<Filter>())
                {
                    // If we want to check the gaze interactions
                    if (entity.GazeInputCapture.CheckGazeInteractions)
                        CheckGazeInputs(entity.GazeInputCapture);
                }
            }
        }
        #endregion


        #region PRIVATE_METHODS
        /// <summary>
        /// Handle the Gaze click and Touch button 
        /// </summary>
        void CheckGazeInputs(SimulatorGazeInputCaptureComponent comp)
        {
            // If the gaze boolVariable is at false but the gaze button is clicking
            if (!_inputContainer.GazeIsCliking.Value && 
                Input.GetKeyDown(GazeInteractionSimulator.Dictionnary[new STuples<EControllersButton, EHand>(_gazeParameters.GazeButtonSimulator, EHand.GAZE)]))
            {
                _inputContainer.GazeIsCliking.SetValue(true);
                _inputContainer.GazeIsTouching.SetValue(true);
                new ButtonClickEvent(EHand.GAZE, _gazeParameters.GazeButtonSimulator);
                new ButtonTouchEvent(EHand.GAZE, _gazeParameters.GazeButtonSimulator);
            }
            // If the gaze boolVariable is at true but the gaze button is not clicking
            else if (_inputContainer.GazeIsCliking.Value && 
                Input.GetKeyUp(GazeInteractionSimulator.Dictionnary[new STuples<EControllersButton, EHand>(_gazeParameters.GazeButtonSimulator, EHand.GAZE)]))
            {
                _inputContainer.GazeIsCliking.SetValue(false);
                _inputContainer.GazeIsTouching.SetValue(false);
                new ButtonUnclickEvent(EHand.GAZE, _gazeParameters.GazeButtonSimulator);
                new ButtonUntouchEvent(EHand.GAZE, _gazeParameters.GazeButtonSimulator);
            }
        }


        /// <summary>
        /// Check that the specified Gaze Button in the Gaze Parameters Window was set correctly
        /// </summary>
        private void CheckGazeClickButton(SimulatorGazeInputCaptureComponent comp)
        {
            if (_gazeParameters.GazeButtonSimulator == EControllersButton.NONE)
            {
                comp.CheckGazeInteractions = false;
            }
        }
        #endregion
    }
}