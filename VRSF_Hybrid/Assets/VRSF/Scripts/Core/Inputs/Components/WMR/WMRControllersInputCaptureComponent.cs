﻿using ScriptableFramework.Variables;
using UnityEngine;

namespace VRSF.Core.Inputs
{
    /// <summary>
    /// Required to have the references to the controllers parameters and capture the inputs
    /// </summary>
    [RequireComponent(typeof(Unity.Entities.GameObjectEntity))]
    public class WMRControllersInputCaptureComponent : MonoBehaviour
    {
        [HideInInspector] public BoolVariable RightMenuClick;
        [HideInInspector] public BoolVariable LeftMenuClick;
    }
}