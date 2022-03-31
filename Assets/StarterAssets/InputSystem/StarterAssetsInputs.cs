using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

//StarterAssets 스크립트를 수정하여 나에게 맞는 것으로 변형하였음!

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
			//PlayerInput 컴포넌트를 받아서 참조를 저장한다.
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
		/// 디버그 키(F3)키가 눌렸을 때 작동하는 이벤트
		/// </summary>
		/// <param name="value"></param>
		public void OnDebugMenu()
		{
			//디버그 메뉴 활성화 여부 토글
			Debug.Log("OnDebugMenu Called");
			if (DebugText != null)
				DebugText.SetActive(!DebugText.activeSelf);
		}
		/// <summary>
		/// 스크롤의 축 값을 float로 저장하는 이벤트
		/// </summary>
		/// <param name="value"></param>
		public void OnScroll(InputValue value)
		{
			ScrollAxis = value.Get<float>();
		}

		//클릭 상태 변수 반환
		public void OnLeftClick(InputValue value)
		{
			IsLeftClicked = value.isPressed;
		}
		public void OnRightClick(InputValue value)
		{
			IsRightClicked = value.isPressed;
		}

		/// <summary>
		/// 인벤토리키가 눌렸을 때의 동작
		/// </summary>
		/// <param name="value"></param>
		public void OnInventoryToggle(InputValue value)
		{
			Debug.Log("OnInventoryToggle Called | IsOnUI =" + IsOnUI);
			IsOnUI = !IsOnUI;

			//ActionMap을 UI on/off 여부에 따라 전환
			if(IsOnUI)
			{
				playerInput.SwitchCurrentActionMap("UI");
			}
			else
			{
				playerInput.SwitchCurrentActionMap("Player");
			}

			//현재 ActionMap을 출력한다.
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