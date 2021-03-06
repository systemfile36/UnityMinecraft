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


    //효과음 재생을 위해 각 블럭에서 상황마다 필요한 효과음 이름을 저장한다.
    //이 이름들은 직렬화 될 것
    //index는 각 효과음의 순회를 위한 변수로서 직렬화에서 제외된다.

    public string[] se_step = new string[4];

    public string[] se_placed = new string[4];

    public string[] se_breaked = new string[4];

    public string[] se_falled = new string[4];

    private int se_index = 0;

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

    /// <summary>
    /// 전달된 인자에 맞는 해당 블럭의 효과음 이름을 반환한다.
    /// </summary>
    public string GetSeName(BlockSESub seSub)
    {
        string seName; 
        
        switch(seSub)
        {
            case BlockSESub.Step:
                seName = se_step[se_index];
                break;
            case BlockSESub.Placed:
                seName = se_placed[se_index];
                break;
            case BlockSESub.Breaked:
                seName = se_breaked[se_index];
                break;
            case BlockSESub.Falled:
                seName = se_falled[se_index];
                break;
            default:
                seName = null;
                break;
        }

        //인덱스 순회(4개를 순회하기 위함)
        se_index++;
        if (se_index > 3)
            se_index = 0;

        return seName;
    }

}
/// <summary>
/// SE의 이름을 받아올 때 사용할 열거형
/// </summary>
public enum BlockSESub
{
    Step, Placed, Breaked, Falled
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

    //저장될 월드의 이름과 설정될 시드
    public static string worldName = "Default";
    public static int seed = 65535;


    /// <summary>
    /// World의 데이터가 저장되는 WorldData 클래스
    /// </summary>
    public WorldData worldData;

    /// <summary>
    /// 로딩 화면에 대한 참조
    /// </summary>
    public LoadingControl loadingControl;

    //바이옴들
    public BiomeAttributes[] biomes;

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

    /// <summary>
    /// 각 블럭의 id에 해당하는 BlockType 저장,
    /// id를 인덱스로 사용해서 접근
    /// </summary>
    public BlockType[] blockTypes;

    //청크들의 배열
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    //활성화된 청크들 저장
    //초기 크기 (시야 범위 * 시야 범위)
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    //플레이어의 현재 위치한 청크 캐싱
    public ChunkCoord playerChunkCoord;
    //플레이어가 마지막으로 위치한 청크
    ChunkCoord playerLastChunkCoord;

    //만들 청크들을 저장하는 리스트
    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();

    //구조물과 청크들의 업데이트 코루틴을 위한 변수
    private bool IsApplyingAll = false;

    //구조물 생성을 위한 Que
    //구조물의 형태를 저장한 Queue를 저장하는 Queue
    public ConcurrentQueue<Queue<VoxelMod>> modifications = new ConcurrentQueue<Queue<VoxelMod>>();

    //구조물 생성 후 업데이트를 위한 리스트
    //업데이트할 청크 객체를 저장함
    public List<Chunk> chunksToRefresh = new List<Chunk>(80);

    //청크들을 갱신하는 스레드
    Thread RefreshChunksThread;

    //청크 갱신 스레드 작동 컨트롤
    private bool RefreshChunksThreadRunning = true;

    /// <summary>
    /// EditVoxel을 통해 갱신된 청크들을 따로 갱신하기 위한 큐
    /// </summary>
    public ConcurrentQueue<Queue<Chunk>> chunksToRefresh_Edit = new ConcurrentQueue<Queue<Chunk>>();

    /// <summary>
    /// 이미 생성된 청크가 수정되었을 때 갱신하는 스레드
    /// </summary>
    Thread RefreshEditedChunksThread;

    /// <summary>
    /// RefreshEditedChunksThread의 작동 컨트롤
    /// </summary>
    private bool RefreshEditedChunksThreadRunning = true;

