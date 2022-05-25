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

		[Header("UI Mode On/Off")]
		[SerializeField] private bool _IsOnUI = false;

		[Header("Is Holding Block In Cursor")]
		[SerializeField] private bool _IsHoldingCursor = false;

		public bool IsOnUI
		{
			get
			{
				return _IsOnUI;
			}

			set
			{
				//커서에 홀딩중인 블럭이 있으면
				//무조건 UI를 켠 상태로 유지한다.
				if(_IsHoldingCursor)
				{
					GameManager.Mgr.PrintInfo("Holding Item Now! Can't Off UI");
					_IsOnUI = true;
				}
				else
				{
					_IsOnUI = value;
				}
			}
		}

		public bool IsHoldingCursor
		{
			get
			{
				return _IsHoldingCursor;
			}

			set
			{
				_IsHoldingCursor = value;
			}

		}

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

			SetCursorState(true);
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
			Debug.Log("OnInventoryToggle Called | IsOnUI =" + _IsOnUI);

			//인벤토리 토글
			IsOnUI = !IsOnUI;

			//ActionMap을 UI on/off 여부에 따라 전환
			if(_IsOnUI)
			{
				//커서 잠금을 해제
				SetCursorState(false);
				playerInput.SwitchCurrentActionMap("UI");
			}
			else
			{
				//커서 잠금
				SetCursorState(true);
				playerInput.SwitchCurrentActionMap("Player");
			}

			//현재 ActionMap을 출력한다.
			Debug.Log(playerInput.currentActionMap);
		}

		/// <summary>
		/// 종료 명령이 들어왔을 때의 경우
		/// </summary>
		/// <param name="value"></param>
        public void OnExit(InputValue value)
        {
			//만약 UI모드라면 
			if(IsOnUI)
            {
				//UI를 끄려고 시도한다.
				IsOnUI = false;

				//끄는데 성공했다면 Player모드로 전환한다.
				if(!IsOnUI)
                {
					SetCursorState(true);
					playerInput.SwitchCurrentActionMap("Player");
                }
				
				return;
            }

			//UI모드가 아니라면 종료한다.
			GameManager.Mgr.QuitGame();
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

		//포커스가 이 프로그램으로 옮겨 졌을 때 발생하는 이벤트
		private void OnApplicationFocus(bool hasFocus)
		{
			//UI모드가 아닐때만, 어플리케이션으로 커서가 옮겨졌을 때 커서잠금
			if(!_IsOnUI)
				SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

#endif

	}
	
}