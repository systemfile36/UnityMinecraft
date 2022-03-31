using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

//StarterAssets ��ũ��Ʈ�� �����Ͽ� ������ �´� ������ �����Ͽ���!

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("DebugMenu Set")]
		public GameObject DebugText;

		[Header("Scroll Value")]
		public float ScrollAxis = 0;
		public float ScrollThreshold = 1f;

		[Header("Click Value")]
		public bool IsLeftClicked = false;
		public bool IsRightClicked = false;

		[Header("Value for UI")]
		public bool IsOnUI = false;

		[Header("Reference of PlayerInput Component")]
		private PlayerInput playerInput;

#if !UNITY_IOS || !UNITY_ANDROID
		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
#endif

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED

		void Awake()
		{
			//PlayerInput ������Ʈ�� �޾Ƽ� ������ �����Ѵ�.
			playerInput = transform.GetComponent<PlayerInput>();
		}

		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		/// <summary>
		/// ����� Ű(F3)Ű�� ������ �� �۵��ϴ� �̺�Ʈ
		/// </summary>
		/// <param name="value"></param>
		public void OnDebugMenu()
		{
			//����� �޴� Ȱ��ȭ ���� ���
			Debug.Log("OnDebugMenu Called");
			if (DebugText != null)
				DebugText.SetActive(!DebugText.activeSelf);
		}
		/// <summary>
		/// ��ũ���� �� ���� float�� �����ϴ� �̺�Ʈ
		/// </summary>
		/// <param name="value"></param>
		public void OnScroll(InputValue value)
		{
			ScrollAxis = value.Get<float>();
		}

		//Ŭ�� ���� ���� ��ȯ
		public void OnLeftClick(InputValue value)
		{
			IsLeftClicked = value.isPressed;
		}
		public void OnRightClick(InputValue value)
		{
			IsRightClicked = value.isPressed;
		}

		/// <summary>
		/// �κ��丮Ű�� ������ ���� ����
		/// </summary>
		/// <param name="value"></param>
		public void OnInventoryToggle(InputValue value)
		{
			Debug.Log("OnInventoryToggle Called | IsOnUI =" + IsOnUI);
			IsOnUI = !IsOnUI;

			//ActionMap�� UI on/off ���ο� ���� ��ȯ
			if(IsOnUI)
			{
				playerInput.SwitchCurrentActionMap("UI");
			}
			else
			{
				playerInput.SwitchCurrentActionMap("Player");
			}

			//���� ActionMap�� ����Ѵ�.
			Debug.Log(playerInput.currentActionMap);
		}

#else
	// old input sys if we do decide to have it (most likely wont)...
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

#if !UNITY_IOS || !UNITY_ANDROID

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

#endif

	}
	
}