    //그려낼 청크들 저장하는 큐
    //다른 스레드가 메쉬 데이터를 만들고 여기 넣으면
    //메인 스레드가 이것을 참고로 화면에 그려냄
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>(10);

    //수정된 청크를 갱신하는 Queue
    public ConcurrentQueue<Queue<Chunk>> chunksToDraw_Edit = new ConcurrentQueue<Queue<Chunk>>(); 

    /// <summary>
    /// 언로드할 청크의 좌표 묶음을 저장해 놓은 Queue
    /// </summary>
    ConcurrentQueue<List<Vector2Int>> chunksToUnload = new ConcurrentQueue<List<Vector2Int>>();

    /// <summary>
    /// activeChunks가 수정 가능한지 여부 확인
    /// </summary>
    bool IsActiveChunksEditable = true;

    /// <summary>
    /// 현재 게임 모드를 나타내는 정적 변수
    /// </summary>
    public GameMode gameMode = GameMode.Debug;

    //ViewDistance가 런타임에 변경되었는지 확인하기 위한 변수
    int lastViewDistance;

    void Awake()
	{
        //GameManager에 현재 생성된 World Instance 할당
        if (GameManager.Mgr.World == null)
            GameManager.Mgr.World = this;
        else
            //이미 존재하면 새로 생성된거 파괴
            Destroy(this.gameObject);

        //worldData 인스턴스 할당
        //worldData = new WorldData(worldName, seed);

        //World를 로드한다.
        worldData = SaveManager.LoadWorldJson(worldName, seed);
        
        activeChunks.Capacity = (GameManager.Mgr.settings.ViewDistanceInChunks * 2) * (GameManager.Mgr.settings.ViewDistanceInChunks * 2);

        //마지막 시야 범위 초기화
        lastViewDistance = GameManager.Mgr.settings.ViewDistanceInChunks;

        //스폰 지점
        spawnPosition = GameManager.Mgr.settings.spawnPosition.GetVector3();

        //시드 값에 따라 난수생성기 초기화
        //같은 시드는 같은 맵
        Random.InitState(seed);
        
    }

    private void OnDestroy()
    {
        //파괴될때 자기 자신의 인스턴스가 할당되었다면 null로 초기화
        if (GameManager.Mgr != null)
            GameManager.Mgr.World = null;

        //종료할 때 저장함
        //SaveManager.SaveWorld(worldData);

    }

    void Start()
	{
        
        //로딩 화면을 구현하기 위해 Start() 메시지를 최대한 가볍게 만들고, 
        //실제 초기화 작업은 코루틴에서 하게 변경함 20220606

        //Global Light Level의 최대 최소값을 셰이더에 넘긴다.
        Shader.SetGlobalFloat("minGlobalLight", VoxelData.minLight);
        Shader.SetGlobalFloat("maxGlobalLight", VoxelData.maxLight);

        //플레이어가 위치한 청크 초기화
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

        //초기 값 IsCompleted == false 인 IAsyncResult 구현 객체
        //코루틴이 작업을 끝냈는지 여부를 확인하기 위함
        TempIAsyncResult initResult = new TempIAsyncResult();

        //IAsyncResult 구현 객체를 넘겨줌
        loadingControl.LoadingStart(initResult);

        //초기화 코루틴 시작
        StartCoroutine(InitWorld(initResult));

	}

    WaitForSecondsRealtime waitFor100ms = new WaitForSecondsRealtime(0.1f);

