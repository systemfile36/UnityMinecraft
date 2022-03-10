using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//������ �����͸� ���� �����ϱ� ���� ���̺���� ��Ƴ��� ����
public class VoxelData
{
	//ûũ �ϳ��� ũ��
	public static readonly int ChunkWidth = 5;
	public static readonly int ChunkHeight = 15;
	public static readonly int WorldSizeInChunks = 5;

	//�ؽ��� ��Ʋ���� ũ��, �� ��(��)�� ��� �ؽ��İ� �ִ°�
	public static readonly int TextureAtlasSizeInBlocks = 16;
	
	//�� �ؽ��İ� ���� ������� ���� 
	public static float NormalizedBlockTextureSize
	{
		get { return 1f / (float)TextureAtlasSizeInBlocks; }
	}

	//������ ������ ��� ��ǥ
	public static readonly Vector3[] voxelVerts = new Vector3[8]
	{
		//����						   //���� ��ȣ
		new Vector3(0.0f, 0.0f, 0.0f), //0
		new Vector3(1.0f, 0.0f, 0.0f), //1
		new Vector3(1.0f, 1.0f, 0.0f), //2
		new Vector3(0.0f, 1.0f, 0.0f), //3
		
		//�ĸ�
		new Vector3(0.0f, 0.0f, 1.0f), //4
		new Vector3(1.0f, 0.0f, 1.0f), //5
		new Vector3(1.0f, 1.0f, 1.0f), //6
		new Vector3(0.0f, 1.0f, 1.0f), //7
	};

	//voxelTris�� ������ ���缭 �� ���� �������� 1ĭ �̵��� ���� ��� ��ǥ�� ���
	public static readonly Vector3[] faceChecks = new Vector3[6]
	{
		//�� ���� �ٶ󺸴� ���� ����
		new Vector3(0.0f, 0.0f, -1.0f), //-Z
		new Vector3(0.0f, 0.0f, 1.0f), //+Z
		new Vector3(0.0f, 1.0f, 0.0f), //+Y
		new Vector3(0.0f, -1.0f, 0.0f), //-Y
		new Vector3(-1.0f, 0.0f, 0.0f), //-X
		new Vector3(1.0f, 0.0f, 0.0f) //+X
	};

	//6���� ���� �ﰢ������ ���� �ε��� ��ǥ
	public static readonly int[,] voxelTris = new int[6, 4]
	{
		//�� ���� ������ ���� �Ʒ��� 0, 0�� ���� ������ ��
		//���� ��, 0, 1, 2, 2, 1, 3 ������ ������
		//���� ���Խ� ���(�ﰢ���� ������ ���߱� ����)
		{ 0, 3, 1, 2 }, //����
		{ 5, 6, 4, 7 }, //�ĸ�
		{ 3, 7, 2, 6 }, //����
		{ 1, 5, 0, 4 }, //�Ʒ���
		{ 4, 7, 0, 3 }, //������
		{ 1, 2, 5, 6 }  //������
	};

	//uv ����, �ؽ��� ���� �� ���, voxelTris�� ������ ����
	public static readonly Vector2[] voxelUvs = new Vector2[4]
	{
		//�ﰢ���� �ε��� ������ ����, �ּ��� ������� ����
		//������ ���߾ uv������ ���
		new Vector2(0.0f, 0.0f), //0
		new Vector2(0.0f, 1.0f), //3
		new Vector2(1.0f, 0.0f), //1
		new Vector2(1.0f, 1.0f)  //2
	};
}
