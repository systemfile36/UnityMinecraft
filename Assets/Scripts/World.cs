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

/// <summary>
/// 전반적인 세계를 다루는 클래스(오브젝트)
/// </summary>
public class World : MonoBehaviour
{
    //맵의 시드값
    public int seed;

    //바이옴을 설정하는 변수
    public BiomeAttributes biome;

    //플레이어의 좌표 참조를 위한 변수
    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blockTypes;

    //청크들의 배열
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    //활성화된 청크들 저장
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    //플레이어의 현재 위치한 청크 캐싱
    ChunkCoord playerChunkCoord;
    //플레이어가 마지막으로 위치한 청크
    ChunkCoord playerLastChunkCoord;

    //만들 청크들을 저장하는 큐
    Queue<ChunkCoord> chunksToCreateQue = new Queue<ChunkCoord>();
    //코루틴이 이미 실행중인지 여부 판단을 위한 변수
    private bool IsCreateChunks;

    private IEnumerator m_CreateChunks;

	void Awake()
	{
        //코루틴 캐싱
        m_CreateChunks = CreateChunks();

    }

	void Start()
	{
        //시드 값에 따라 난수생성기 초기화
        //같은 시드는 같은 맵
        Random.InitState(seed);

        //월드 중앙에 스폰
        spawnPosition =
            new Vector3(VoxelData.WorldSizeInVoxels / 2f,
            VoxelData.ChunkHeight, VoxelData.WorldSizeInVoxels / 2f);
        
        //월드 생성
        GenerateWorld();

        //플레이어가 위치한 청크 초기화
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        
	}

	void Update()
	{
        //플레이어 현재 청크 갱신
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        //플레이어의 현재 위치한 청크가 마지막으로 위치한 청크와 다르다면
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        { 
            CheckViewDistance();
            //다시 갱신
            playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        }

        //만약 만들 청크가 하나 이상이고 이미 갱신한 상태가 아니라면
        if(chunksToCreateQue.Count > 0 && !IsCreateChunks)
		{
            m_CreateChunks = CreateChunks();
            //코루틴을 시작하여 청크를 만든다.
            StartCoroutine(m_CreateChunks);
		}

        
	}
    
    /// <summary>
    /// 지정된 좌표에 복셀의 유무를 반환한다.
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_y"></param>
    /// <param name="_z"></param>
    /// <returns></returns>
    public bool CheckForVoxel(float _x, float _y, float _z)
	{
        return CheckForVoxel(new Vector3(_x, _y, _z));
    }
    
    /// <summary>
    /// 월드 좌표를 받아서 그 좌표가 속한 청크의 맵을 참조해서
    /// 블럭의 유무를 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckForVoxel(Vector3 pos)
	{
        //pos가 속한 청크 좌표 불러옴
        ChunkCoord thisChunk = new ChunkCoord(pos);

        //좌표 유효 반환
        if (!IsVoxelInWorld(pos))
            return false;

        //지정된 좌표에 청크가 생성되었고, 청크의 맵이 초기화 되었다면
        if(chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsMapInit)
		{
            //지정된 좌표에 있는 블럭의 타입을 받아 isSolid 반환
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].isSolid;
		}