    /// <summary>
    /// 초기화 작업을 나누어서 실행하기 위한 코루틴
    /// TempIAsyncResult를 받아서 현재 작업 상황을 알림 
    /// </summary>
    /// <returns></returns>
    IEnumerator InitWorld(TempIAsyncResult result)
    {
        //처음 실행 시 게임 시간 정지
        Time.timeScale = 0f;

        //비동기로 진행할 초기화 테스크
        System.Action initAsyncTask = () =>
        {
            //로드 요청
            
            worldData.LoadAllChunks(Vector3Int.FloorToInt(spawnPosition));
            

            //구조물들 반영
            ApplyModifications();

            //초기 저장
            SaveManager.SaveWorldJson(worldData);
        };

        //실행하기 전에 딜레이를 주어야 제대로 실행됨
        yield return new WaitForSecondsRealtime(0.1f);

        //비동기 로딩 시작
        System.IAsyncResult temp = initAsyncTask.BeginInvoke(null, null);

        //로딩이 끝나지 않았다면 
        while(!temp.IsCompleted)
        {
            //100ms 대기
            yield return waitFor100ms;
            
        }

        //로딩이 끝났다면 비동기로 하기 어려운 초기화 실행
        //청크들 갱신 스레드 시작
        RefreshChunksThread = new Thread(RefreshChunks_ThreadTask);
        RefreshChunksThread.Name = "RefreshChunksThread";
        RefreshChunksThread.Start();

        //수정된 청크 갱신 스레드 시작
        RefreshEditedChunksThread = new Thread(RefreshEditedChunks_ThreadTask);
        RefreshEditedChunksThread.Name = "RefreshEditedChunksThread";
        RefreshEditedChunksThread.Start();

        //실제 오브젝트를 생성함
        GenerateWorld();

        //끝났음을 알림
        result._IsCompleted = true;

        //시간을 원래대로
        Time.timeScale = 1f;
    }

    private class TempIAsyncResult : System.IAsyncResult
    {
        public bool _IsCompleted = false;

        public object AsyncState => throw new System.NotImplementedException();

        public WaitHandle AsyncWaitHandle => throw new System.NotImplementedException();

        public bool CompletedSynchronously => throw new System.NotImplementedException();

