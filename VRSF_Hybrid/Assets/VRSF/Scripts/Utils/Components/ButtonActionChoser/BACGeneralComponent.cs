﻿using UnityEngine;
using UnityEngine.Events;
using VRSF.Controllers;
using VRSF.Inputs;

namespace VRSF.Utils.Components.ButtonActionChoser
{
    [RequireComponent(typeof(Unity.Entities.GameObjectEntity), typeof(BACCalculationsComponent))]
    public class BACGeneralComponent : MonoBehaviour
    {
        [Header("The type of Interaction you want to use")]
        [HideInInspector] public EControllerInteractionType InteractionType = EControllerInteractionType.NONE;
        
        [Header("The hand on which the button to use is situated")]
        [HideInInspector] public EHand ButtonHand = EHand.NONE;

        [Header("Wheter you want to use the Gaze Click for the Action")]
        [HideInInspector] public bool UseGazeButton = false;

        [Header("The button you wanna use for the Action")]
        [HideInInspector] public EControllersButton ActionButton = EControllersButton.NONE;

        [Header("The position of the Thumb to start this feature")]
        [HideInInspector] public EThumbPosition LeftTouchThumbPosition = EThumbPosition.NONE;
        [HideInInspector] public EThumbPosition RightTouchThumbPosition = EThumbPosition.NONE;
        [HideInInspector] public EThumbPosition LeftClickThumbPosition = EThumbPosition.NONE;
        [HideInInspector] public EThumbPosition RightClickThumbPosition = EThumbPosition.NONE;

        [Header("The Thresholds for the Thumb on the Controller")]
        [HideInInspector] public float TouchThreshold = 0.5f;
        [HideInInspector] public float ClickThreshold = 0.5f;
        
        [Header("The UnityEvents called when the user is Touching")]
        [HideInInspector] public UnityEvent OnButtonStartTouching;
        [HideInInspector] public UnityEvent OnButtonStopTouching;
        [HideInInspector] public UnityEvent OnButtonIsTouching;

        [Header("The UnityEvents called when the user is Clicking")]
        [HideInInspector] public UnityEvent OnButtonStartClicking;
        [HideInInspector] public UnityEvent OnButtonStopClicking;
        [HideInInspector] public UnityEvent OnButtonIsClicking;
    }
}