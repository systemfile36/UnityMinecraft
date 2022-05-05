using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 설정을 저장하는 클래스
/// </summary>
[System.Serializable]
public class Settings
{
	//성능 관련
    [Header("Performance")]
    public int ViewDistanceInChunks = 5;
	public int targetFrameRate = 75;

	//월드 관련
	[Header("World")]
	public GameMode gameMode = GameMode.Debug;
	public int seed;
	public Vector3 spawnPosition;


	//FirstPersonController.cs의 플레이어 행동 관련 변수들
	#region Player Behaviour Variables
	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float MoveSpeed = 4.0f;
	[Tooltip("Sprint speed of the character in m/s")]
	public float SprintSpeed = 6.0f;
	[Tooltip("Rotation speed of the character")]
	public float RotationSpeed = 2.0f;
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
	[Tooltip("Useful for rough ground")]
	public float GroundedOffset = -0.14f;
	[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	public float GroundedRadius = 0.3f;

	[Header("Cinemachine")]
	[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 85.0f;
	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -85.0f;

	//레이 캐스트 구현을 위한 변수
	//checkInterval : 체크하는 간격, 이 값만큼 좌표를 더하면서 체크
	//reach : 손이 닿는 범위
	[Header("Value to Pseudo Raycast")]
	[Tooltip("Interval of Check")]
	public float checkInterval = 0.1f;
	[Tooltip("Reach of Player")]
	public float reach = 8f;

	[Header("Place/Destroy Delay")]
	[Tooltip("Place Delay")]
	public float PlaceDelay = 0.125f;
	[Tooltip("Destroy Delay")]
	public float DestroyDelay = 0.125f;

	#endregion
}
