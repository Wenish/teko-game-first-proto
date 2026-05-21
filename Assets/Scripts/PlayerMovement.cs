using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float groundAcceleration = 80f;
    [SerializeField] private float groundDeceleration = 80f;
    [SerializeField] private float airAcceleration = 25f;
    [SerializeField] private float airDeceleration = 8f;
    [SerializeField] private float sprintReleaseAirGrace = 0.15f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 13f;
    [SerializeField] private int maxAirJumps = 1;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float gravity = 35f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundMask = ~0;

    private Rigidbody _rigidbody;
    private Collider _collider;
    private Vector2 _moveInput;
    private bool _isGrounded;
    private int _airJumpsRemaining;
    private float _coyoteTimer;
    private bool _jumpHeld;
    private bool _isSprinting;
    private float _airSprintGraceTimer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _rigidbody.useGravity = false;
        _airJumpsRemaining = maxAirJumps;
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        _isSprinting = value.Get<float>() > 0f;

        if (_isGrounded)
        {
            _airSprintGraceTimer = 0f;
        }
        else if (!_isSprinting)
        {
            _airSprintGraceTimer = sprintReleaseAirGrace;
        }
    }

    public void OnJump(InputValue value)
    {
        _jumpHeld = value.isPressed;

        if (!value.isPressed) return;

        if (_isGrounded || _coyoteTimer > 0f)
        {
            PerformJump();
            _coyoteTimer = 0f;
        }
        else if (_airJumpsRemaining > 0)
        {
            PerformJump();
            _airJumpsRemaining--;
        }
    }

    private void PerformJump()
    {
        _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, jumpForce, _rigidbody.linearVelocity.z);
    }

    private void FixedUpdate()
    {
        CheckGround();
        TickAirSprintGrace();
        ApplyMovement();
        ApplyJumpGravity();
    }

    private void TickAirSprintGrace()
    {
        if (_airSprintGraceTimer > 0f)
        {
            _airSprintGraceTimer -= Time.fixedDeltaTime;
        }
    }

    private void CheckGround()
    {
        float bottomY = _collider != null ? _collider.bounds.min.y + 0.05f : transform.position.y;
        Vector3 rayOrigin = new Vector3(transform.position.x, bottomY, transform.position.z);
        _isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundMask);

        if (_isGrounded)
        {
            _airJumpsRemaining = maxAirJumps;
            _coyoteTimer = coyoteTime;
            if (!_isSprinting)
            {
                _airSprintGraceTimer = 0f;
            }
        }
        else
        {
            _coyoteTimer -= Time.fixedDeltaTime;
        }
    }

    private void ApplyJumpGravity()
    {
        if (_isGrounded && _rigidbody.linearVelocity.y <= 0f)
        {
            return;
        }

        float mult = _rigidbody.linearVelocity.y < 0f ? fallGravityMultiplier
            : !_jumpHeld ? lowJumpMultiplier : 1f;

        _rigidbody.AddForce(Vector3.down * gravity * mult, ForceMode.Acceleration);
    }

    private void ApplyMovement()
    {
        bool hasAirSprintGrace = !_isGrounded && _airSprintGraceTimer > 0f;
        bool sprintActive = _isSprinting || hasAirSprintGrace;
        float speed = moveSpeed * (sprintActive ? sprintMultiplier : 1f);
        Vector3 targetVelocity = new Vector3(_moveInput.x, 0f, _moveInput.y) * speed;
        Vector3 currentHorizontal = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);

        float acceleration;
        if (_isGrounded)
        {
            acceleration = _moveInput.sqrMagnitude > 0f ? groundAcceleration : groundDeceleration;
        }
        else
        {
            float currentSpeed = currentHorizontal.magnitude;
            float targetSpeed = targetVelocity.magnitude;
            bool isAccelerating = targetSpeed > currentSpeed;
            acceleration = isAccelerating ? airAcceleration : airDeceleration;
        }

        Vector3 newHorizontal = Vector3.MoveTowards(currentHorizontal, targetVelocity, acceleration * Time.fixedDeltaTime);

        _rigidbody.linearVelocity = new Vector3(newHorizontal.x, _rigidbody.linearVelocity.y, newHorizontal.z);
    }
}