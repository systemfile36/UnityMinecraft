using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

/// <summary>
/// 월드에서의 청크의 좌표
/// </summary>
public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord()
	{
        x = 0;
        z = 0;
	}

    public ChunkCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
    /// <summary>
    /// 좌표를 받아 그 좌표가 속한 청크의 좌표로 초기화
    /// </summary>
    /// <param name="pos"></param>
    public ChunkCoord(Vector3 pos)
	{
        int xP = Mathf.FloorToInt(pos.x);
        int zP = Mathf.FloorToInt(pos.z);

        x = xP / VoxelData.ChunkWidth;
        z = zP / VoxelData.ChunkWidth;
	}
	//비교문을 위한 Equals 오버라이딩
	public override bool Equals(object obj)
	{
        ChunkCoord c = obj as ChunkCoord;
        if (c == null)
            return false;

        if (c.x == x && c.z == z)
            return true;
        else
            return false;
	}
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}

/// <summary>
/// 각 블럭의 상태를 저장한다.
/// 맵에 저장될 타입
/// </summary>
public class VoxelState
{
    public byte id;

    //이 블럭이 받는 빛의 양
    public float globalLightWeight;

    public VoxelState()
	{
        id = 0;
        globalLightWeight = 0f;
	}

    public VoxelState(byte id)
	{
        this.id = id;
        globalLightWeight = 0f;
	}
}

/// <summary>
/// 청크의 기본 틀, 매쉬 정보를 가진 오브젝트와 청크 내의 맵 정보를 가진다.
/// </summary>
public class Chunk
{
    public ChunkCoord coord;

    //메쉬 필터와 메쉬 렌더러를 얻기 위함이다.
    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    //MeshCollider meshCollider;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>(20000);
    List<int> triangles = new List<int>(20000);
    List<Vector2> uvs = new List<Vector2>(20000);

    //정점의 색을 저장, 셰이더에 넘겨줄 것이다.
    List<Color> colors = new List<Color>();

    //투명블럭의 삼각형 좌표 저장하는 리스트
    List<int> TransparentTriangles = new List<int>();

    //마테리얼 복수 적용을 위한 마테리얼 배열
    Material[] materials = new Material[2];

    //byte 값으로 구성된 맵, 블럭의 VoxelState를 저장한다.
    public VoxelState[,,] voxelMap = 
        new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    //블럭 타입등의 참조를 위한 World 참조
    World world;

    private bool _IsActive;

    //voxelMap이 초기화 되었는지 여부
    private bool IsMapInit = false;

    //매쉬데이터를 추가하는 스레드가 작동 중인지 여부
    public bool IsLockedMeshThread = false;

    //World 스크립트에서 직접 추가하기 위해서 public으로 선언
    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    /// <summary>
    /// 청크의 월드 기준 좌표
    /// </summary>
    public Vector3 position;

    //World를 인자로 받는다.(find는 비싼(expansive한 작업))
    /// <summary>
    /// 청크의 생성자
    /// </summary>
    /// <param name="_coord">생성될 청크의 좌표</param>
    /// <param name="_world">World에 대한 참조</param>
    public Chunk (ChunkCoord coord, World world)
	{
        this.world = world;
        this.coord = coord;
        //IsActive = true;
    }

    /// <summary>
    /// 청크를 초기화. 맵을 세팅하고 메쉬를 만들어서 추가하는 과정
    /// </summary>
    public void Init()
	{
        
        chunkObject = new GameObject();
        //IsActive = true;
        //인스펙터에서 하던걸 코드로 옮긴 것이다.
        //chunkObject에 메쉬 필터와 메쉬 렌더러를 추가하고 변수에 저장한다.
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        //마테리얼을 설정한다.
        //meshRenderer.material = world.material;

        //투명 블럭 구현을 위한 마테리얼 여러개 설정
        //materials[0] = world.material;
        //materials[1] = world.TransparentMaterial;

        //일단은, 하나의 마테리얼로
        meshRenderer.material = world.material;

        //meshCollider = chunkObject.AddComponent<MeshCollider>();

        //보기 좋기 위해 부모를 설정하도록 하자.
        chunkObject.transform.SetParent(world.transform);
        //상대위치ChunkCoord를 기반으로 실제 위치 반영
        chunkObject.transform.position
            = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = string.Format("Chunk {0}, {1}", coord.x, coord.z);

        //청크의 월드 위치 초기화
        position = chunkObject.transform.position;

        //스레드 풀에 복셀 맵 세팅 메소드를 넣는다.
        ThreadPool.QueueUserWorkItem(PopulateVoxelMap);

    }

