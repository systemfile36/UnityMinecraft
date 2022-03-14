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

/// <summary>
/// �������� ���踦 �ٷ�� Ŭ����(������Ʈ)
/// </summary>
public class World : MonoBehaviour
{
    //���� �õ尪
    public int seed;

    //���̿��� �����ϴ� ����
    public BiomeAttributes biome;

    //�÷��̾��� ��ǥ ������ ���� ����
    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blockTypes;

    //ûũ���� �迭
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    //Ȱ��ȭ�� ûũ�� ����
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    //�÷��̾��� ���� ��ġ�� ûũ ĳ��
    ChunkCoord playerChunkCoord;
    //�÷��̾ ���������� ��ġ�� ûũ
    ChunkCoord playerLastChunkCoord;


	void Start()
	{
        //�õ� ���� ���� ���������� �ʱ�ȭ
        //���� �õ�� ���� ��
        Random.InitState(seed);

        //���� �߾ӿ� ����
        spawnPosition =
            new Vector3(VoxelData.WorldSizeInVoxels / 2f,
            VoxelData.ChunkHeight, VoxelData.WorldSizeInVoxels / 2f);
        

        GenerateWorld();

        //�÷��̾ ��ġ�� ûũ �ʱ�ȭ
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        
	}

	void Update()
	{
        //�÷��̾� ���� ûũ ����
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        //�÷��̾��� ���� ��ġ�� ûũ�� ���������� ��ġ�� ûũ�� �ٸ��ٸ�
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        { 
            CheckViewDistance();
            //�ٽ� ����
            playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        }
	}
    /// <summary>
    /// ������ ��ǥ�� ������ ������ ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_y"></param>
    /// <param name="_z"></param>
    /// <returns></returns>
    public bool CheckForVoxel(float _x, float _y, float _z)
	{
        //������ 0�� ������ ����Ű�� ���� ��ǥ
        int xP = Mathf.FloorToInt(_x);
        int yP = Mathf.FloorToInt(_y);
        int zP = Mathf.FloorToInt(_z);

        //������ ���� ûũ�� ��ǥ
        int xChunk = xP / VoxelData.ChunkWidth;
        int zChunk = zP / VoxelData.ChunkWidth;

        //���� ��ǥ�� �� ûũ�� �����ǥ�� ��ȯ�ϴ� ����
        xP -= (xChunk * VoxelData.ChunkWidth);
        zP -= (zChunk * VoxelData.ChunkWidth);

        if (xP < 0 || xP >= VoxelData.WorldSizeInVoxels
            || yP < 0 || yP >= VoxelData.ChunkHeight
            || zP < 0 || zP >= VoxelData.WorldSizeInVoxels)
        {
            /*
            Debug.Log(string.Format("x : {0} y : {1} z : {2}\n" +
                "xChunk : {3} zChunk : {4}", xP, yP, zP, xChunk, zChunk));
            */
            return false;
        }
            

        return blockTypes[chunks[xChunk, zChunk].voxelMap[xP, yP, zP]].isSolid;
        /*
        ������ �������� �Ҽ����� ���󰡹Ƿ� xChunk���� xP��ǥ�� ���� ûũ�� ��ǥ, 
        �� ChunkCoord�� x ��ǥ�� �� ���̴�. zChunk�� ��������. 
        �̰Ϳ� ûũ�� ���̸� ���ؼ� ���ִ� ������ ûũ �������� ��ǥ�� ���� �� �ִ�.
        */

    }

    //�� �ڵ�� ���带 ����ų� ������ ����ų� �ϴ� ���� �˰��� ���� ��
    //���̿� ���� ���۵� �̰����� �߻���
    /// <summary>
    /// ��ǥ�� �޾Ƽ� �ش� ��ǥ�� �� ID�� ��ȯ
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public byte GetVoxel(Vector3 pos)
	{
        //y��ǥ ����ȭ
        int tempY = Mathf.FloorToInt(pos.y);

        //������ ���� ���ο� ���� �ʴٸ� Air(=0)��ȯ
        if (!IsVoxelInWorld(pos))
            return 0;
        //�� �Ϻ� ���������
        if (pos.y < 1)
        {
            return 1;
        }

        
        //0 ~ 1�� ���� �޸������ ���̸� ���ؼ� ������ ����
        int terHeight = 
            Mathf.FloorToInt(biome.terHeight * Noise.GetPerlin2D(
                new Vector2(pos.x, pos.z), 0, biome.terScale))
            + biome.solidHeight;

        //�ٷ� �����ϸ� ����� ��ġ���� ����
        byte vValue = 0;


        //�׽�Ʈ�� Height ��
        if (tempY == terHeight)
            vValue = 3;
        else if (tempY < terHeight && tempY > terHeight - 4)
            vValue = 4;
        else if (tempY > terHeight)
            vValue = 0;
        else
            vValue = 2;

        //�׽�Ʈ�� lode �ݿ� ��
        //���̶��(== ǥ���� ���̳� ������ �ƴ϶��
        if(vValue == 2)
		{
            foreach(Lode lode in biome.lodes)
			{
                //���� ���� �ִٸ�
                if(tempY > lode.minHeight && tempY < lode.maxHeight)
				{
                    //����� üũ�ؼ� true�� ��ȯ�Ǹ�
                    if(Noise.GetPerlin3D(pos, lode.Offset, lode.scale, lode.threshold))
					{
                        //vValue�� ����
                        vValue = lode.blockID;
					}
				}
			}
		}
        //�׸��� ����
        return vValue;
    }