        //만약에 위에 조건에 모두 해당이 없으면 GetVoxel을 호출해서 확인
        return blockTypes[GetVoxel(pos)].isSolid;
	}

    //이 코드는 월드를 만들거나 동굴을 만들거나 하는 등의 알고리즘에 사용될 것
    //바이옴 등의 동작도 이곳에서 발생함
    /// <summary>
    /// 좌표를 받아서 해당 좌표의 블럭 ID를 반환
    /// 맵 생성 알고리즘이 포함됨
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public byte GetVoxel(Vector3 pos)
	{
        //y좌표 정수화
        int tempY = Mathf.FloorToInt(pos.y);

        //복셀이 월드 내부에 있지 않다면 Air(=0)반환
        if (!IsVoxelInWorld(pos))
            return 0;
        //맵 하부 베드락으로
        if (pos.y < 1)
        {
            return 1;
        }

        
        //0 ~ 1의 값인 펄린노이즈에 높이를 곱해서 범위를 조정
        int terHeight = 
            Mathf.FloorToInt(biome.terHeight * Noise.GetPerlin2D(
                new Vector2(pos.x, pos.z), 0, biome.terScale))
            + biome.solidHeight;

        //바로 리턴하면 제대로 배치되지 않음
        byte vValue = 0;


        //테스트용 Height 맵
        if (tempY == terHeight)
            vValue = 3;
        else if (tempY < terHeight && tempY > terHeight - 4)
            vValue = 4;
        else if (tempY > terHeight)
            vValue = 0;
        else
            vValue = 2;

        //테스트용 lode 반영 맵
        //돌이라면(== 표면의 흙이나 배드락이 아니라면
        if(vValue == 2)
		{
            //Lode에 대해서 반복, 블럭의 생성범위와 임계치, 스케일 체크 해서 vValue값을 변형
            foreach(Lode lode in biome.lodes)
			{
                //범위 내에 있다면
                if(tempY > lode.minHeight && tempY < lode.maxHeight)
				{
                    //노이즈를 체크해서 true가 반환되면
                    if(Noise.GetPerlin3D(pos, lode.Offset, lode.scale, lode.threshold))
					{
                        //vValue를 변형
                        vValue = lode.blockID;
					}
				}
			}
		}
        //그리고 리턴
        return vValue;
    }

    //게임이 처음 실행되었을 때 한번 실행되는 메소드
    //업데이트는 나중에 추가
    /// <summary>
    /// World 초기 청크 생성
    /// </summary>
    void GenerateWorld()
	{
        //각 청크 좌표의 최대 최소, 처음 스폰 지점의 시야 범위 안쪽만 로드
        player.position = spawnPosition;
        
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; 
            x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
		{
            
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks;
                z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
			{
                //처음에는 생성되자마자 초기화 되어야 함
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
			}
		}  
	}

    /// <summary>
    /// 청크를 생성하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator CreateChunks()
	{
        //청크 생성중임을 의미하는 플래그
        IsCreateChunks = true;

        //만들 청크들이 남아 있으면 계속 반복
        while(chunksToCreateQue.Count > 0)
		{
            Debug.Log(chunksToCreateQue.Count);
            //만들 청크 큐에서 청크를 초기화 시키고 큐에서 삭제
            chunks[chunksToCreateQue.Peek().x, chunksToCreateQue.Peek().z].Init();
            chunksToCreateQue.Dequeue();
            //한 프레임 동안 양보
            yield return null;
		}

        IsCreateChunks = false;
	}

    /// <summary>
    /// 좌표를 받아서 그에 맞는 청크 좌표를 ChunkCoord로 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
	{
        //좌표값 정수로
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return new ChunkCoord(x, z);

    }
    /// <summary>
    /// 플레이어의 좌표를 참조, 시야 범위내의 청크를 생성
    /// </summary>
    void CheckViewDistance()
	{
        //플레이어 위치의 청크 좌표를 구한다.
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);

        //뷰 디스텐스를 갱신하기 전 활성화 되어 있던 청크들 저장
        List<ChunkCoord> prevActiveChunks = new List<ChunkCoord>(activeChunks);

        //플레이어 시야 범위 내의 청크들로 반복
        for(int x = coord.x - VoxelData.ViewDistanceInChunks; 
            x < coord.x + VoxelData.ViewDistanceInChunks; x++)
		{
            for (int z = coord.z - VoxelData.ViewDistanceInChunks;
            z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                //임시 캐싱
                ChunkCoord temp = new ChunkCoord(x, z);
                //시야 내의 청크가 유효하다면
                if (IsChunkInWorld(temp))
				{
                    //범위 내에 있는데 만들어지지 않았다면
                    if (chunks[temp.x, temp.z] == null)
                    {
                        //청크를 생성만 하고(초기화는 하지 않은 상태)
                        //만들 청크 목록에 넣는다.
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreateQue.Enqueue(temp);
                    }
                    //범위 내에 있는데 활성화 되어 있지 않다면
                    else if (!chunks[temp.x, temp.z].IsActive)
                    {
                        //활성화 시키고 활성화된 목록에 올린다.
                        chunks[temp.x, temp.z].IsActive = true;
                        activeChunks.Add(temp);
                    }
				}
                //이전 활성 목록에서 현재 시야에 있는 것들을 뺀다
                for(int i = 0; i < prevActiveChunks.Count; i++)
				{
                    if(prevActiveChunks[i].Equals(temp))
					{
                        prevActiveChunks.RemoveAt(i);
					}
				}
            }
        }
        //위의 반복 후 남은 것들은 전에는 시야에 있었지만 현재엔 없는 것들이다.
        //따라서 비활성화 시킨다.
        foreach(ChunkCoord c in prevActiveChunks)
		{
            chunks[c.x, c.z].IsActive = false;
		}
	}

    /// <summary>
    /// 지정된 청크 좌표에 있는 청크가 월드 범위 내에 있는지 여부 반환
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
    /// 복셀이 월드 내부에 있는지 여부 반환
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
