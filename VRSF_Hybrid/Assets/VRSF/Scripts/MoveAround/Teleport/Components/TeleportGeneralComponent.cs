using ScriptableFramework.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace VRSF.MoveAround.Teleport
{
    /// <summary>
    /// Contains all variable necessary for the Teleport Systems to work.
    /// It implements the ITeleportComponent to be able to display the boundaries limits with TeleportBoundaries.
    /// </summary>
    public class TeleportGeneralComponent : MonoBehaviour
    {
        [Header("Is this teleport feature using fade out/in")]
        public bool IsUsingFadingEffect = true;

        [Header("Layer on which we shouldn't check the teleport hit")]
        public LayerMask ExcludedLayers = new LayerMask();

        /// <summary>
        /// RaycastHitVar linked to the hand with which we use this feature
        /// </summary>
        [HideInInspector] public RaycastHitVariable RaycastHitVar;

        /// <summary>
        /// RaycastHitVar linked to the hand with which we use this feature
        /// </summary>
        [HideInInspector] public RayVariable RayVar;

        /// <summary>
        /// Indicates the current use of teleportation.
        /// None: The player is not using teleportation right now
        /// Selecting: The player is currently selecting a teleport destination (holding down on touchpad)
        /// Teleporting: The player has selected a teleport destination and is currently teleporting now (fading in/out)
        /// </summary>
        [HideInInspector] public ETeleportState CurrentTeleportState { get; set; }

        /// <summary>
        /// Whether the user can use the teleport feature
        /// </summary>
        [HideInInspector] public static bool CanTeleport = true;

        /// <summary>
        /// The point where the user needs to be teleported to
        /// </summary>
        [HideInInspector] public static Vector3 PointToGoTo = Vector3.zero;

        [System.NonSerialized] public UnityAction StartInteractingAction;
        [System.NonSerialized] public UnityAction StopInteractingAction;
        [System.NonSerialized] public UnityAction IsInteractingAction;
    }
} 