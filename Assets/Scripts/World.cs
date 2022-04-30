using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using UnityEngine;
using System.Threading;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

//직렬화 가능한 BlockType 클래스
[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    //인접한 면을 그려야하는 지 여부
    public bool drawNearPlane;

    //투명도
    public float transparency;

    public byte MaxStackSize = 64;

    //인벤토리 등에서 사용될 아이콘
    public Sprite icon;

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
/// 청크의 경계를 넘나드는 구조물등의 정보를 위한 클래스
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
/// 게임 모드의 열거형
/// </summary>
public enum GameMode
{
    Creative,
    Survival,
    Debug
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

    //전역 밝기 조정
    [Range(0f, 0.93f)]
    public float globalLightLevel;

    //낮과 밤에 따른 배경 색
    public Color dayColor;
    public Color nightColor;

    //플레이어의 좌표 참조를 위한 변수
    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public Material TransparentMaterial;
    public BlockType[] blockTypes;

    //청크들의 배열
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    //활성화된 청크들 저장
    //초기 크기 (시야 범위 * 시야 범위)
    List<ChunkCoord> activeChunks = new List<ChunkCoord>(VoxelData.ViewDistanceInChunks * VoxelData.ViewDistanceInChunks);

    //플레이어의 현재 위치한 청크 캐싱
    public ChunkCoord playerChunkCoord;
    //플레이어가 마지막으로 위치한 청크
    ChunkCoord playerLastChunkCoord;

    //만들 청크들을 저장하는 리스트
    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>(VoxelData.ViewDistanceInChunks * VoxelData.ViewDistanceInChunks);
    
    //코루틴이 이미 실행중인지 여부 판단을 위한 변수
    //private bool IsCreateChunks;

    //구조물과 청크들의 업데이트 코루틴을 위한 변수
    private bool IsApplyingAll = false;

    //코루틴을 위한 캐싱 변수
    //private IEnumerator m_CreateChunks;

    //새 코루틴을 위한 캐싱 변수
    //private IEnumerator m_ApplyModifications;

    //구조물 생성을 위한 Que
    //구조물의 형태를 저장한 Queue를 저장하는 Queue
    public ConcurrentQueue<Queue<VoxelMod>> modifications = new ConcurrentQueue<Queue<VoxelMod>>();

    //구조물 생성 후 업데이트를 위한 리스트
    //업데이트할 청크 객체를 저장함
    public List<Chunk> chunksToRefresh = new List<Chunk>(80);

    //청크들을 갱신하는 스레드
    Thread RefreshChunksThread;

    //청크 갱신 스레드 작동 중 여부 검사
    private bool RefreshChunksThreadRunning = true;


    //그려낼 청크들 저장하는 큐
    //다른 스레드가 메쉬 데이터를 만들고 여기 넣으면
    //메인 스레드가 이것을 참고로 화면에 그려냄
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>(10);

    /// <summary>
    /// activeChunks가 수정 가능한지 여부 확인
    /// </summary>
    bool IsActiveChunksEditable = true;

    /// <summary>
    /// 현재 게임 모드를 나타내는 정적 변수
    /// </summary>
    public static readonly GameMode gameMode = GameMode.Debug;

	void Awake()
	{
        //프레임 60으로 고정
        Application.targetFrameRate = 75;
	}

	void Start()
	{
        //시드 값에 따라 난수생성기 초기화
        //같은 시드는 같은 맵
        Random.InitState(seed);

        //Global Light Level의 최대 최소값을 셰이더에 넘긴다.
        Shader.SetGlobalFloat("minGlobalLight", VoxelData.minLight);
        Shader.SetGlobalFloat("maxGlobalLight", VoxelData.maxLight);

        //청크들 갱신 스레드 시작
        RefreshChunksThread = new Thread(RefreshChunks_ThreadTask);
        RefreshChunksThread.Name = "RefreshCHunksThread";
        RefreshChunksThread.Start();

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

    long maxTicks = 0;
    long currentTicks = 0;
    long beforeTicks = 0;
    long afterTicks = 0;

    void Update()
	{
        
        //플레이어 현재 청크 갱신
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

		#region 테스트용 코드
		//"GlobalLightLevel"라는 변수에 globalLightLevel을 세팅한다.
		//모든 셰이더에 이 이름을 가진 변수를 찾는다.
		Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);

        //카메라의 배경색을 변경, 
        //낮의 색과 밤의 색 사이를 전역 밝기 만큼 선형 보간함
        //Camera.main.backgroundColor = Color.Lerp(dayColor, nightColor, globalLightLevel);
		#endregion


		//플레이어의 현재 위치한 청크가 마지막으로 위치한 청크와 다르다면
		if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            //lock (activeChunks)
            {
  
                CheckViewDistance();  //2022-04-25 기준 최대 40ms 지연

            }

            //다시 갱신
            playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        }
        
        //청크 초기화(Init)
        //한프레임에 하나씩만 (2022-04-25 기준 최대 3ms 지연)
        if (chunksToCreate.Count > 0)
            CreateChunk();


        
        //만약 그려낼 청크가 있다면
        //한프레임에 하나씩만
        if (chunksToDraw.Count > 0)
		{
            lock (chunksToDraw)
            {
                //만약 만들 청크가 맵이 구성되었고
                //스레드에 의해 수정중이 아니라면
                if (chunksToDraw.Peek().IsEditable)
                {
                    
                    chunksToDraw.Dequeue().ApplyChunkMesh(); //2022-04-25 기준 최대 11ms 지연
                    
                }
            }
		}
        
        //PrintTimeElapsed(beforeTicks, afterTicks);
    }

    void PrintTimeElapsed(long beforeTicks, long afterTicks)
	{
        currentTicks = afterTicks - beforeTicks;
        if (currentTicks > maxTicks)
        {
            maxTicks = currentTicks;
        }
        Debug.Log($"currentTicks : {currentTicks} ticks, {currentTicks / 10000} ms\nmaxTicks : {maxTicks} ticks, {maxTicks / 10000} ms");
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
        return CheckVoxelSolid(new Vector3(_x, _y, _z));
    }
    
    /// <summary>
    /// 월드 좌표를 받아서 그 좌표가 속한 청크의 맵을 참조해서
    /// 블럭의 isSolid를 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckVoxelSolid(Vector3 pos)
	{
        //pos가 속한 청크 좌표 불러옴
        ChunkCoord thisChunk = new ChunkCoord(pos);

        //좌표 유효 반환
        if (!IsVoxelInWorld(pos))
            return false;

        //지정된 좌표에 청크가 생성되었고, 청크의 맵이 초기화 되었다면
        if(chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
		{
            //지정된 좌표에 있는 블럭의 타입을 받아 isSolid 반환
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos).id].isSolid;
		}

        //만약에 위에 조건에 모두 해당이 없으면 GetVoxel을 호출해서 확인
        return blockTypes[GetVoxel(pos)].isSolid;
	}

    /// <summary>
    /// 월드 좌표를 받아서 그 좌표가 속한 청크의 맵을 참조,
    /// 이 블럭이 건너편이 비치는 블럭인지 여부를 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckVoxelTransparent(Vector3 pos)
    {
        //pos가 속한 청크 좌표 불러옴
        ChunkCoord thisChunk = new ChunkCoord(pos);

        //좌표 유효 반환
        if (!IsVoxelInWorld(pos))
            return false;

        //지정된 좌표에 청크가 생성되었고, 청크의 맵이 초기화 되었다면
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
        {
            //지정된 좌표에 있는 블럭의 타입을 받아 건너편이 비치는 지 반환
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos).id].drawNearPlane;
        }

        //만약에 위에 조건에 모두 해당이 없으면 GetVoxel을 호출해서 확인
        return blockTypes[GetVoxel(pos)].drawNearPlane;
    }

    /// <summary>
    /// 지정된 자표의 VoxelState 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public VoxelState GetVoxelState(Vector3 pos)
	{
        //pos가 속한 청크 좌표 불러옴
        ChunkCoord thisChunk = new ChunkCoord(pos);

        //좌표 유효하지 않으면 null 반환
        if (!IsVoxelInWorld(pos))
            return null;

        //지정된 좌표에 청크가 생성되었고, 청크의 맵이 초기화 되었다면
        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].IsEditable)
        {
            //지정된 좌표에 있는 VoxelState 반환
            return chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos);
        }

        //만약에 위에 조건에 모두 해당이 없으면 GetVoxel을 호출해서 생성한 뒤 반환
        return new VoxelState(GetVoxel(pos));
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


        //기본적인 맵의 틀
        //높이에 따라 블럭들 배치
        if (tempY == terHeight)
            vValue = 3;
        else if (tempY < terHeight && tempY > terHeight - 4)
            vValue = 4;
        else if (tempY > terHeight)
            vValue = 0;
        else
            vValue = 2;

        //Lode 반영 부분, 내부 블록들 설정
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

        //나무 반영 부분
        //지면에만 생성하기 위한 조건문
        if(tempY == terHeight)
		{
            //나무가 생성되는 범위를 설정
            if(Noise.GetPerlin2D(new Vector2(pos.x, pos.z), 0, biome.ForestScale) > biome.ForestThreshold)
			{
                //vValue = 1;
                //실제 나무가 생성되는 위치
                //이미 숲으로 설정된 상태에서 다시 Noise를 받아서 vValue를 바꾸었으므로
                //숲의 범위 안에 다시 분산도와 임계치에 따라 배치됨
                if (Noise.GetPerlin2D(new Vector2(pos.x, pos.z), 0, biome.TreeScale) > biome.TreeThreshold)
				{

                    //나무 형태가 들어있는 Queue<VoxelMod>를 받아서 modifications에 Enqueue한다.
                    //만에 하나를 위해 lock을 건다.
                    //lock (lockObject)
                    {
                        modifications.Enqueue(Structure.CreateTree(pos, biome.Min_TreeHeight, biome.Max_TreeHeight));
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

        //청크 좌표 캐싱
        ChunkCoord coord;

        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; 
            x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
		{
            
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks;
                z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
			{
                //캐싱한 청크 좌표 변형
                coord = new ChunkCoord(x, z);
                chunks[x, z] = new Chunk(coord, this);

                //만들 목록에 추가
                chunksToCreate.Add(coord);
			}
		}

        //시야 내를 활성화 하기 위해서
        CheckViewDistance();

        
    
    }

	/// <summary>
	/// 만들청크 큐에서 하나씩 꺼내 초기화하는 메소드
	/// 각 청크의 Init()을 호출하는 유일한 메소드
	/// </summary>
	void CreateChunk()
	{
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        //activeChunks.Add(c);
        chunks[c.x, c.z].Init();
	}

    //activeChunks에 추가할 좌표를 저장한다.
    Queue<ChunkCoord> coordToAddActiveChunks = new Queue<ChunkCoord>();

    /// <summary>
    /// 청크를 체크해서 업데이트 한다. chunksToRefresh 리스트 처리 (한번 실행에 하나씩)
    /// RefreshChunks_ThreadTask에서 호출됨
    /// </summary>
    void RefreshChunk()
	{
        //while루프를 제어하기 위해
        bool refreshed = false;
        int index = 0;

        //chunksToRefresh가 반복 도중 수정되는 것을 막기 위함
        lock (chunksToRefresh)
        {
            while (!refreshed && index < chunksToRefresh.Count - 1)
            {
                //만약 업데이트할 청크 목록의 청크가 맵이 세팅 되었다면
                //즉, GetVoxel을 호출해서 World.cs에 구조물을 추가했다면
                if (chunksToRefresh[index].IsEditable)
                {
                    //청크 업데이트
                    chunksToRefresh[index]._RefreshChunkMeshData(null);


                    //activeChunks에 추가할 좌표 저장
                    coordToAddActiveChunks.Enqueue(chunksToRefresh[index].coord);

                    chunksToRefresh.RemoveAt(index);
                    
                    //반복문 탈출
                    refreshed = true;
                }
                //청크가 맵이 세팅 되지 않았다면
                else
                {
                    //인덱스 증가하고 다시 확인
                    index++;
                }
            }
        }

        //chunksToRefresh의 lock 블럭을 최소화 하기 위해 바깥으로 빼낸 부분
        //activeChunks의 접근 가능을 체크한 뒤 activeChunks에 중복 없이 추가한다.
        //중복되어 들어가면 활성화/비활성화가 원활하지 않다.
        if(IsActiveChunksEditable)
		{
            lock(activeChunks)
			{
                while(coordToAddActiveChunks.Count != 0)
				{
                    ChunkCoord temp = coordToAddActiveChunks.Dequeue();
                    if (!activeChunks.Contains(temp))
                        activeChunks.Add(temp);
                }
                
            }
		}

	}

    /// <summary>
    /// modifications를 포함한 청크 초기화
    /// </summary>
    /// <returns></returns>
    void ApplyModifications()
	{
        //실행중
        IsApplyingAll = true;

        VoxelMod v;
        Queue<VoxelMod> que;
        while(modifications.Count > 0)
		{
            //Queue<Queue<VoxelMod>>에서 Queue<VoxelMod> 하나를 가져온다.
            //만약 ConcurrentQueue에서 가져오는 것을 실패했다면, 이번 갱신을 넘긴다.
            if (!modifications.TryDequeue(out que))
                break;

            //que에 대해 반복하며 실제 블럭들 반영
            while (que.Count > 0)
            {
                //VoxelMod를 받아옴
                v = que.Dequeue();

                //VoxelMod의 위치가 속한 청크의 좌표 받아옴
                ChunkCoord c = GetChunkCoordFromVector3(v.pos);

                //아래 코드는 생성할 구조물이 청크에 걸쳐있을 경우
                //잘리는 것을 방지하기 위함이다.
                //즉, 구조물이 청크에 걸치면 걸친 청크를 생성한다.
                //중복체크는 CheckViewDistance에서 한다.
                //만약 그 위치가 아직 생성되지 않은 청크라면
                if (chunks[c.x, c.z] == null)
                {
                    //생성하고 만들 목록에 추가
                    chunks[c.x, c.z] = new Chunk(c, this);
                    chunksToCreate.Add(c);
                }

                //World.cs에 있던 modifications들을 
                //위치에 맞는 청크들의 modifications에 넣어준다.
                chunks[c.x, c.z].modifications.Enqueue(v);

                //이미 Init()에서 추가함
                /*
                lock(chunksToRefresh)
				{
                    //만약 업데이트할 청크 목록에 현재 받아온 청크가 없다면
                    if (!chunksToRefresh.Contains(chunks[c.x, c.z]))
                    {
                        //추가한다.
                        chunksToRefresh.Add(chunks[c.x, c.z]);
                    }
                }
                */
            }
        }
        //종료됨
        IsApplyingAll = false;
    }

    /// <summary>
    /// 청크들의 구조물과 메쉬데이터를 업데이트 함
    /// 스레드에서 실행
    /// </summary>
    void RefreshChunks_ThreadTask()
	{
        while(RefreshChunksThreadRunning)
		{
            //구조물 생성부
            //이미 생성 중이 아니라면
            if (!IsApplyingAll)
            {
                ApplyModifications();
            }

            //청크 갱신
            //역시 한프레임에 하나씩만
            if (chunksToRefresh.Count > 0)
                RefreshChunk();

        }
	}

	private void OnDisable()
	{
        //비활성화 되면, 청크 갱신 스레드 중지
        RefreshChunksThreadRunning = false;

    }

	#region 청크 생성 코루틴 (삭제됨)
	/*
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
            
            //만들 청크 큐에서 청크를 초기화 시키고 큐에서 삭제
            chunks[chunksToCreateQue.Peek().x, chunksToCreateQue.Peek().z].Init();
            chunksToCreateQue.Dequeue();
            //한 프레임 동안 양보
            yield return null;
		}

        IsCreateChunks = false;
	}
    */
	#endregion

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
    /// 좌표를 받아서 그 좌표가 속한 청크 객체를 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Chunk GetChunkFromVector3(Vector3 pos)
	{
        //좌표값 정수로
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        
        return chunks[x,z];
    }

    //뷰 디스텐스를 갱신하기 전 활성화 되어 있던 청크들 저장
    List<ChunkCoord> prevActiveChunks;

    /// <summary>
    /// 플레이어의 좌표를 참조, 시야 범위내의 청크를 생성
    /// </summary>
    void CheckViewDistance()
	{

        //플레이어 위치의 청크 좌표를 구한다.
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);

        //activeChunks를 수정하는 동안은 잠금
        IsActiveChunksEditable = false;

        //activeChunks에 있는 ChunksCoord에 대한 참조들을 복사한다.
        //그리고 activeChunks의 참조를 삭제한다.
        prevActiveChunks = new List<ChunkCoord>(activeChunks);
        activeChunks.Clear();

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
                        chunks[x, z] = new Chunk(temp, this);
                        chunksToCreate.Add(temp);
                    }
                    //범위 내에 있는데 활성화 되어 있지 않다면
                    else if (!chunks[temp.x, temp.z].IsActive)
                    {
                        //활성화 시키고 활성화된 목록에 올린다.
                        chunks[temp.x, temp.z].IsActive = true;
                        
                    }
                    activeChunks.Add(temp);

                  
                }
                //이전 활성 목록에서 현재 시야에 있는 것들을 뺀다
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
        

        //위의 반복 후 남은 것들은 전에는 시야에 있었지만 현재엔 없는 것들이다.
        //따라서 비활성화 시킨다.
        foreach (ChunkCoord c in prevActiveChunks)
		{
        
            chunks[c.x, c.z].IsActive = false;
            
		}

        //잠금 해제
        IsActiveChunksEditable = true;
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
