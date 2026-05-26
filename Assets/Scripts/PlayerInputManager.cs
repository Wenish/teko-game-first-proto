using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class PlayerInputManager : MonoBehaviour
{
	private FrogInputStateService _frogInputStateService;
	private CameraOrbitInputService _cameraOrbitInputService;
	private Vector2 _lookInput;

	[Inject]
	public void Construct(
		FrogInputStateService frogInputStateService,
		CameraOrbitInputService cameraOrbitInputService)
	{
		_frogInputStateService = frogInputStateService;
		_cameraOrbitInputService = cameraOrbitInputService;
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
		_lookInput += value.Get<Vector2>();
	}

	private void Update()
	{
		if (_frogInputStateService == null || _cameraOrbitInputService == null)
		{
			return;
		}

		float mouseTurnInput = 0f;
		if (Mouse.current != null)
		{
			bool isLeftMousePressed = Mouse.current.leftButton.isPressed;
			_cameraOrbitInputService.SetOrbitPressed(isLeftMousePressed);

			if (isLeftMousePressed)
			{
				_cameraOrbitInputService.AddOrbitLookInput(_lookInput);
			}

			bool isRightMousePressed = Mouse.current.rightButton.isPressed;
			if (isRightMousePressed)
			{
				mouseTurnInput = Mouse.current.delta.ReadValue().x;

				if (Mathf.Approximately(mouseTurnInput, 0f))
				{
					mouseTurnInput = _lookInput.x;
				}
			}
		}
		else
		{
			_cameraOrbitInputService.SetOrbitPressed(false);
			mouseTurnInput = _lookInput.x;
		}

		_frogInputStateService.AddMouseTurnInput(mouseTurnInput);
		_lookInput = Vector2.zero;
	}
}