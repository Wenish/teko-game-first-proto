using UnityEngine;

[CreateAssetMenu(menuName = "Game/Frog Player Settings", fileName = "FrogPlayerSettings")]
public class FrogPlayerSettings : ScriptableObject
{
    [Header("Jump Charge")]
    [Min(0.05f)] public float chargeCycleSeconds = 1.2f;

    [Header("Jump Impulse")]
    [Min(0f)] public float minUpwardJumpForce = 6f;
    [Min(0f)] public float maxUpwardJumpForce = 14f;
    [Min(0f)] public float directionalForceMultiplier = 0.9f;
    [Min(0f)] public float directionDeadzone = 0.1f;

    [Header("Gravity")]
    [Min(0f)] public float gravity = 30f;
    [Min(1f)] public float fallGravityMultiplier = 2.2f;

    [Header("Ground Check")]
    [Min(0.01f)] public float groundCheckDistance = 0.15f;
    [Range(0.1f, 1f)] public float groundCheckRadiusScale = 0.9f;
    public LayerMask groundLayerMask = ~0;
}