        public bool IsCompleted
        {
            get { return _IsCompleted; }
        }
    }

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
                //비동기로 청크 로드를 요청한다.
                ThreadPool.QueueUserWorkItem((obj) => CheckLoadDistance());
                CheckViewDistance(); 

            }

            //다시 갱신
            playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        }
        
        //청크 초기화(Init) 한프레임에 하나
        if (chunksToCreate.Count > 0)
            CreateChunk();


        
        //만약 그려낼 청크가 있다면 한 프레임에 하나씩 그린다.
        if (chunksToDraw.Count > 0)
		{
            lock (chunksToDraw)
            {
                 chunksToDraw.Dequeue().ApplyChunkMesh(); 
            }
		}

        //수정된 청크는 따로 갱신하여 준다.
        Queue<Chunk> editedChunk;
        if(chunksToDraw_Edit.Count > 0 && chunksToDraw_Edit.TryDequeue(out editedChunk))
        {
            while(editedChunk.Count > 0)
            {
                editedChunk.Dequeue().ApplyChunkMesh();
            }
        }

        
        List<Vector2Int> unload;
        if(chunksToUnload.TryDequeue(out unload))
        {
                
            UnloadChunk(unload.ToArray());
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
        VoxelState temp = worldData.GetVoxelState(pos);

        if (temp != null)
            return blockTypes[temp.id].isSolid;
        else
            return false;
	}

    /// <summary>
    /// 월드 좌표를 받아서 그 좌표가 속한 청크의 맵을 참조,
    /// 이 블럭이 건너편이 비치는 블럭인지 여부를 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckVoxelTransparent(Vector3 pos)
    {
        VoxelState temp = worldData.GetVoxelState(pos);
        if (temp != null)
            return blockTypes[temp.id].drawNearPlane;
        else
            return false;
    }

    /// <summary>
    /// 지정된 자표의 VoxelState 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public VoxelState GetVoxelState(Vector3 pos)
	{
        return worldData.GetVoxelState(pos);   
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

        //Vector2 캐싱
        Vector2 pos2 = new Vector2(pos.x, pos.z);

        //바이옴 선택 부분
        //바이옴 출현에 대한 노이즈를 계산해서 가중치로 삼고, 
        //이것이 가장 높은 것을 선택한다.
        //https://blog.naver.com/loliisjinri/222737479286 참조

        float HeightSum = 0f;
        float highestWeight = 0f;
        int selectedBiomeIndex = 0;

        //바이옴들에 대해 순회
        for(int i = 0; i < biomes.Length; i++)
        {
            //해당 바이옴의 노이즈로 가중치 설정
            float weight = 
                Noise.GetPerlin2D(pos2, 
                    biomes[i].biomeOffset, biomes[i].biomeScale, seed);

            //가중치 최대값 갱신, 인덱스 갱신
            //제일 가중치가 큰 것을 선택하기 위함
            if (weight > highestWeight)
            {
                highestWeight = weight;
                selectedBiomeIndex = i;
            }

            //해당 바이옴 기준 높이를 구하고 가중치를 곱한다.
            float height = biomes[i].terHeight
                * Noise.GetPerlin2D(pos2, 0, biomes[i].terScale, seed)
                * weight;

            //평균을 구하기 위해 높이를 합한다.
            HeightSum += height;
            
        }

        //선택된 인덱스로 biome 설정
        BiomeAttributes biome = biomes[selectedBiomeIndex];

        //높이의 평균을 구한다.
        HeightSum /= biomes.Length;

        //바이옴들의 높이 평균과 고정 층을 더해서 
        //실제 높이로 결정
        int terHeight = Mathf.FloorToInt(HeightSum + BiomeAttributes.solidHeight);

        /* 단일 바이옴 일때의 코드
        //0 ~ 1의 값인 펄린노이즈에 높이를 곱해서 범위를 조정
        int terHeight = 
            Mathf.FloorToInt(biome.terHeight * Noise.GetPerlin2D(
                new Vector2(pos.x, pos.z), 0, biome.terScale))
            + BiomeAttributes.solidHeight;
        */


        //바로 리턴하면 제대로 배치되지 않음
        byte vValue = 0;


        //기본적인 맵의 틀
        //높이에 따라 블럭들 배치
        //biome에 따라 표면 블럭 변경
        if (tempY == terHeight)
            vValue = biome.surfaceBlockId;
        else if (tempY < terHeight && tempY > terHeight - 4)
            vValue = biome.b_surfaceBlockId;
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
                    if(Noise.GetPerlin3D(pos, lode.Offset, lode.scale, lode.threshold, seed))
					{
                        //vValue를 변형
                        vValue = lode.blockID;
					}
				}
			}
		}

        //구조물 반영 부분
        //지면에만 생성하기 위한 조건문
        if(tempY == terHeight && biome.isCreatePlants)
		{
            //구조물가 생성되는 범위를 설정
            if(Noise.GetPerlin2D(new Vector2(pos.x, pos.z), 0, biome.PlantSetScale, seed) > biome.PlantSetThreshold)
			{
                //vValue = 1;
                //실제 구조물이 생성되는 위치
                //이미 집합으로 설정된 상태에서 다시 Noise를 받아서 vValue를 바꾸었으므로
                //집합의 범위 안에 다시 분산도와 임계치에 따라 배치됨
                if (Noise.GetPerlin2D(pos2, 0, biome.PlantScale, seed) > biome.PlantThreshold)
				{

                    //식물 형태가 들어있는 Queue<VoxelMod>를 받아서 modifications에 Enqueue한다.
                    {
                        modifications.Enqueue(Structure.CreateMajorPlant(biome.PlantId, pos, biome.Min_TreeHeight, biome.Max_TreeHeight));
                    }
				}

			}
		}


        //그리고 리턴
        return vValue;
    }

    /// <summary>
    /// 스폰 지점을 기준으로 LoadDistanceInChunk만큼 청크를 로드한다.
    /// </summary>
    void LoadWorld()
    {
        
        for (int x = (VoxelData.WorldSizeInChunks / 2) - GameManager.Mgr.settings.LoadDistanceInChunks;
            x < (VoxelData.WorldSizeInChunks / 2) + GameManager.Mgr.settings.LoadDistanceInChunks; x++)
        {

            for (int z = (VoxelData.WorldSizeInChunks / 2) - GameManager.Mgr.settings.LoadDistanceInChunks;
                z < (VoxelData.WorldSizeInChunks / 2) + GameManager.Mgr.settings.LoadDistanceInChunks; z++)
            {
                //WorldData의 Dictionary에 해당 좌표의 ChunkData 로드를 요청한다.
                worldData.LoadChunks(new Vector2Int(x, z));
            }
        }

    }

    /// <summary>
    /// LoadWorld를 비동기로 실행함
    /// </summary>
    /// <returns></returns>
    System.IAsyncResult LoadWorldAsync()
    {
        System.Action action = LoadWorld;

        return action.BeginInvoke(null, null);
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

        for (int x = (VoxelData.WorldSizeInChunks / 2) - GameManager.Mgr.settings.ViewDistanceInChunks; 
            x < (VoxelData.WorldSizeInChunks / 2) + GameManager.Mgr.settings.ViewDistanceInChunks; x++)
		{
            
            for (int z = (VoxelData.WorldSizeInChunks / 2) - GameManager.Mgr.settings.ViewDistanceInChunks;
                z < (VoxelData.WorldSizeInChunks / 2) + GameManager.Mgr.settings.ViewDistanceInChunks; z++)
			{
                //캐싱한 청크 좌표 변형
                coord = new ChunkCoord(x, z);
                chunks[x, z] = new Chunk(coord);

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

        //chunksToRefresh가 반복 도중 수정되는 것을 막기 위함
        lock (chunksToRefresh)
        {
            //IsEditable을 체크하거나 하는 등의 과정은 필요하지 않다.20220529 기준

            //청크 업데이트
            chunksToRefresh[0]._RefreshChunkMeshData(null);


            //activeChunks에 추가할 좌표 저장
            coordToAddActiveChunks.Enqueue(chunksToRefresh[0].coord);

            chunksToRefresh.RemoveAt(0);

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
    /// modifications를 반영함
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

                //WorldData의 SetVoxel을 통해
                //실제 맵에 세팅한다.
                worldData.SetVoxel(v.pos, v.id);

                #region 기존의 구조물 반영 코드(삭제됨)
                /*
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
                    chunks[c.x, c.z] = new Chunk(c);
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
                #endregion
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

            Thread.Sleep(8);

        }
	}

    /// <summary>
    /// 이미 생성된 청크가 수정되었을 때 갱신하는 스레드 테스크
    /// </summary>
    void RefreshEditedChunks_ThreadTask()
	{
        //이미 생성된 청크를 대상으로 갱신하므로 activeChunks는
        //참조할 필요가 없다. 또한 IsEditable도 확인할 필요가 없다.

        //수정된 청크의 묶음
        Queue<Chunk> editedChunk;
        while(RefreshEditedChunksThreadRunning)
		{
            //갱신할 수정된 청크의 묶음이 1개 이상이고, Dequeue에 성공했다면
            if(chunksToRefresh_Edit.Count > 0 
                && chunksToRefresh_Edit.TryDequeue(out editedChunk))
			{
                //chunksToDraw_Edit에 추가할 Queue
                Queue<Chunk> addToDraw = new Queue<Chunk>();

                //그 묶음의 청크 전부 갱신
                while(editedChunk.Count > 0)
                {
                    Chunk c = editedChunk.Dequeue();   
                    c._RefreshChunkMeshData(true);
                    addToDraw.Enqueue(c);
                }

                //그려낼 목록에 추가
                chunksToDraw_Edit.Enqueue(addToDraw);
			}
		}
	}

	private void OnDisable()
	{
        //비활성화 되면, 청크 갱신 스레드 중지
        RefreshChunksThreadRunning = false;
        RefreshEditedChunksThreadRunning = false;
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
        for(int x = coord.x - GameManager.Mgr.settings.ViewDistanceInChunks; 
            x < coord.x + GameManager.Mgr.settings.ViewDistanceInChunks; x++)
		{
            for (int z = coord.z - GameManager.Mgr.settings.ViewDistanceInChunks;
            z < coord.z + GameManager.Mgr.settings.ViewDistanceInChunks; z++)
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
                        chunks[x, z] = new Chunk(temp);
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

        //마지막 시야범위가 현재 설정과 다르다면
        if (lastViewDistance != GameManager.Mgr.settings.ViewDistanceInChunks)
		{
            //activeChunks의 Capacity를 조정한다.
            activeChunks.TrimExcess();

            //갱신
            lastViewDistance = GameManager.Mgr.settings.ViewDistanceInChunks;
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
    /// 범위를 체크해서 WorldData의 딕셔너리에 로드 요청
    /// </summary>
    void CheckLoadDistance()
    {
        //현재 로드된 청크 딕셔너리의 키값만 깊은 복사로 받아온다.
        //즉, 갱신 전에 로드된 목록이다.
        var loadedChunks = new List<Vector2Int>(worldData.chunks.Keys);

        //플레이어 위치의 청크 좌표를 구한다.
        ChunkCoord coord = playerChunkCoord; //player.position은 메인 스레드에서만 가능!

        //로드 범위에 대해 반복
        for (int x = coord.x - GameManager.Mgr.settings.LoadDistanceInChunks;
            x < coord.x + GameManager.Mgr.settings.LoadDistanceInChunks; x++)
        {
            for (int z = coord.z - GameManager.Mgr.settings.LoadDistanceInChunks;
                z < coord.z + GameManager.Mgr.settings.LoadDistanceInChunks; z++)
            {
                Vector2Int temp = new Vector2Int(x, z);

                //청크 로드 요청
                //이미 로드되었다면 메소드가 알아서 무시한다.
                worldData.LoadChunks(temp);

                //갱신 전 목록과 현재 범위의 겹치는 부분을 뺀다.
                loadedChunks.Remove(temp);
            }
        }
        

        //loadedChunks에 남은 것들을 언로드할 목록에 추가한다.
        Debug.Log($"Enqueue loadedChunks {loadedChunks.Count}");
        chunksToUnload.Enqueue(loadedChunks);
    }

    /// <summary>
    /// 지정된 좌표의 청크를 언로드한다.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    void UnloadChunk(int x, int z)
    {
        if (!IsChunkInWorld(x, z))
            return;

        if (chunks[x, z] == null)
            return;

        chunks[x, z].DestroyGameObj();
        chunks[x, z] = null;
        worldData.UnloadChunks(new Vector2Int(x, z));
    }

    /// <summary>
    /// 청크 좌표를 배열로 받아 모두 언로드한다.
    /// </summary>
    /// <param name="poses"></param>
    void UnloadChunk(Vector2Int[] poses)
    {
        foreach(var pos in poses)
        {
            //예외
            if (!IsChunkInWorld(pos.x, pos.y))
                continue;

            //예외
            if (chunks[pos.x, pos.y] == null)
                continue;

            chunks[pos.x, pos.y].DestroyGameObj();
            chunks[pos.x, pos.y] = null;
        }

        //모두 언로드
        worldData.UnloadChunks(poses);
    }

    /// <summary>
    /// 지정된 청크 좌표에 있는 청크가 월드 범위 내에 있는지 여부 반환
    /// </summary>
    bool IsChunkInWorld(ChunkCoord coord)
	{
        if (coord.x >= 0 && coord.x < VoxelData.WorldSizeInChunks - 1
            && coord.z >= 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return false;
	}

    /// <summary>
    /// 지정된 청크 좌표에 있는 청크가 월드 범위 내에 있는지 여부 반환
    /// </summary>
    bool IsChunkInWorld(int x, int z)
    {
        if (x >= 0 && x < VoxelData.WorldSizeInChunks - 1
            && z >= 0 && z < VoxelData.WorldSizeInChunks - 1)
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