    /// <summary>
    /// 복셀맵을 세팅한다.
    /// </summary>
    void PopulateVoxelMap(object obj)
	{
        //y 순방향 루프를 먼저 함으로서 아래부터 위로 설정되어 감
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    //GetVoxel로 해당 좌표의 블럭 아이디를 받아서 맵에 넣는다.
                    //이때 월드 기준 좌표를 넣어야 함에 유의하라
                    voxelMap[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + position));
                }
            }
        }
        //_RefreshChunkMeshData(null);

        IsMapInit = true;

        //세팅하고 갱신할 목록에 넣는다.
        lock (world.chunksToRefresh)
        {
            world.chunksToRefresh.Add(this);
        }

        
    }
    /// <summary>
    /// 복셀이 청크 내부에 있는지 여부 반환
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    bool IsVoxelInChunk(int x, int y, int z)
	{
        if (x < 0 || x >= VoxelData.ChunkWidth
            || y < 0 || y >= VoxelData.ChunkHeight
            || z < 0 || z >= VoxelData.ChunkWidth)
            return false;
        else
            return true;
    }

    /// <summary>
    /// 지정된 좌표의 블럭을 id로 바꾼다.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="id"></param>
    public void EditVoxel(Vector3 pos, byte id)
	{
        //좌표값 정수로
        int xP = Mathf.FloorToInt(pos.x);
        int yP = Mathf.FloorToInt(pos.y);
        int zP = Mathf.FloorToInt(pos.z);

        //월드 기준 좌표를 이 청크의 맵에 사용되는 좌표로 변환
        //청크의 절대 좌표를 빼는 것으로 가능함
        xP -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zP -= Mathf.FloorToInt(chunkObject.transform.position.z);


        //맵에 저장된 id를 변경
        voxelMap[xP, yP, zP].id = id;

        //EditVoxel은, 특별히 병렬 처리 안해도 됨
        //_RefreshChunkMeshData(null);

        lock(world.chunksToRefresh)
		{
            //블럭 수정은 우선적으로 처리하기 위하여 맨 앞에 넣는다.
            world.chunksToRefresh.Insert(0, this);
            RefreshAdjacentChunk(xP, yP, zP);
        }

        //인접한 청크 조건에 따라 갱신
        //RefreshAdjacentChunk(xP, yP, zP);
    }
    /// <summary>
    /// 수정한 블럭의 좌표를 인자로 받아 그 블럭과 인접한 청크 갱신
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    private void RefreshAdjacentChunk(int x, int y, int z)
	{
        Vector3 tempV = new Vector3(x, y, z);

        //수정된 블럭의 각 면의 방향에 대하여 반복
        for(int p = 0; p < 6; p++)
		{
            //각 면의 방향으로 한칸 이동한 좌표
            Vector3 temp = tempV + VoxelData.faceChecks[p];

            //수정된 블럭에서 한칸 씩 이동해서 그 블럭이 청크 내부에 있는지 체크한다.
            if (!IsVoxelInChunk((int)temp.x, (int)temp.y, (int)temp.z))
			{
                //만약 내부에 없다면, 다른 청크에 있고, 수정된 블럭과 접해있다는 뜻
                //따라서 그 청크를 갱신한다.
                //world.GetChunkFromVector3(temp + position).RefreshChunkMeshData();
                
                //lock은 이미 호출하는 EditVoxel에서 이미 걸려 있음
                world.chunksToRefresh.Insert(0, world.GetChunkFromVector3(temp + position));
			}
		}
	}

    /// <summary>
    /// 지정 좌표의 VoxelState 반환
    /// </summary>
    /// <param name="pos">좌표</param>
    /// <returns></returns>
    VoxelState GetVoxelState(Vector3 pos)
	{
        //좌표값 정수로
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //만약 복셀이 청크 내부에 있지 않으면 
        //청크의 좌표를 pos에 더해서 world의 GetVoxelState 호출
        //즉, 다른 청크의 블럭이면 World로 판단 유보
        if (!IsVoxelInChunk(x, y, z))
            //월드 좌표로 변환함에 유의
            return world.GetVoxelState(pos + position);

        //voxemMap에 저장된 VoxelState 반환
        return voxelMap[x, y, z];
	}
    #region CheckVoxelTransparent (삭제됨)
    /*
    /// <summary>
    /// 지정 좌표의 블록 drawNearPlane 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool CheckVoxelTransparent(Vector3 pos)
    {
        //좌표값 정수로
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //만약 복셀이 청크 내부에 있지 않으면 
        //청크의 좌표를 pos에 더해서 블럭 타입의 drawNearPlane 참조
        //즉, 다른 청크의 블럭일 경우엔 World 클래스로 판단을 넘긴다.
        //바깥면 여부를 판단할 때 필요함
        //Air는 drawNearPlane가 true 이므로 그려지지 않을 것
        if (!IsVoxelInChunk(x, y, z))
            //월드 좌표로 변환함에 유의
            return world.CheckVoxelTransparent(pos + position);

        //voxemMap에 저장된 블럭 타입을 참고로 World의 BlockType[]의 drawNearPlane 참조
        return world.blockTypes[voxelMap[x, y, z].id].drawNearPlane;
    }
    */
	#endregion

	/// <summary>
	/// 월드 기준 좌표를 받아서 자신의 맵 참조, 그 위치의 VoxelState 반환
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public VoxelState GetVoxelFromGlobalVector3(Vector3 pos)
	{
        //좌표값 정수로
        int xP = Mathf.FloorToInt(pos.x);
        int yP = Mathf.FloorToInt(pos.y);
        int zP = Mathf.FloorToInt(pos.z);

        //월드 기준 좌표를 이 청크의 맵에 사용되는 좌표로 변환
        //청크의 절대 좌표를 빼는 것으로 가능함
        xP -= Mathf.FloorToInt(position.x);
        zP -= Mathf.FloorToInt(position.z);

        return voxelMap[xP, yP, zP];

    }
    /*
    /// <summary>
    /// 외부에서 참조할 예정, 스레드 큐에 청크 메쉬 데이터 업데이트를 맡김
    /// </summary>
    public void RefreshChunkMeshData()
	{
        ThreadPool.QueueUserWorkItem(_RefreshChunkMeshData);
       
	}
    */
    /// <summary>
    /// 메쉬 데이터를 갱신한다.
    /// WaitCallback()형식에 맞추기 위해 object 받음
    /// 데이터 세팅 후 chunksToDraw에 자신을 Enqueue
    /// </summary>
    public void _RefreshChunkMeshData(object obj)
	{
        //스레드가 작동중임을 알려 맵 수정을 잠근다.
        IsLockedMeshThread = true;

        //구조물 세팅 부분
        VoxelMod v;
        //큐가 빌때까지

        Vector3 pos;

        while(modifications.Count > 0)
		{
            //큐에서 하나를 꺼낸다.
            v = modifications.Dequeue();

            //월드 좌표를 청크 내의 좌표로 변환
            pos = v.pos - position;

            //지정된 위치의 id를 세팅
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].id = v.id;


		}
        
        ClearMeshData();

        //빛을 계산하여 세팅한다.
        CalcLighting();  

        Vector3 temp = new Vector3();
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    //블럭이 Solid 일때만 그림
                    if(world.blockTypes[voxelMap[x, y, z].id].isSolid)
					{
                        temp.x = x; temp.y = y; temp.z = z;
                        //AddVoxelDataToChunk(new Vector3(x, y, z));
                        AddVoxelDataToChunk(temp);
                    }
                        
                }
            }
        }

        //갱신을 완료한 뒤 그려낼 청크 목록에 추가한다.

        lock (world.chunksToDraw)
        { 
            world.chunksToDraw.Enqueue(this);
        }
       
        

        //플래그 리셋
        IsLockedMeshThread = false;

        //ApplyChunkMesh();
    }

    /// <summary>
    /// 메쉬 데이터를 초기화 한다. 갱신을 위함
    /// </summary>
    void ClearMeshData()
	{
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        //TransparentTriangles.Clear();
        uvs.Clear();

        colors.Clear();
	}

    /// <summary>
    /// 빛의 감쇠를 적용할 블럭들의 좌표들
    /// </summary>
    Queue<Vector3Int> lit_Voxels = new Queue<Vector3Int>(VoxelData.ChunkWidth * VoxelData.ChunkWidth);

    /// <summary>
    /// 각 블럭의 투명도에 따라 빛을 계산하여 맵에 세팅한다.
    /// 후에 최적화 필요
    /// </summary>
    void CalcLighting()
    {
        #region 빛 계산 방식 설명
        /*
        x = 0, z = 0 위치에 있는 블럭들을 위에서부터 순회한다.
        y = 최대 높이에서 빛을 최대치로 설정하고,
        아래로 내려가면서 만나는 블럭들의 투명도에 따라 빛을 설정한다.
        그리고 각 블럭들이 받는 빛의 양을 설정한다.
        예를 들어, y = 100 위치에 투명도가 0인 블럭이 있다면
        빛의 양은 0이 되어, 그 아래에 있는 블럭(y=99)이 받는 빛의 양은 0이 될것이다.
        그리고 y = 0 까지 반복이 끝나면 x = 1, z = 0으로 간다.
        여기서 또 같은 일을 반복해서 최종적으로 모든 맵의 블럭에 받는 빛의 양을 설정하게 된다.

        이때, Air블럭은 빛의 양에 변화를 주지 않음을 기억하라!
        즉, Air블럭은 자신의 위에 블럭이 갖는 빛의 양을 자기 자신의 빛의 양으로 가지는 것이다.
        이것은 어두운 부분에 있는 블럭들의 보이는 면에 전부 그림자를 적용시키기 위함이다.
        각 면의 방향으로 나아간 곳에 있는 Air블럭이 음지에 있다면 그 면 역시 음지에 있다.
        반대로 양지에 있다면, 그 면 역시 양지에 있는 것이다.﻿
        */
        #endregion

        //빛이 계산될 때마다 비우고 다시 시작
        lit_Voxels.Clear();

        //현재 순회중인 블럭 캐싱
        VoxelState currentVoxel;

        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.ChunkWidth; z++)
            {
                //광원, 해당 x, z 좌표의 y 순회가 끝날때마다 초기화 된다.
                float lightRay = 1f;

                //위에서 부터 순회하기 위한 y 역순 루프이다.
                for (int y = VoxelData.ChunkHeight - 1; y >=0 ; y--)
                {
                    currentVoxel = voxelMap[x, y, z];

                    //Air블럭이 아닐 경우, 현재 빛을 블럭의 투명도 만큼으로 설정한다.
                    //투명하면 1, 불투명하면 불투명할수록 0에 가까워 질것이다.
                    //그리고 빛은 무조건 감쇄되는 방향으로 변해야 한다.
                    //이를 위해 투명도가 현재 빛의 양보다 낮을때만 갱신한다.
                    if(currentVoxel.id > 0 && world.blockTypes[currentVoxel.id].transparency < lightRay)
					{
                        lightRay = world.blockTypes[currentVoxel.id].transparency;
					}

                    //현재 순회중인 블럭이 받는 빛의 양을 설정한다.
                    currentVoxel.globalLightWeight = lightRay;

                    //다시 맵에 설정한다.
                    voxelMap[x, y, z] = currentVoxel;

                    //빛을 감쇠 시킬 수 있을 만큼 밝은 블럭만 넣는다.
                    //즉, 감쇠 시킬 필요가 있는 블럭만 넣는다.
                    if(lightRay > VoxelData.lightFallOff)
					{
                        lit_Voxels.Enqueue(new Vector3Int(x, y, z));
					}
                }
            }

        }

        //빛 감쇠를 적용할 블럭에 대해 순회한다.
        Vector3Int v;
        Vector3 currentLitVoxel;
        Vector3Int nearVoxel;
        while (lit_Voxels.Count > 0)
		{
            //Queue에서 하나를 빼온다.
            v = lit_Voxels.Dequeue();

            //각 면에 대해 반복
            for(int p = 0; p < 6; p++)
			{
                //불투명한 블럭의 아랫면에 대해서는 빛의 감쇠를 적용하지 않는다.
                //안그러면 불투명한 블럭의 아래에도 빛이 침투한다.
                if (p == 3)
                    continue;

                //각 면에 인접한 블럭의 좌표 받아옴
                currentLitVoxel = v + VoxelData.faceChecks[p];

                //Vector3를 Vector3Int로 변환
                nearVoxel = Vector3Int.FloorToInt(currentLitVoxel);

                //인접 블럭이 청크 내에 있을 때만 계산
                //청크 경계를 넘는 빛은 후에 구현할 예정
                if(IsVoxelInChunk(nearVoxel.x, nearVoxel.y, nearVoxel.z))
				{
                    //자신의 밝기 - 감쇠치 보다 낮은 밝기를 가지는 인접블럭에만
                    //빛의 감쇠를 적용한다.
                    //즉, 이미 감소치를 적용한 밝기보다 어두운 블럭에 대해서만 적용한다.
                    if(voxelMap[nearVoxel.x, nearVoxel.y, nearVoxel.z].globalLightWeight 
                        < voxelMap[v.x, v.y, v.z].globalLightWeight - VoxelData.lightFallOff)
					{
                        //인접 블럭의 밝기를 현재 블럭- 감쇠치만큼으로 만듬
                        voxelMap[nearVoxel.x, nearVoxel.y, nearVoxel.z].globalLightWeight =
                            voxelMap[v.x, v.y, v.z].globalLightWeight - VoxelData.lightFallOff;

                        //빛을 한 단계 감쇠시켰음에도 아직 감쇠의 여지가 있다면
                        //다시 감쇠시킬 목록에 추가한다.
                        if(voxelMap[nearVoxel.x, nearVoxel.y, nearVoxel.z].globalLightWeight
                            > VoxelData.lightFallOff)
						{
                            lit_Voxels.Enqueue(nearVoxel);
						}
                    }
				}
			}
		}

    }

    /// <summary>
    /// 청크의 활성화 여부를 가져오거나 설정함
    /// </summary>
    public bool IsActive
	{
        get
		{
            return _IsActive;
		}
        set
		{
            _IsActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
		}
	}
    
    /// <summary>
    /// 맵이 세팅되었는지 여부와 메쉬 데이터가 스레드에 의해
    /// 수정중인지 여부를 체크하여 반환
    /// </summary>
    public bool IsEditable
	{
        get
		{
            //만약 맵이 세팅되지 않았다면
            if(!IsMapInit)
			{
                return false;
			}
            else
			{
                return true;
			}
		}
	}

    /// <summary>
    /// 위치값을 받아 복셀 데이터를 메쉬 데이터 리스트에 추가합니다.
    /// 실제 Mesh를 구성하는 정점과 폴리곤, uv, color등의 정보가 추가되는 곳
    /// </summary>
    /// <param name="pos">복셀 데이터의 위치</param>
    void AddVoxelDataToChunk(Vector3 pos)
	{
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //맵에서 인자로 넘어온 pos의 블럭 아이디를 조회한다.
        byte blockID = voxelMap[x, y, z].id;

        //pos좌표의 블럭이 건너편이 비치는 지 여부를 확인
        //bool drawNearPlane = world.blockTypes[blockID].drawNearPlane;

        //각 정점의 밝기 값
        float lightLevel = 0;
        //Vector3 ShadeTemp = new Vector3(pos.x, pos.y, pos.z);

        //인접 블럭 캐싱
        VoxelState nearVoxel;

        for (int p = 0; p < 6; p++)
        {
            //각 면의 방향으로 한칸 간 곳에 있는 블럭의 VoxelState를 받아온다.
            nearVoxel = GetVoxelState(pos + VoxelData.faceChecks[p]);

            //---------수정됨--------------
            //각 면의 방향으로 한칸 간 곳에 있는 블럭이 투명블럭일때만 그린다.
            //Air는 투명블록이므로 그려지지 않는다.
            //if(CheckVoxelTransparent(pos + VoxelData.faceChecks[p]))
            //------------------------------

            //인접 블럭이 유효하고, 주변이 비치는 블럭일때만 그 면을 그린다.
            if(nearVoxel != null && world.blockTypes[nearVoxel.id].drawNearPlane)
            {
                
                //개인적으로 보기 편해서 for문은 안썻다.
                //정점 4개과 그에 맞는 uv를 넣는다.
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                //p의 값은 0 ~ 5로 변화하면 각 면을 그린다.
                //그에 따른 순서도 맞추어져 있으므로 faceIndex로 p를 넘긴다.
                AddTexture(world.blockTypes[blockID].GetTextureID(p));

                #region 구 그림자 부분 (삭제됨)
                /*
				//현재 그리는 면이 +Y축 방향 면이고, 비치지 않는 블럭일때만 그림자 그림
				//각 면의 p인덱스는 VoxelData.cs의 voxelTris 배열을 참고하라
				if (p == 2 && !drawNearPlane)
                {
                    //단순히 위로 한칸씩 가보면서 비치지 않는 블럭이 있는지 체크하고
                    //있으면 그림자를 그리고 아니면 그리지 않는다.
                    float yPos = pos.y + 1;
                    bool IsShade = false;

                    //맵의 한계 y까지 체크한다.
                    while (yPos < VoxelData.ChunkHeight)
                    {
                        ShadeTemp.y = yPos;
                        //위에 불투명한 블럭이나, 나뭇잎(11번)이 있으면 그림자를 그림
                        if (!CheckVoxelTransparent(ShadeTemp) 
                            || voxelMap[x, (int)yPos, z].id == 11)
                        {
                            IsShade = true;
                            break;
                        }
                        yPos++;
                    }
                    if (IsShade)
                        lightLevel = 0.6f;
                    else
                        lightLevel = 0.0f;
                }
                */
                #endregion


                //이 면과 인접한 블럭이 받는 빛의 양을 현재 면의 밝기로 한다.
                //즉, 맞닿아 있는 Air블럭이 받는 빛이 0이라면, 이 면은 어두운 면이 된다.
                //빛을 계산하는 부분은 CalcLighting 메소드 참조
                lightLevel = nearVoxel.globalLightWeight;

                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));


                //만약 투명하지 않다면, 기본 삼각형 리스트에 넣는다.
                //if (!isTransparent)
                {
                    //삼각형의 각 꼭짓점을 정점 4개에 맞게 정수로 넣는다.
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }
                //투명이 아니면 투명 삼각형 배열에 넣는다.
                /*
                else
				{
                    //삼각형의 각 꼭짓점을 정점 4개에 맞게 정수로 넣는다.
                    TransparentTriangles.Add(vertexIndex);
                    TransparentTriangles.Add(vertexIndex + 1);
                    TransparentTriangles.Add(vertexIndex + 2);
                    TransparentTriangles.Add(vertexIndex + 2);
                    TransparentTriangles.Add(vertexIndex + 1);
                    TransparentTriangles.Add(vertexIndex + 3);
                }
                */
                vertexIndex += 4;
            }
        }
        
    }

    /// <summary>
    /// 청크의 메쉬를 만듭니다.(메쉬 데이터 반영)
    /// 스레딩을 위해 World.cs에서 참조 예정
    /// </summary>
    public void ApplyChunkMesh()
	{
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        //mesh.triangles = triangles.ToArray();

        //이 메쉬에 사용할 마테리얼의 개수 설정
        //mesh.subMeshCount = 2;

        //기본 마테리얼을 사용할 삼각형들
        //mesh.SetTriangles(triangles.ToArray(), 0);

        //투명 마테리얼을 사용할 삼각형들
        //mesh.SetTriangles(TransparentTriangles.ToArray(), 1);

        mesh.triangles = triangles.ToArray();

        mesh.uv = uvs.ToArray();

        //정점의 색을 지정
        mesh.colors = colors.ToArray();

        //큐브를 깔끔하게 그리기 위해 필요한 연산
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        //Debug.Log($"Chunk : {coord.x} : {coord.z}, vert : {vertices.Count}, tri : {triangles.Count}, uvs : {uvs.Count}, col : {colors.Count}");

    }

    /// <summary>
    /// 텍스쳐 ID를 받아 uv값을 추가합니다.
    /// </summary>
    /// <param name="textureID"></param>
    void AddTexture(int textureID)
	{
        //텍스쳐ID에 따른 x, y좌표
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);
     
        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;
        
        //좌표를 UV의 배치에 맞게 변경해주는 과정
        y = 1f - y - VoxelData.NormalizedBlockTextureSize;
        
        //미리 설정된 순서에 따라(uv 테이블이 삼각형에 맞춰져 있으므로)
        uvs.Add(new Vector2(x, y)); //0, 0
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize)); //0, 1
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y)); //1, 0
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize)); //1, 1
    }

}

