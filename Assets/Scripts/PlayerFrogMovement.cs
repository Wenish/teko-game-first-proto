using System;
using UnityEngine;
using MessagePipe;
using VContainer;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerFrogMovement : MonoBehaviour
{
	private FrogPlayerSettings _settings;
	private FrogInputStateService _inputStateService;
	private FrogGroundStateService _groundStateService;
	private IFrogChargeStateReader _frogChargeStateReader;
	private Rigidbody _rigidbody;
	private Collider _collider;
	private IDisposable _jumpReleasedSubscription;
	private Vector3 _airborneMoveDirection;
	private bool _wasGrounded;
	private bool _suppressGroundControlAfterJump;
	private bool _hasLeftGroundSinceJump;
	private const float GroundedUpwardVelocityTolerance = 0.05f;

	[Inject]
	public void Construct(
		FrogPlayerSettings settings,
		FrogInputStateService inputStateService,
		FrogGroundStateService groundStateService,
		IFrogChargeStateReader frogChargeStateReader,
		ISubscriber<FrogJumpReleasedEvent> jumpReleasedSubscriber)
	{
		_settings = settings;
		_inputStateService = inputStateService;
		_groundStateService = groundStateService;
		_frogChargeStateReader = frogChargeStateReader;
		_jumpReleasedSubscription = jumpReleasedSubscriber.Subscribe(OnJumpReleased);
	}

	private void Awake()
	{
		if (_settings == null)
		{
			enabled = false;
			return;
		}

		_rigidbody = GetComponent<Rigidbody>();
		_collider = GetComponent<Collider>();
		_rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		_rigidbody.useGravity = false;
		_airborneMoveDirection = GetPlanarDirection(transform.forward);
		_wasGrounded = IsGroundedForMovement();
	}

	private void FixedUpdate()
	{
		bool isGrounded = IsGroundedForMovement();

		if (_suppressGroundControlAfterJump && !_hasLeftGroundSinceJump && !isGrounded)
		{
			_hasLeftGroundSinceJump = true;
		}

		if (!isGrounded && _wasGrounded)
		{
			_airborneMoveDirection = GetPlanarDirection(transform.forward);
		}

		_groundStateService.SetIsGrounded(isGrounded);
		TickMovement(isGrounded);
		ApplyCustomGravity();

		// Clear jump lock AFTER movement so the landing frame itself is still suppressed.
		if (_suppressGroundControlAfterJump && _hasLeftGroundSinceJump && isGrounded)
		{
			_suppressGroundControlAfterJump = false;
			_hasLeftGroundSinceJump = false;
		}

		_wasGrounded = isGrounded;
	}

	private void OnDestroy()
	{
		_jumpReleasedSubscription?.Dispose();
	}

	private void OnJumpReleased(FrogJumpReleasedEvent e)
	{
		if (!IsGroundedForMovement())
		{
			return;
		}

		float clampedCharge = Mathf.Clamp01(e.ChargeNormalized);
		float upwardForce = Mathf.Lerp(
			_settings.minUpwardJumpForce,
			_settings.maxUpwardJumpForce,
			clampedCharge);
		_airborneMoveDirection = GetPlanarDirection(transform.forward);
		Vector3 directionalForce = _airborneMoveDirection * (upwardForce * _settings.directionalForceMultiplier);
		Vector3 velocity = _rigidbody.linearVelocity;
		velocity.y = 0f;
		_rigidbody.linearVelocity = velocity;

		Vector3 jumpImpulse = (Vector3.up * upwardForce) + directionalForce;
		_rigidbody.AddForce(jumpImpulse, ForceMode.VelocityChange);
		_suppressGroundControlAfterJump = true;
		_hasLeftGroundSinceJump = false;
	}

	private void TickMovement(bool isGrounded)
	{
		if (isGrounded && _suppressGroundControlAfterJump)
		{
			_inputStateService.ConsumeMouseTurnInput();
			return;
		}

		bool isCharging = _frogChargeStateReader != null
			&& _frogChargeStateReader.IsCharging.CurrentValue;

		if (isGrounded && isCharging)
		{
			_inputStateService.ConsumeMouseTurnInput();

			Vector3 velocity = _rigidbody.linearVelocity;
			velocity.x = 0f;
			velocity.z = 0f;
			_rigidbody.linearVelocity = velocity;
			return;
		}

		Vector2 moveInput = _inputStateService.MoveInput.CurrentValue;
		float mouseTurnInput = _inputStateService.ConsumeMouseTurnInput();

		if (Mathf.Abs(mouseTurnInput) < _settings.mouseTurnInputDeadzone)
		{
			mouseTurnInput = 0f;
		}

		float turnInput = moveInput.x;
		float moveInputForward = moveInput.y;

		if (Mathf.Abs(moveInputForward) < _settings.directionDeadzone)
		{
			moveInputForward = 0f;
		}

		if (Mathf.Abs(turnInput) < _settings.directionDeadzone)
		{
			turnInput = 0f;
		}

		float turnDegrees = (turnInput * _settings.groundTurnSpeed * Time.fixedDeltaTime)
			+ (mouseTurnInput * GetMouseTurnSensitivity());

		if (!Mathf.Approximately(turnDegrees, 0f))
		{
			Quaternion deltaRotation = Quaternion.Euler(
				0f,
				turnDegrees,
				0f);
			_rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);
		}

		if (isGrounded)
		{
			Vector3 velocity = _rigidbody.linearVelocity;
			Vector3 planarVelocity = transform.forward * (moveInputForward * _settings.groundMoveSpeed);
			velocity.x = planarVelocity.x;
			velocity.z = planarVelocity.z;
			_rigidbody.linearVelocity = velocity;
			return;
		}

		// Keep jump trajectory deterministic: no forward/back acceleration while airborne.
		return;
	}

	private void ApplyCustomGravity()
	{
		float gravityMultiplier = _rigidbody.linearVelocity.y < 0f
			? _settings.fallGravityMultiplier
			: 1f;

		_rigidbody.AddForce(
			Vector3.down * _settings.gravity * gravityMultiplier,
			ForceMode.Acceleration);
	}

	private float GetMouseTurnSensitivity()
	{
		return _settings.mouseTurnSensitivity * _settings.RuntimeMouseTurnMultiplier;
	}

	private bool IsGrounded()
	{
		Vector3 boundsCenter = _collider.bounds.center;
		Vector3 boundsExtents = _collider.bounds.extents;

		Vector3 halfExtents = new Vector3(
			boundsExtents.x * _settings.groundCheckRadiusScale,
			0.01f,
			boundsExtents.z * _settings.groundCheckRadiusScale);

		float castDistance = boundsExtents.y + _settings.groundCheckDistance;

		return Physics.BoxCast(
			boundsCenter,
			halfExtents,
			Vector3.down,
			out _,
			Quaternion.identity,
			castDistance,
			_settings.groundLayerMask,
			QueryTriggerInteraction.Ignore);
	}

	private bool IsGroundedForMovement()
	{
		if (!IsGrounded())
		{
			return false;
		}

		return _rigidbody.linearVelocity.y <= GroundedUpwardVelocityTolerance;
	}

	private Vector3 GetPlanarDirection(Vector3 source)
	{
		Vector3 planar = new Vector3(source.x, 0f, source.z);
		if (planar.sqrMagnitude > 0.0001f)
		{
			return planar.normalized;
		}

		Vector3 fallbackForward = transform.forward;
		Vector3 fallbackPlanar = new Vector3(fallbackForward.x, 0f, fallbackForward.z);
		if (fallbackPlanar.sqrMagnitude > 0.0001f)
		{
			return fallbackPlanar.normalized;
		}

		return Vector3.forward;
	}
}