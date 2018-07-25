﻿using ScriptableFramework.Variables;
using Unity.Entities;
using UnityEngine;
using VRSF.Controllers.Components;
using VRSF.Interactions;
using VRSF.Utils;

namespace VRSF.Controllers.Systems
{
    /// <summary>
    /// Handle the Length of the Pointer depending on if the raycast is hitting something
    /// </summary>
    public class PointerLengthSystem : ComponentSystem
    {
        struct Filter
        {
            public ControllerPointerComponents ControllerPointerComp;
        }


        #region PRIVATE_VARIABLE
        private ControllersParametersVariable _controllersParameters;
        private InteractionVariableContainer _interactionsContainer;
        #endregion PRIVATE_VARIABLE


        #region ComponentSystem_Methods
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            _controllersParameters = ControllersParametersVariable.Instance;
            _interactionsContainer = InteractionVariableContainer.Instance;
        }

        // Update is called once per frame
        protected override void OnUpdate()
        {
            foreach (var e in GetEntities<Filter>())
            {
                // As the vive send errors if the controller are not seen on the first frame, we need to put that in the update method
                if (e.ControllerPointerComp._IsSetup)
                {
                    if (_controllersParameters.UseControllers)
                    {
                        SetControllerRayLength(_interactionsContainer.LeftHit, VRSF_Components.LeftController, EHand.LEFT, e.ControllerPointerComp);
                        SetControllerRayLength(_interactionsContainer.RightHit, VRSF_Components.RightController, EHand.RIGHT, e.ControllerPointerComp);
                    }
                }
            }
        }
        #endregion ComponentSystem_Methods


        #region PRIVATE_METHODS
        /// <summary>
        /// Set the size of the line renderer depending on the hit from the RayCast.
        /// </summary>
        /// <param name="hit">The RaycastHitVariable containing the RaycastHit for the controller</param>
        /// <param name="controller">The controller GameObject from which the ray started</param>
        /// <param name="hand">The hand rom which we are checking the raycastHit</param>
        private void SetControllerRayLength(RaycastHitVariable hit, GameObject controller, EHand hand, ControllerPointerComponents comp)
        {
            try
            {
                if (!hit.isNull)
                {
                    //Reduce lineRenderer from the controllers position to the object that was hit
                    controller.GetComponent<LineRenderer>().SetPositions(new Vector3[]
                    {
                        new Vector3(0.0f, 0.0f, 0.03f),
                        controller.transform.InverseTransformPoint(hit.Value.point),
                    });
                    return;
                }

                // Checking max distance of Line renderer depending on the Hand Variable
                var maxDistanceLr = (hand == EHand.LEFT
                    ? _controllersParameters.MaxDistancePointerLeft
                    : _controllersParameters.MaxDistancePointerRight);

                //put back lineRenderer to its normal length if nothing was hit
                controller.GetComponent<LineRenderer>().SetPositions(new Vector3[]
                {
                        new Vector3(0.0f, 0.0f, 0.03f),
                        new Vector3(0, 0, maxDistanceLr),
                });
            }
            catch (System.Exception e)
            {
                Debug.Log("VRSF : VR Components not setup yet, waiting for next frame.\n" + e);
            }
        }
        #endregion PRIVATE_METHODS
    }
}