using System;
using UnityEngine;
using MessagePipe;
using VContainer;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFrogMovement : MonoBehaviour
{
	private FrogPlayerSettings _settings;
	private Rigidbody _rigidbody;
	private IDisposable _jumpReleasedSubscription;

	[Inject]
	public void Construct(
		FrogPlayerSettings settings,
		ISubscriber<FrogJumpReleasedEvent> jumpReleasedSubscriber)
	{
		_settings = settings;
		_jumpReleasedSubscription = jumpReleasedSubscriber.Subscribe(OnJumpReleased);
	}

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_rigidbody.useGravity = false;
	}

	private void FixedUpdate()
	{
		ApplyCustomGravity();
	}

	private void OnDestroy()
	{
		_jumpReleasedSubscription?.Dispose();
	}

	private void OnJumpReleased(FrogJumpReleasedEvent e)
	{
		float clampedCharge = Mathf.Clamp01(e.ChargeNormalized);
		float upwardForce = Mathf.Lerp(
			_settings.minUpwardJumpForce,
			_settings.maxUpwardJumpForce,
			clampedCharge);

		Vector2 planarInput = e.MoveInput;
		Vector3 planarDirection = new Vector3(planarInput.x, 0f, planarInput.y);

		if (planarDirection.sqrMagnitude > (_settings.directionDeadzone * _settings.directionDeadzone))
		{
			planarDirection.Normalize();
		}
		else
		{
			planarDirection = Vector3.zero;
		}

		Vector3 directionalForce = planarDirection * (upwardForce * _settings.directionalForceMultiplier);
		Vector3 velocity = _rigidbody.linearVelocity;
		velocity.y = 0f;
		_rigidbody.linearVelocity = velocity;

		Vector3 jumpImpulse = (Vector3.up * upwardForce) + directionalForce;
		_rigidbody.AddForce(jumpImpulse, ForceMode.VelocityChange);
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
}