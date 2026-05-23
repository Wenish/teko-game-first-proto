using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class PlayerInputManager : MonoBehaviour
{
	private FrogInputStateService _frogInputStateService;

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
}