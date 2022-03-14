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


	void Start()
	{
        //시드 값에 따라 난수생성기 초기화
        //같은 시드는 같은 맵
        Random.InitState(seed);

        //월드 중앙에 스폰
        spawnPosition =
            new Vector3(VoxelData.WorldSizeInVoxels / 2f,
            VoxelData.ChunkHeight, VoxelData.WorldSizeInVoxels / 2f);
        

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
        //복셀의 0번 정점을 가리키는 정수 좌표
        int xP = Mathf.FloorToInt(_x);
        int yP = Mathf.FloorToInt(_y);
        int zP = Mathf.FloorToInt(_z);

        //복셀이 속한 청크의 좌표
        int xChunk = xP / VoxelData.ChunkWidth;
        int zChunk = zP / VoxelData.ChunkWidth;

        //절대 좌표를 각 청크의 상대좌표로 변환하는 과정
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
        정수의 나눗셈은 소수점이 날라가므로 xChunk에는 xP좌표가 속한 청크의 좌표, 
        즉 ChunkCoord의 x 좌표가 들어갈 것이다. zChunk도 마찬가지. 
        이것에 청크의 넓이를 곱해서 빼주는 것으로 청크 내에서의 좌표를 얻을 수 있다.
        */

    }

    //이 코드는 월드를 만들거나 동굴을 만들거나 하는 등의 알고리즘에 사용될 것
    //바이옴 등의 동작도 이곳에서 발생함
    /// <summary>
    /// 좌표를 받아서 해당 좌표의 블럭 ID를 반환
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
                
                CreateNewChunk(x, z);
			}
		}  
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
                        //만든다
                        CreateNewChunk(temp.x, temp.z);
                    //범위 내에 있는데 활성화 되어 있지 않다면
                    else if(!chunks[temp.x, temp.z].IsActive)
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
    /// 청크 좌표를 기반으로 청크 생성
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    void CreateNewChunk(int x, int z)
	{
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        
        //새 청크를 만들면 활성화 된 것에 추가
        activeChunks.Add(new ChunkCoord(x, z));
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
