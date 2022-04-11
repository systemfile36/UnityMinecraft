using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

//����ȭ ������ BlockType Ŭ����
[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    //�������� ����
    public bool isTransparent;

    public byte MaxStackSize = 64;

    //�κ��丮 ��� ���� ������
    public Sprite icon;

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
/// ûũ�� ��踦 �ѳ���� ���������� ������ ���� Ŭ����
/// </summary>
public class VoxelMod
{
    public Vector3 pos;
    public byte id;

    public VoxelMod(Vector3 pos, byte id)
	{
        this.pos = pos;
        this.id = id;
	}

    public VoxelMod()
	{
        this.pos = new Vector3();
        this.id = 0;
	}
}
/// <summary>
/// ���� ����� ������
/// </summary>
public enum GameMode
{
    Creative,
    Survival,
    Debug
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

    //���� ��� ����
    [Range(0f, 0.93f)]
    public float globalLightLevel;

    //���� �㿡 ���� ��� ��
    public Color dayColor;
    public Color nightColor;

    //�÷��̾��� ��ǥ ������ ���� ����
    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public Material TransparentMaterial;
    public BlockType[] blockTypes;

    //ûũ���� �迭
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    //Ȱ��ȭ�� ûũ�� ����
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    //�÷��̾��� ���� ��ġ�� ûũ ĳ��
    public ChunkCoord playerChunkCoord;
    //�÷��̾ ���������� ��ġ�� ûũ
    ChunkCoord playerLastChunkCoord;

    //���� ûũ���� �����ϴ� ť
    Queue<ChunkCoord> chunksToCreateQue = new Queue<ChunkCoord>();
    
    //�ڷ�ƾ�� �̹� ���������� ���� �Ǵ��� ���� ����
    //private bool IsCreateChunks;

    //�������� ûũ���� ������Ʈ �ڷ�ƾ�� ���� ����
    private bool IsApplyingAll = false;

    //�ڷ�ƾ�� ���� ĳ�� ����
    //private IEnumerator m_CreateChunks;

    //�� �ڷ�ƾ�� ���� ĳ�� ����
    private IEnumerator m_ApplyModifications;

    //������ ������ ���� Que
    //�������� ���¸� ������ Queue�� �����ϴ� Queue
    public Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    //������ ���� �� ������Ʈ�� ���� ����Ʈ
    //������Ʈ�� ûũ ��ü�� ������
    List<Chunk> chunksToRefresh = new List<Chunk>();

    //�׷��� ûũ�� �����ϴ� ť
    //�ٸ� �����尡 �޽� �����͸� ����� ���� ������
    //���� �����尡 �̰��� ����� ȭ�鿡 �׷���
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    /// <summary>
    /// ����� ���� ������Ʈ
    /// </summary>
    public Object lockObject = new Object();

    /// <summary>
    /// ���� ���� ��带 ��Ÿ���� ���� ����
    /// </summary>
    public static readonly GameMode gameMode = GameMode.Debug;

	void Awake()
	{
        //�ڷ�ƾ ĳ��
        //m_CreateChunks = CreateChunks();
        //m_ApplyModifications = ApplyModifications();
    }

	void Start()
	{
        //�õ� ���� ���� ���������� �ʱ�ȭ
        //���� �õ�� ���� ��
        Random.InitState(seed);

        //���� �߾ӿ� ����
        spawnPosition =
            new Vector3(VoxelData.WorldSizeInVoxels / 2f,
            VoxelData.ChunkHeight, VoxelData.WorldSizeInVoxels / 2f);
        
        //���� ����
        GenerateWorld();

        //�÷��̾ ��ġ�� ûũ �ʱ�ȭ
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        
	}

	void Update()
	{
        //�÷��̾� ���� ûũ ����
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

		#region �׽�Ʈ�� �ڵ�
		//"GlobalLightLevel"��� ������ globalLightLevel�� �����Ѵ�.
		//��� ���̴��� �� �̸��� ���� ������ ã�´�.
		Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);

        //ī�޶��� ������ ����, 
        //���� ���� ���� �� ���̸� ���� ��� ��ŭ ���� ������
        //Camera.main.backgroundColor = Color.Lerp(dayColor, nightColor, globalLightLevel);
		#endregion


		//�÷��̾��� ���� ��ġ�� ûũ�� ���������� ��ġ�� ûũ�� �ٸ��ٸ�
		if (!playerChunkCoord.Equals(playerLastChunkCoord))
        { 
            CheckViewDistance();
            //�ٽ� ����
            playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        }

        /*
        //���� ���� ûũ�� �ϳ� �̻��̰� �̹� ������ ���°� �ƴ϶��
        if(chunksToCreateQue.Count > 0 && !IsCreateChunks)
		{
            m_CreateChunks = CreateChunks();
            //�ڷ�ƾ�� �����Ͽ� ûũ�� �����.
            StartCoroutine(m_CreateChunks);
		}
        */
       
        //������ ������
        //�̹� ���� ���� �ƴ϶��
        if(!IsApplyingAll)
		{
            ApplyModifications();
        }

        //���� �׷��� ûũ�� �ִٸ�
        //�������ӿ� �ϳ�����
        if(chunksToDraw.Count > 0)
		{
            //�ϴ� ��ٴ�.
            lock(lockObject)
			{
                //���� ���� ûũ�� ���� �����Ǿ���
                //�����忡 ���� �������� �ƴ϶��
                if(chunksToDraw.Peek().IsEditable)
				{
                    chunksToDraw.Dequeue().ApplyChunkMesh();
				}
			}
		}


        //ûũ �ʱ�ȭ(Init)
        //�������ӿ� �ϳ�����
        if (chunksToCreateQue.Count > 0)
            CreateChunk();

        //ûũ ����
        //���� �������ӿ� �ϳ�����
        if (chunksToRefresh.Count > 0)
            RefreshChunk();

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
        return CheckVoxelSolid(new Vector3(_x, _y, _z));
    }
    
    /// <summary>
    /// ���� ��ǥ�� �޾Ƽ� �� ��ǥ�� ���� ûũ�� ���� �����ؼ�
    /// ���� isSolid�� ��ȯ
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckVoxelSolid(Vector3 pos)
	{
        //pos�� ���� ûũ ��ǥ �ҷ���
        ChunkCoord thisChunk = new ChunkCoord(pos);

        //��ǥ ��ȿ ��ȯ
        if (!IsVoxelInWorld(pos))
            return false;

        //������ ��ǥ�� ûũ�� �����Ǿ���, ûũ�� ���� �ʱ�ȭ �Ǿ��ٸ�
        if(chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
		{
            //������ ��ǥ�� �ִ� ���� Ÿ���� �޾� isSolid ��ȯ
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;
		}

        //���࿡ ���� ���ǿ� ��� �ش��� ������ GetVoxel�� ȣ���ؼ� Ȯ��
        return blockTypes[GetVoxel(pos)].isSolid;
	}

    /// <summary>
    /// ���� ��ǥ�� �޾Ƽ� �� ��ǥ�� ���� ûũ�� ���� ����,
    /// ���� ���� ���θ� ��ȯ
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckVoxelTransparent(Vector3 pos)
    {
        //pos�� ���� ûũ ��ǥ �ҷ���
        ChunkCoord thisChunk = new ChunkCoord(pos);

        //��ǥ ��ȿ ��ȯ
        if (!IsVoxelInWorld(pos))
            return false;

        //������ ��ǥ�� ûũ�� �����Ǿ���, ûũ�� ���� �ʱ�ȭ �Ǿ��ٸ�
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
        {
            //������ ��ǥ�� �ִ� ���� Ÿ���� �޾� isSolid ��ȯ
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isTransparent;
        }

        //���࿡ ���� ���ǿ� ��� �ش��� ������ GetVoxel�� ȣ���ؼ� Ȯ��
        return blockTypes[GetVoxel(pos)].isTransparent;
    }

    //�� �ڵ�� ���带 ����ų� ������ ����ų� �ϴ� ���� �˰��� ���� ��
    //���̿� ���� ���۵� �̰����� �߻���
    /// <summary>
    /// ��ǥ�� �޾Ƽ� �ش� ��ǥ�� �� ID�� ��ȯ
    /// �� ���� �˰����� ���Ե�
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


        //�⺻���� ���� Ʋ
        //���̿� ���� ���� ��ġ
        if (tempY == terHeight)
            vValue = 3;
        else if (tempY < terHeight && tempY > terHeight - 4)
            vValue = 4;
        else if (tempY > terHeight)
            vValue = 0;
        else
            vValue = 2;

        //Lode �ݿ� �κ�, ���� ��ϵ� ����
        //���̶��(== ǥ���� ���̳� ������ �ƴ϶��
        if(vValue == 2)
		{
            //Lode�� ���ؼ� �ݺ�, ���� ���������� �Ӱ�ġ, ������ üũ �ؼ� vValue���� ����
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

        //���� �ݿ� �κ�
        //���鿡�� �����ϱ� ���� ���ǹ�
        if(tempY == terHeight)
		{
            //������ �����Ǵ� ������ ����
            if(Noise.GetPerlin2D(new Vector2(pos.x, pos.z), 0, biome.ForestScale) > biome.ForestThreshold)
			{
                //vValue = 1;
                //���� ������ �����Ǵ� ��ġ
                //�̹� ������ ������ ���¿��� �ٽ� Noise�� �޾Ƽ� vValue�� �ٲپ����Ƿ�
                //���� ���� �ȿ� �ٽ� �л굵�� �Ӱ�ġ�� ���� ��ġ��
                if (Noise.GetPerlin2D(new Vector2(pos.x, pos.z), 0, biome.TreeScale) > biome.TreeThreshold)
				{

                    //���� ���°� ����ִ� Queue<VoxelMod>�� �޾Ƽ� modifications�� Enqueue�Ѵ�.
                    //���� �ϳ��� ���� lock�� �Ǵ�.
                    lock (lockObject)
                    {
                        modifications.Enqueue(Structure.CreateTree(pos, biome.Min_TreeHeight, biome.Max_TreeHeight));
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
                //ó������ �������ڸ��� �ʱ�ȭ �Ǿ�� ��
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
			}
		}

		#region ������ ���ú�
		/*
        //�������� �����ϴ� �κ�
        VoxelMod v;
        while(modifications.Count > 0)
		{
            v = modifications.Dequeue();

            //VoxelMod�� ��ġ�� ���� ûũ�� ��ǥ �޾ƿ�
            ChunkCoord c = GetChunkCoordFromVector3(v.pos);

            //�Ʒ� �ڵ�� ������ �������� ûũ�� �������� ���
            //�߸��� ���� �����ϱ� �����̴�.
            //��, �������� ûũ�� ��ġ�� ��ģ ûũ�� �����Ѵ�.
            //�ߺ�üũ�� CheckViewDistance���� �Ѵ�.
            //���� �� ��ġ�� ���� �������� ���� ûũ���
            if(chunks[c.x, c.z] == null)
			{
                //�����ϰ� ��Ƽ�꿡 �߰�
                chunks[c.x, c.z] = new Chunk(c, this, true);
                activeChunks.Add(c);
			}

            //World.cs�� �ִ� modifications���� 
            //��ġ�� �´� ûũ���� modifications�� �־��ش�.
            chunks[c.x, c.z].modifications.Enqueue(v);

            //���� ������Ʈ�� ûũ ��Ͽ� ���� �޾ƿ� ûũ�� ���ٸ�
            if(!chunksToRefresh.Contains(chunks[c.x, c.z]))
			{
                //�߰��Ѵ�.
                chunksToRefresh.Add(chunks[c.x, c.z]);
			}
		}
        //������ ûũ ��ϵ��� ûũ�� ���� �����Ѵ�.
        while(chunksToRefresh.Count > 0)
		{
            chunksToRefresh[0].RefreshChunkMeshData();
            chunksToRefresh.RemoveAt(0);
		}
        */
		#endregion
	}

	/// <summary>
	/// ����ûũ ť���� �ϳ��� ���� �ʱ�ȭ�ϴ� �޼ҵ�
	/// Update���� ȣ���
	/// </summary>
	void CreateChunk()
	{
        ChunkCoord c = chunksToCreateQue.Dequeue();
        activeChunks.Add(c);
        chunks[c.x, c.z].Init();
	}

    /// <summary>
    /// ûũ�� üũ�ؼ� ������Ʈ �Ѵ�.
    /// Update���� ȣ���
    /// </summary>
    void RefreshChunk()
	{
        //while������ �����ϱ� ����
        bool refreshed = false;
        int index = 0;

        while(!refreshed && index < chunksToRefresh.Count - 1)
		{
            //���� ������Ʈ�� ûũ ����� ûũ�� ���� ���� �Ǿ��ٸ�
            //��, GetVoxel�� ȣ���ؼ� World.cs�� �������� �߰��ߴٸ�
            if(chunksToRefresh[index].IsEditable)
			{
                //ûũ ������Ʈ �� ����
                chunksToRefresh[index].RefreshChunkMeshData();
                chunksToRefresh.RemoveAt(index);

                //�ݺ��� Ż��
                refreshed = true;
			}
            //ûũ�� ���� ���� ���� �ʾҴٸ�
            else
			{
                //�ε��� �����ϰ� �ٽ� Ȯ��
                index++;
			}
		}
	}

    /// <summary>
    /// modifications�� ������ ûũ �ʱ�ȭ �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    void ApplyModifications()
	{
        //������
        IsApplyingAll = true;

        

        VoxelMod v;
        Queue<VoxelMod> que;
        while(modifications.Count > 0)
		{
            //Queue<Queue<VoxelMod>>���� Queue<VoxelMod> �ϳ��� �����´�.
            que = modifications.Dequeue();

            //que�� ���� �ݺ��ϸ� ���� ���� �ݿ�
            while (que.Count > 0)
            {
                //VoxelMod�� �޾ƿ�
                v = que.Dequeue();

                //VoxelMod�� ��ġ�� ���� ûũ�� ��ǥ �޾ƿ�
                ChunkCoord c = GetChunkCoordFromVector3(v.pos);

                //�Ʒ� �ڵ�� ������ �������� ûũ�� �������� ���
                //�߸��� ���� �����ϱ� �����̴�.
                //��, �������� ûũ�� ��ġ�� ��ģ ûũ�� �����Ѵ�.
                //�ߺ�üũ�� CheckViewDistance���� �Ѵ�.
                //���� �� ��ġ�� ���� �������� ���� ûũ���
                if (chunks[c.x, c.z] == null)
                {
                    //�����ϰ� ��Ƽ�꿡 �߰�
                    chunks[c.x, c.z] = new Chunk(c, this, true);
                    activeChunks.Add(c);
                }

                //World.cs�� �ִ� modifications���� 
                //��ġ�� �´� ûũ���� modifications�� �־��ش�.
                chunks[c.x, c.z].modifications.Enqueue(v);

                //���� ������Ʈ�� ûũ ��Ͽ� ���� �޾ƿ� ûũ�� ���ٸ�
                if (!chunksToRefresh.Contains(chunks[c.x, c.z]))
                {
                    //�߰��Ѵ�.
                    chunksToRefresh.Add(chunks[c.x, c.z]);
                }
            }
        }
        //�ڷ�ƾ �����
        IsApplyingAll = false;
    }

    /*
    /// <summary>
    /// ûũ�� �����ϴ� �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    IEnumerator CreateChunks()
	{
        //ûũ ���������� �ǹ��ϴ� �÷���
        IsCreateChunks = true;

        //���� ûũ���� ���� ������ ��� �ݺ�
        while(chunksToCreateQue.Count > 0)
		{
            
            //���� ûũ ť���� ûũ�� �ʱ�ȭ ��Ű�� ť���� ����
            chunks[chunksToCreateQue.Peek().x, chunksToCreateQue.Peek().z].Init();
            chunksToCreateQue.Dequeue();
            //�� ������ ���� �纸
            yield return null;
		}

        IsCreateChunks = false;
	}
    */

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
    /// ��ǥ�� �޾Ƽ� �� ��ǥ�� ���� ûũ ��ü�� ��ȯ
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Chunk GetChunkFromVector3(Vector3 pos)
	{
        //��ǥ�� ������
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        
        return chunks[x,z];
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
        //���� �� activeChunks Ŭ����
        activeChunks.Clear();
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
                    {
                        //ûũ�� ������ �ϰ�(�ʱ�ȭ�� ���� ���� ����)
                        //���� ûũ ��Ͽ� �ִ´�.
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreateQue.Enqueue(temp);
                    }
                    //���� ���� �ִµ� Ȱ��ȭ �Ǿ� ���� �ʴٸ�
                    else if (!chunks[temp.x, temp.z].IsActive)
                    {
                        //Ȱ��ȭ ��Ű�� Ȱ��ȭ�� ��Ͽ� �ø���.
                        chunks[temp.x, temp.z].IsActive = true;
                        
                    }
                    activeChunks.Add(temp);
                }
                //���� Ȱ�� ��Ͽ��� ���� �þ߿� �ִ� �͵��� ����
                for(int i = 0; i < prevActiveChunks.Count; i++)
				{
                    if(prevActiveChunks[i].Equals(temp))
					{
                        //Debug.Log(prevActiveChunks[i].x + " " + prevActiveChunks[i].z);
                        prevActiveChunks.RemoveAt(i);
					}
				}
            }
        }
        

        //���� �ݺ� �� ���� �͵��� ������ �þ߿� �־����� ���翣 ���� �͵��̴�.
        //���� ��Ȱ��ȭ ��Ų��.
        
        foreach (ChunkCoord c in prevActiveChunks)
		{
        
            chunks[c.x, c.z].IsActive = false;
            
		}
        
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
