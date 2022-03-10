using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//����ȭ ������ BlockType Ŭ����
[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;


    //���� �ĸ� ���� �Ʒ��� ���� ���� ����
    /// <summary>
    /// �� ���� �ؽ��� ���̵� ��ȯ
    /// </summary>
    /// <param name="faceIndex"></param>
    /// <returns></returns>
    public int GetTextureID(int faceIndex)
	{
        switch(faceIndex)
		{
            case 0:
                return frontFaceTexture;
            case 1:
                return backFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error : Invalid faceIndex \"" + faceIndex + "\"");
                return 0;
        }
	}
}

public class World : MonoBehaviour
{
    public Material material;
    public BlockType[] blockTypes;

    //ûũ���� �迭
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

	void Start()
	{
        GenerateWorld();
	}

    //������ ó�� ����Ǿ��� �� �ѹ� ����Ǵ� �޼ҵ�
    //������Ʈ�� ���߿� �߰�
    void GenerateWorld()
	{
        for(int x = 0; x < VoxelData.WorldSizeInChunks; x++)
		{
            for(int z = 0; z < VoxelData.WorldSizeInChunks; z++)
			{
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
			}
		}
	}
}
