using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 복셀의 생성을 위한 테이블과 청크, 월드 등의 static 정보를 저장하는 클래스
/// </summary>
public class VoxelData
{
	//청크 하나의 크기
	public static readonly int ChunkWidth = 16;
	public static readonly int ChunkHeight = 128;
	public static readonly int WorldSizeInChunks = 100;

	//빛과 관련된 값들
	public static float minLight = 0.15f;

	public static float maxLight = 0.8f;

	public static float lightFallOff = 0.08f;

	//시야 범위, 이 범위 내의 청크만 로드
	public static readonly int ViewDistanceInChunks = 5;

	//위의 변수들을 참고해 계산해야하므로 readonly가 불가능
	public static int WorldSizeInVoxels
	{
		get
		{
			return WorldSizeInChunks * ChunkWidth;
		}
	}

	//텍스쳐 아틀라스의 크기, 한 열(행)에 몇개의 텍스쳐가 있는가‱
	public static readonly int TextureAtlasSizeInBlocks = 16;
	
	//각 텍스쳐가 갖는 상대적인 비율 
	public static float NormalizedBlockTextureSize
	{
		get { return 1f / (float)TextureAtlasSizeInBlocks; }
	}

	//복셀의 정점들 상대 좌표
	public static readonly Vector3[] voxelVerts = new Vector3[8]
	{
		//전면						   //정점 번호
		new Vector3(0.0f, 0.0f, 0.0f), //0
		new Vector3(1.0f, 0.0f, 0.0f), //1
		new Vector3(1.0f, 1.0f, 0.0f), //2
		new Vector3(0.0f, 1.0f, 0.0f), //3
		
		//후면
		new Vector3(0.0f, 0.0f, 1.0f), //4
		new Vector3(1.0f, 0.0f, 1.0f), //5
		new Vector3(1.0f, 1.0f, 1.0f), //6
		new Vector3(0.0f, 1.0f, 1.0f), //7
	};

	//voxelTris와 순서를 맞춰서 각 면의 방향으로 1칸 이동한 곳의 상대 좌표를 기록
	public static readonly Vector3[] faceChecks = new Vector3[6]
	{
		//각 면이 바라보는 축의 방향
		new Vector3(0.0f, 0.0f, -1.0f), //-Z
		new Vector3(0.0f, 0.0f, 1.0f), //+Z
		new Vector3(0.0f, 1.0f, 0.0f), //+Y
		new Vector3(0.0f, -1.0f, 0.0f), //-Y
		new Vector3(-1.0f, 0.0f, 0.0f), //-X
		new Vector3(1.0f, 0.0f, 0.0f) //+X
	};

	//6개의 면의 삼각형들의 정점 인덱스 좌표
	public static readonly int[,] voxelTris = new int[6, 4]
	{
		//각 면의 기준은 왼쪽 아래가 0, 0인 쪽이 전면인 것
		//참조 시, 0, 1, 2, 2, 1, 3 순으로 참조함
		//정점 삽입시 사용(삼각형의 정점에 맞추기 위해)
		{ 0, 3, 1, 2 }, //전면
		{ 5, 6, 4, 7 }, //후면
		{ 3, 7, 2, 6 }, //윗면
		{ 1, 5, 0, 4 }, //아랫면
		{ 4, 7, 0, 3 }, //좌측면
		{ 1, 2, 5, 6 }  //우측면
	};

	//uv 값들, 텍스쳐 넣을 때 사용, voxelTris의 순서에 맞춤
	public static readonly Vector2[] voxelUvs = new Vector2[4]
	{
		//삼각혁의 인덱스 순서에 맞춤, 주석은 전면부의 예시
		//정점에 맞추어서 uv설정시 사용
		new Vector2(0.0f, 0.0f), //0
		new Vector2(0.0f, 1.0f), //3
		new Vector2(1.0f, 0.0f), //1
		new Vector2(1.0f, 1.0f)  //2
	};
	/// <summary>
	/// 벡터 두개를 정수로 내림한 후 일치 여부 반환
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static bool CompareVector3ByInteger(Vector3 a, Vector3 b)
	{
		int aX = Mathf.FloorToInt(a.x);
		int aY = Mathf.FloorToInt(a.y);
		int aZ = Mathf.FloorToInt(a.z);

		int bX = Mathf.FloorToInt(b.x);
		int bY = Mathf.FloorToInt(b.y);
		int bZ = Mathf.FloorToInt(b.z);

		if (aX == bX && aY == bY && aZ == bZ)
			return true;
		else
			return false;
	}
}
