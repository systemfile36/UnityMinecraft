using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

/* StarterAssets을 변형하였다. 한국어 주석이 붙은 것은 주로 내가 수정한 부분
 */

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.0f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.2f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		//private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		//World 참조
		private World world;

		//카메라의 임계치(미세한 움직임을 막기 위함)
		private const float _threshold = 0.01f;

		//플레이어 판정 범위용 변수
		[Header("Colliders")]
		[Tooltip("Width of Player Collision Check")]
		public float pWidthCol = 0.2f;
		[Tooltip("Height Offset of Player Collision Check")]
		public float pHeightCol = 1.5f;
		[Tooltip("Offset for XZ Collision Check, adding to y")]
		public float pYOffset = 0.5f;
		[Tooltip("Offset for Side Collision Check, adding to pWidth")]
		public float pWidthSideOffset = 0.2f;

		//가이드 블럭 참조
		[Header("Guide Reference")]
		[Tooltip("This represents Selected Block")]
		public Transform selectGuide;
		[Tooltip("This represents Block that will be Placed")]
		public Transform placeGuide;

		//레이 캐스트 구현을 위한 변수
		//checkInterval : 체크하는 간격, 이 값만큼 좌표를 더하면서 체크
		//reach : 손이 닿는 범위
		[Header("Value to Pseudo Raycast")]
		[Tooltip("Interval of Check")]
		public float checkInterval = 0.1f;
		[Tooltip("Reach of Player")]
		public float reach = 8f;

		//들고있는 블럭의 인덱스
		public byte HoldingBlockId = 0;


		//블럭 설치, 파괴 딜레이를 위한 델타 타임
		private float _PlaceTimeOut;
		private float _DestroyTimeOut;

		[Header("Place/Destroy Delay")]
		[Tooltip("Place Delay")]
		public float PlaceDelay = 0.125f;
		[Tooltip("Destroy Delay")]
		public float DestroyDelay = 0.125f;

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}

			//월드 스크립트를 찾아서 리셋한다.
			if(world == null)
			{
				world = GameObject.Find("World").GetComponent<World>();
			}
		}

		private void Start()
		{
			_input = GetComponent<StarterAssetsInputs>();

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;

			//설치, 파괴 딜레이 초기화
			_PlaceTimeOut = PlaceDelay;
			_DestroyTimeOut = DestroyDelay;
		}

		private void Update()
		{
			/*
			GroundedCheck();
			ApplyGravity();
			Jump();
			UpCollision();
			Move();
			*/
			//가이드 블럭 생신
			SetGuideBlock();

			PlaceAndDestroyBlock();

			/*
			//스크롤로 블럭을 선택하는 로직
			if (_input.ScrollAxis != 0)
			{
				Debug.Log(_input.ScrollAxis);
				//스크롤 값에 따라 인덱스 증감
				if (_input.ScrollAxis > _input.ScrollThreshold)
					HoldingBlockIndex++;
				else if(_input.ScrollAxis < -_input.ScrollThreshold)
					HoldingBlockIndex--;

				//범위 체크해서 순회하게 만듬
				if(HoldingBlockIndex > (byte)(world.blockTypes.Length - 1))
				{
					HoldingBlockIndex = 1;
				}

				if(HoldingBlockIndex < 1)
				{
					HoldingBlockIndex = (byte)(world.blockTypes.Length - 1);
				}

				
			}
			*/
		}

		private void FixedUpdate()
		{
			GroundedCheck();
			ApplyGravity();
			Jump();
			UpCollision();
			Move();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		//여기서 부터 --End-- 까지 이동을 제외한 로직

		/// <summary>
		/// 마우스 입력에 따라 블럭 설치와 파괴
		/// </summary>
		public void PlaceAndDestroyBlock()
		{
			//선택 가이드가 활성화 되어 있다면
			if (selectGuide.gameObject.activeSelf)
			{
				//왼쪽 클릭시 블럭 파괴, 시간 체크
				if (_input.IsLeftClicked && _PlaceTimeOut <= 0.0f)
				{
					//선택 가이드의 좌표의 청크를 받아서 그 좌표의 블럭을 Air로 만든다.
					//즉 파괴한다.
					world.GetChunkFromVector3(selectGuide.position).EditVoxel(selectGuide.position, 0);
					_input.IsLeftClicked = false;

					//델타 타임 초기화
					_PlaceTimeOut = PlaceDelay;
				}
				//델타 타임 감소
				if (_PlaceTimeOut >= 0.0f)
				{
					_PlaceTimeOut -= Time.deltaTime;
				}
			}

			//놓을 위치 가이드가 활성화 되어있다면
			if(placeGuide.gameObject.activeSelf)
			{
				//오른쪽 클릭시 들고 있는 블럭 설치 (들고 있는 블럭이 0이라면 설치 X)
				if (_input.IsRightClicked && _DestroyTimeOut <= 0.0f && HoldingBlockId != 0)
				{
					//놓을 위치 가이드의 위치에 들고있는 블럭을 넣는다.
					//놓을 위치가 플레이어 위치거나 머리 위치면 놓지 않는다.
					if (!VoxelData.CompareVector3ByInteger(placeGuide.position, transform.position) && !VoxelData.CompareVector3ByInteger(placeGuide.position, transform.position + Vector3.up))
						world.GetChunkFromVector3(placeGuide.position).EditVoxel(placeGuide.position, HoldingBlockId);
					_input.IsRightClicked = false;

					_DestroyTimeOut = DestroyDelay;
				}

				//델타 타임 감소
				if (_DestroyTimeOut >= 0.0f)
				{
					_DestroyTimeOut -= Time.deltaTime;
				}
			}
		}
		
		/// <summary>
		/// 블럭을 선택하고 그에 맞게 가이드 블럭을 배치하는 메소드
		/// 의사 레이캐스트로 구현
		/// </summary>
		private void SetGuideBlock()
		{
			float step = checkInterval;

			//마지막 위치를 저장한다.
			//놓을 위치를 정할 때 사용하기 위함이다.
			Vector3 lastP = new Vector3();

			//step이 reach가 될때까지 반복
			while (step < reach)
			{
				//메인 카메라의 좌표에 바라보는 방향 벡터를 계속해서 더한다.
				//step은 늘어날 예정
				Vector3 pos = _mainCamera.transform.position + (_mainCamera.transform.forward * step);

				//만약 pos에 블럭이 있다면
				if(world.CheckForVoxel(pos))
				{
					//SelectGuide를 현재 pos 좌표로 옮긴다. 
					//다시한번 기록하지만, 어떤 좌표의 값을 정수로 내림하면 그 좌표가 속한 블럭의 좌표이다.
					selectGuide.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

					//PlaceGuide를 이전 pos 좌표로 옮긴다. 그래야 선택된 블럭 바로 전에 놓일 것
					placeGuide.position = lastP;

					//활성화 시킨다.
					selectGuide.gameObject.SetActive(true);
					placeGuide.gameObject.SetActive(true);

					return;
				}
				
				//이번 pos에 블럭이 없었다면 마지막 위치를 갱신하고 step을 올린다.
				lastP = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
				step += checkInterval;
			}

			//만약 reach에 닿을 때까지 블럭을 발견하지 못했다면 가이드 비활성화
			selectGuide.gameObject.SetActive(false);
			placeGuide.gameObject.SetActive(false);
		}

		//--End--

		private void GroundedCheck()
		{
			/*
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
			*/
			//착지 자체 구현
			// 4개의 꼭짓점에서 수선의 발을 내려 그곳에 블럭이 있는지 확인
			if (world.CheckForVoxel(transform.position.x + GroundedRadius, transform.position.y + GroundedOffset, transform.position.z + GroundedRadius)
				|| world.CheckForVoxel(transform.position.x - GroundedRadius, transform.position.y + GroundedOffset, transform.position.z - GroundedRadius)
				|| world.CheckForVoxel(transform.position.x - GroundedRadius, transform.position.y + GroundedOffset, transform.position.z + GroundedRadius)
				|| world.CheckForVoxel(transform.position.x + GroundedRadius, transform.position.y + GroundedOffset, transform.position.z - GroundedRadius))
			{
				Grounded = true;
			}
			else
				Grounded = false;
			
		}
		/// <summary>
		/// 머리 부분 충돌 판정, 수직 속도 0으로 리셋
		/// </summary>
		private void UpCollision()
		{
			if ((world.CheckForVoxel(transform.position.x + pWidthCol, transform.position.y + pHeightCol + 1f, transform.position.z + pWidthCol)
				|| world.CheckForVoxel(transform.position.x - pWidthCol, transform.position.y + pHeightCol + 1f, transform.position.z - pWidthCol)
				|| world.CheckForVoxel(transform.position.x - pWidthCol, transform.position.y + pHeightCol + 1f, transform.position.z + pWidthCol)
				|| world.CheckForVoxel(transform.position.x + pWidthCol, transform.position.y + pHeightCol + 1f, transform.position.z - pWidthCol))
				&& _verticalVelocity >= 0)
			{
				//상승 중일때만 적용
				_verticalVelocity = 0.0f;
				//점프 초기화
				_input.jump = false;
			}
		}

		#region X, Z축 충돌 판정

		/// <summary>
		/// +X방향 충돌 확인
		/// </summary>
		private bool PosX
		{
			get
			{
				if (world.CheckForVoxel(transform.position.x + pWidthCol + pWidthSideOffset, transform.position.y + pHeightCol, transform.position.z)
				|| world.CheckForVoxel(transform.position.x + pWidthCol + pWidthSideOffset, transform.position.y + pYOffset, transform.position.z))
				{
					return true;
				}
				else
				{
					return false;
				}
					
			}
		}
		/// <summary>
		/// -X방향 충돌 확인
		/// </summary>
		private bool NegX
		{
			get
			{
				if (world.CheckForVoxel(transform.position.x - pWidthCol - pWidthSideOffset, transform.position.y + pHeightCol, transform.position.z)
				|| world.CheckForVoxel(transform.position.x - pWidthCol - pWidthSideOffset, transform.position.y + pYOffset, transform.position.z))
				{
					return true;

				}
				else
				{
					return false;
				}
			}
		}
		/// <summary>
		/// +Z방향 충돌 확인
		/// </summary>
		private bool PosZ
		{
			get
			{
				if (world.CheckForVoxel(transform.position.x, transform.position.y + pHeightCol, transform.position.z + pWidthCol + pWidthSideOffset)
				|| world.CheckForVoxel(transform.position.x, transform.position.y + pYOffset, transform.position.z + pWidthCol + pWidthSideOffset))
				{
					return true;
				}
				else
				{
					return false;
				}
					
			}
		}
		/// <summary>
		/// -Z방향 충돌 확인
		/// </summary>
		private bool NegZ
		{
			get
			{
				if (world.CheckForVoxel(transform.position.x, transform.position.y + pHeightCol, transform.position.z - pWidthCol - pWidthSideOffset)
				|| world.CheckForVoxel(transform.position.x, transform.position.y + pYOffset, transform.position.z - pWidthCol - pWidthSideOffset))
				{
					return true;
				}
				else
				{
					return false;
				}	
			}
		}

		

		#endregion

		private void ApplyGravity()
		{
			
			_verticalVelocity += Gravity * Time.fixedDeltaTime;
			

			if (Grounded)
			{
				_verticalVelocity = 0.0f;
			}

		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * Time.deltaTime;
				_rotationVelocity = _input.look.x * RotationSpeed * Time.deltaTime;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			//쓰지 않는 것들을 지운다.
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			//float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			//float speedOffset = 0.1f;
			//float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			/*
			 * 이 마인크래프트에서는 필요가 없다.
			//가속을 담당하는 코드, 
			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}
			*/

			_speed = targetSpeed;

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			
		
			if (_input.move != Vector2.zero)
			{
				//rotation을 고려해서 x축과 z축 방향을 정함, 즉 세계 기준으로 바꿈
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}
			
			//세계 기준 이동 좌표
			Vector3 tempVector = inputDirection.normalized * (_speed * Time.fixedDeltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.fixedDeltaTime;

			//충돌 확인 후 벡터 조정
			if((tempVector.x > 0 && PosX) || (tempVector.x < 0 && NegX))
			{
				tempVector.x = 0.0f;
			}
			if((tempVector.z > 0 && PosZ) || (tempVector.z < 0 && NegZ))
			{
				tempVector.z = 0.0f;
			}
			
			
			//SideCollision(ref tempVector);
			// move the player
			//_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
			transform.Translate(tempVector, Space.World);
			//Debug.Log("PosX : " + world.CheckForVoxel(transform.position.x + pWidthCol, transform.position.y + pHeightCol - 0.1f, transform.position.z) + "\n" +
			//	"PosZ : " + world.CheckForVoxel(transform.position.x, transform.position.y + pHeightCol - 0.1f, transform.position.z + pWidthCol));
		}

		private void Jump()
		{
			
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				/*
				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					//_verticalVelocity = -2f;
					_verticalVelocity = 0.0f;
				}
				*/

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.fixedDeltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.fixedDeltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;


			}

			/*
			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			//착지한 상황에선 중력을 끈다.
			if (_verticalVelocity < _terminalVelocity || !Grounded)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
			*/
			
		}

		

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		//기즈모는 디버그 창에 보이는 것
		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			Gizmos.DrawCube(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), new Vector3(pWidthCol, pHeightCol/2.0f, pWidthCol));

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}