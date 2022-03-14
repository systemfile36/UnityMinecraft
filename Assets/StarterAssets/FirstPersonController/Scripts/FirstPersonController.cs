using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

/* StarterAssets을 변형하였다. 한국어 주석이 붙은 것은 내가 수정한 부분
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
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
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
		public float pWidthCol = 0.5f;
		[Tooltip("Height Offset of Player Collision Check")]
		public float pHeightCol = 2.0f;
		[Tooltip("Offset for XZ Collision Check, adding to y")]
		public float pYOffset = 0.5f;


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
			//Debug.Log(_verticalVelocity + " " + Grounded);
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
			}
		}
		/// <summary>
		/// +X방향 충돌 확인
		/// </summary>
		private bool PosX
		{
			get
			{
				if (world.CheckForVoxel(transform.position.x + pWidthCol, transform.position.y + pHeightCol, transform.position.z)
				|| world.CheckForVoxel(transform.position.x + pWidthCol, transform.position.y + pYOffset, transform.position.z))
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
				if (world.CheckForVoxel(transform.position.x - pWidthCol, transform.position.y + pHeightCol, transform.position.z)
				|| world.CheckForVoxel(transform.position.x - pWidthCol, transform.position.y + pYOffset, transform.position.z))
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
				if (world.CheckForVoxel(transform.position.x, transform.position.y + pHeightCol, transform.position.z + pWidthCol)
				|| world.CheckForVoxel(transform.position.x, transform.position.y + pYOffset, transform.position.z + pWidthCol))
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
				if (world.CheckForVoxel(transform.position.x, transform.position.y + pHeightCol, transform.position.z - pWidthCol)
				|| world.CheckForVoxel(transform.position.x, transform.position.y + pYOffset, transform.position.z - pWidthCol))
				{
					return true;
				}
				else
				{
					return false;
				}	
			}
		}

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