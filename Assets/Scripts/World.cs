using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//직렬화 가능한 BlockType 클래스
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


    //전면 후면 윗면 아랫면 좌측 우측 순서
    /// <summary>
    /// 각 면의 텍스쳐 아이디 반환
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

    //청크들의 배열
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

	void Start()
	{
        GenerateWorld();
	}

    //게임이 처음 실행되었을 때 한번 실행되는 메소드
    //업데이트는 나중에 추가
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