    //������ ó�� ����Ǿ��� �� �ѹ� ����Ǵ� �޼ҵ�
    //������Ʈ�� ���߿� �߰�
    /// <summary>
    /// World �ʱ� ûũ ����
    /// </summary>
    void GenerateWorld()
	{
        //�� ûũ ��ǥ�� �ִ� �ּ�, ó�� ���� ������ �þ� ���� ���ʸ� �ε�
        player.position = spawnPosition;
        
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; 
            x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
		{
            
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks;
                z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
			{
                
                CreateNewChunk(x, z);
			}
		}  
	}

    /// <summary>
    /// ��ǥ�� �޾Ƽ� �׿� �´� ûũ ��ǥ�� ChunkCoord�� ��ȯ
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
	{
        //��ǥ�� ������
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return new ChunkCoord(x, z);

    }
    /// <summary>
    /// �÷��̾��� ��ǥ�� ����, �þ� �������� ûũ�� ����
    /// </summary>
    void CheckViewDistance()
	{
        //�÷��̾� ��ġ�� ûũ ��ǥ�� ���Ѵ�.
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);

        //�� ���ٽ��� �����ϱ� �� Ȱ��ȭ �Ǿ� �ִ� ûũ�� ����
        List<ChunkCoord> prevActiveChunks = new List<ChunkCoord>(activeChunks);

        //�÷��̾� �þ� ���� ���� ûũ��� �ݺ�
        for(int x = coord.x - VoxelData.ViewDistanceInChunks; 
            x < coord.x + VoxelData.ViewDistanceInChunks; x++)
		{
            for (int z = coord.z - VoxelData.ViewDistanceInChunks;
            z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                //�ӽ� ĳ��
                ChunkCoord temp = new ChunkCoord(x, z);
                //�þ� ���� ûũ�� ��ȿ�ϴٸ�
                if (IsChunkInWorld(temp))
				{
                    //���� ���� �ִµ� ��������� �ʾҴٸ�
                    if (chunks[temp.x, temp.z] == null)
                        //�����
                        CreateNewChunk(temp.x, temp.z);
                    //���� ���� �ִµ� Ȱ��ȭ �Ǿ� ���� �ʴٸ�
                    else if(!chunks[temp.x, temp.z].IsActive)
					{
                        //Ȱ��ȭ ��Ű�� Ȱ��ȭ�� ��Ͽ� �ø���.
                        chunks[temp.x, temp.z].IsActive = true;
                        activeChunks.Add(temp);
					}
				}
                //���� Ȱ�� ��Ͽ��� ���� �þ߿� �ִ� �͵��� ����
                for(int i = 0; i < prevActiveChunks.Count; i++)
				{
                    if(prevActiveChunks[i].Equals(temp))
					{
                        prevActiveChunks.RemoveAt(i);
					}
				}
            }
        }
        //���� �ݺ� �� ���� �͵��� ������ �þ߿� �־����� ���翣 ���� �͵��̴�.
        //���� ��Ȱ��ȭ ��Ų��.
        foreach(ChunkCoord c in prevActiveChunks)
		{
            chunks[c.x, c.z].IsActive = false;
		}
	}

    /// <summary>
    /// ûũ ��ǥ�� ������� ûũ ����
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    void CreateNewChunk(int x, int z)
	{
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        
        //�� ûũ�� ����� Ȱ��ȭ �� �Ϳ� �߰�
        activeChunks.Add(new ChunkCoord(x, z));
    }

    /// <summary>
    /// ������ ûũ ��ǥ�� �ִ� ûũ�� ���� ���� ���� �ִ��� ���� ��ȯ
    /// </summary>
    /// <param name="coord"></param>
    /// <returns></returns>
    bool IsChunkInWorld(ChunkCoord coord)
	{
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1
            && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return false;
	}

    /// <summary>
    /// ������ ���� ���ο� �ִ��� ���� ��ȯ
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IsVoxelInWorld(Vector3 pos)
	{
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels  &&
            pos.y >= 0 && pos.y < VoxelData.ChunkHeight  &&
            pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;
	}
}
