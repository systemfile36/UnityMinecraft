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
	public int LoadDistanceInChunks = 10;
    public int ViewDistanceInChunks = 5;
	public int targetFrameRate = 60;

	//월드 관련
	[Header("World")]
	public GameMode gameMode = GameMode.Debug;
	public Vector3_S spawnPosition = 
		new Vector3_S(VoxelData.WorldSizeInVoxels/2, VoxelData.ChunkHeight, VoxelData.WorldSizeInVoxels / 2);

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
	[Tooltip("Edit Delay")]
	public float EditDelay = 0.8f;

	[Space(10)]
	[Header("Colliders")]
	[Header("Can't be modified in Runtime")]
	[Tooltip("Width of Player Collision Check")]
	public float pWidthCol = 0.3f;
	[Tooltip("Height Offset of Player Collision Check")]
	public float pHeightCol = 1.7f;
	[Tooltip("Mid Collider Offset")]
	public float pHeightMidCol = 0.8f;
	[Tooltip("Offset for XZ Collision Check, adding to y")]
	public float pYOffset = 0.08f;
	[Tooltip("Offset for Side Collision Check, adding to pWidth")]
	public float pWidthSideOffset = 0.15f;
	[Tooltip("Percentage of Collider's magnitude used by CheckVoxelInCollider")]
	public float pInvalidRate = 0.85f;
	#endregion

	/// <summary>
	/// 설정값이 유효한지 반환
	/// </summary>
	/// <returns></returns>
	public bool IsValid()
    {
		
		if((ViewDistanceInChunks > 0 && ViewDistanceInChunks < 50)
			&& (targetFrameRate >= 30 && targetFrameRate <= 144)
			&& (spawnPosition.x > 0 && spawnPosition.y > 0 && spawnPosition.z > 0
				&& spawnPosition.x < VoxelData.WorldSizeInVoxels 
				&& spawnPosition.y <= VoxelData.ChunkHeight
				&& spawnPosition.z < VoxelData.WorldSizeInVoxels))
        {
			return true;
        }

		return false;
    }
}

/// <summary>
/// 직렬화 가능한 Vector2 클래스
/// </summary>
[System.Serializable]
public class Vector2_S
{
	public float x;
	public float y;

	public Vector2_S(int x, int y)
    {
		this.x = x; 
		this.y = y;
    }

    public Vector2_S(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2_S(Vector2 v)
	{
		x = v.x;
		y = v.y;
    }

	public Vector2 GetVector2()
    {
		return new Vector2(x, y);
    }
}

/// <summary>
/// 직렬화 가능한 Vector3 클래스
/// </summary>
[System.Serializable]
public class Vector3_S
{
	public float x;
	public float y;
	public float z;

	public Vector3_S(int x, int y, int z)
    {
		this.x = x;
		this.y = y;
		this.z = z;
    }

	public Vector3_S(float x, float y, float z)
    {
		this.x = x;
		this.y = y;
		this.z = z;
    }

	public Vector3_S(Vector3 v)
    {
		x = v.x;
		y = v.y;
		z = v.z;
    }

	public Vector3 GetVector3()
    {
		return new Vector3(x, y, z);
    }
}