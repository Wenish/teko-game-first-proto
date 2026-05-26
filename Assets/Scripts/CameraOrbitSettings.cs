using UnityEngine;

[CreateAssetMenu(
    fileName = "CameraOrbitSettings",
    menuName = "Settings/Camera Orbit Settings")]
public class CameraOrbitSettings : ScriptableObject
{
    [Min(0.1f)] public float distance = 7f;
    public Vector3 targetOffset = new(0f, 1.5f, 0f);

    [Min(0f)] public float orbitYawSpeed = 0.15f;
    [Min(0f)] public float orbitPitchSpeed = 0.1f;
    [Min(-89f)] public float minPitch = -25f;
    [Range(-89f, 89f)] public float maxPitch = 55f;

    [Range(-89f, 89f)] public float defaultPitch = 18f;
    [Min(0f)] public float returnYawSpeed = 360f;
    [Min(0f)] public float returnPitchSpeed = 360f;

    [Min(0f)] public float positionLerpSpeed = 12f;
    [Min(0f)] public float lookLerpSpeed = 16f;
}
