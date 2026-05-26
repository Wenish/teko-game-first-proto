using UnityEngine;
using VContainer;

public class CameraOrbitView : MonoBehaviour
{
    [SerializeField]
    private Transform _target;

    private CameraOrbitSettings _settings;
    private CameraOrbitInputService _inputService;
    private float _yaw;
    private float _pitch;
    private bool _isInitialized;

    [Inject]
    public void Construct(
        CameraOrbitSettings settings,
        CameraOrbitInputService inputService,
        PlayerFrogMovement playerFrogMovement)
    {
        _settings = settings;
        _inputService = inputService;

        if (_target == null && playerFrogMovement != null)
        {
            _target = playerFrogMovement.transform;
        }
    }

    private void LateUpdate()
    {
        if (_target == null || _settings == null || _inputService == null)
        {
            return;
        }

        if (!_isInitialized)
        {
            InitializeAngles();
        }

        if (_inputService.IsOrbitPressed)
        {
            Vector2 lookInput = _inputService.ConsumeOrbitLookInput();
            _yaw += lookInput.x * _settings.orbitYawSpeed;
            _pitch -= lookInput.y * _settings.orbitPitchSpeed;
            _pitch = Mathf.Clamp(_pitch, _settings.minPitch, _settings.maxPitch);
        }
        else
        {
            _inputService.ConsumeOrbitLookInput();
            _yaw = Mathf.MoveTowardsAngle(
                _yaw,
                _target.eulerAngles.y,
                _settings.returnYawSpeed * Time.deltaTime);
            _pitch = Mathf.MoveTowards(
                _pitch,
                _settings.defaultPitch,
                _settings.returnPitchSpeed * Time.deltaTime);
        }

        Vector3 targetPosition = _target.position + _settings.targetOffset;
        Quaternion orbitRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desiredPosition = targetPosition - (orbitRotation * Vector3.forward * _settings.distance);

        float positionT = 1f - Mathf.Exp(-_settings.positionLerpSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionT);

        Quaternion desiredLookRotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);
        float lookT = 1f - Mathf.Exp(-_settings.lookLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredLookRotation, lookT);
    }

    private void InitializeAngles()
    {
        Vector3 toCamera = transform.position - (_target.position + _settings.targetOffset);
        if (toCamera.sqrMagnitude < 0.0001f)
        {
            _yaw = _target.eulerAngles.y;
            _pitch = _settings.defaultPitch;
            _isInitialized = true;
            return;
        }

        Vector3 planarToCamera = new Vector3(toCamera.x, 0f, toCamera.z);
        _yaw = planarToCamera.sqrMagnitude > 0.0001f
            ? Mathf.Atan2(planarToCamera.x, planarToCamera.z) * Mathf.Rad2Deg
            : _target.eulerAngles.y;

        float horizontalDistance = planarToCamera.magnitude;
        _pitch = Mathf.Atan2(-toCamera.y, horizontalDistance) * Mathf.Rad2Deg;
        _pitch = Mathf.Clamp(_pitch, _settings.minPitch, _settings.maxPitch);
        _isInitialized = true;
    }
}
