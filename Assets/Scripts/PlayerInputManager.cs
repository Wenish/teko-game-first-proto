using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class PlayerInputManager : MonoBehaviour
{
	private FrogInputStateService _frogInputStateService;
	private Vector2 _lookInput;

	[Inject]
	public void Construct(FrogInputStateService frogInputStateService)
	{
		_frogInputStateService = frogInputStateService;
	}

	public void OnMove(InputValue value)
	{
		_frogInputStateService.SetMoveInput(value.Get<Vector2>());
	}

	public void OnJump(InputValue value)
	{
		_frogInputStateService.SetJumpPressed(value.isPressed);
	}

	public void OnLook(InputValue value)
	{
		_lookInput = value.Get<Vector2>();
	}

	private void Update()
	{
		if (_frogInputStateService == null)
		{
			return;
		}

		bool isRightMousePressed = Mouse.current != null && Mouse.current.rightButton.isPressed;
		float mouseTurnInput = isRightMousePressed ? _lookInput.x : 0f;
		_frogInputStateService.SetMouseTurnInput(mouseTurnInput);
		_lookInput = Vector2.zero;
	}
}