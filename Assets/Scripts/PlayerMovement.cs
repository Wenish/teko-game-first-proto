using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody _rigidbody;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0f, _moveInput.y);
        Vector3 velocity = direction * moveSpeed;

        _rigidbody.linearVelocity = new Vector3(
            velocity.x,
            _rigidbody.linearVelocity.y,
            velocity.z
        );
    }
}