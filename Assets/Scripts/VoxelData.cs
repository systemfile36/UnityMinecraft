using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//복셀의 데이터를 쉽게 참조하기 위한 테이블들을 모아놓은 파일
public class VoxelData
{
	//청크 하나의 크기
	public static readonly int ChunkWidth = 5;
	public static readonly int ChunkHeight = 5;

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
	public static readonly int[,] voxelTris = new int[6, 6]
	{
		//각 면의 기준은 왼쪽 아래가 0, 0인 쪽이 전면인 것
		{ 0, 3, 1, 1, 3, 2 }, //전면
		{ 5, 6, 4, 4, 6, 7 }, //후면
		{ 3, 7, 2, 2, 7, 6 }, //윗면
		{ 1, 5, 0, 0, 5, 4 }, //아랫면
		{ 4, 7, 0, 0, 7, 3 }, //좌측면
		{ 1, 2, 5, 5, 2, 6 }  //우측면
	};

	//uv 값들, 텍스쳐 넣을 때 사용, voxelTris의 순서에 맞춤
	public static readonly Vector2[] voxelUvs = new Vector2[6]
	{
		//삼각혁의 인덱스 순서에 맞춤, 주석은 전면부의 예시
		new Vector2(0.0f, 0.0f), //0
		new Vector2(0.0f, 1.0f), //3
		new Vector2(1.0f, 0.0f), //1

		new Vector2(1.0f, 0.0f), //1
		new Vector2(0.0f, 1.0f), //3
		new Vector2(1.0f, 1.0f)  //2
	};
}
