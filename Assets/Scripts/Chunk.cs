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
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    //투명블럭의 삼각형 좌표 저장하는 리스트
    List<int> TransparentTriangles = new List<int>();

    //마테리얼 복수 적용을 위한 마테리얼 배열
    Material[] materials = new Material[2];

    //byte 값으로 구성된 맵, 블럭의 코드를 저장한다.
    public byte[,,] voxelMap = 
        new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

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
    /// 청크의 생성자, 즉시 초기화할지 여부를 결정가능
    /// </summary>
    /// <param name="_coord">생성될 청크의 좌표</param>
    /// <param name="_world">World에 대한 참조</param>
    /// <param name="genOnLoad">생성되자마자 Init을 호출 할지 여부</param>
    public Chunk (ChunkCoord _coord, World _world, bool genOnLoad)
	{
        this.world = _world;
        this.coord = _coord;
        IsActive = true;

        if(genOnLoad)
		{
            Init();
		}

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
        materials[0] = world.material;
        materials[1] = world.TransparentMaterial;

        meshRenderer.materials = materials;

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

        //RefreshChunkMeshData();

        //meshCollider.sharedMesh = meshFilter.mesh;
    }

    /// <summary>
    /// 복셀맵을 세팅한다.
    /// </summary>
    void PopulateVoxelMap(object obj)
	{
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    //GetVoxel로 해당 좌표의 블럭 아이디를 받아서 맵에 넣는다.
                    //이때 월드 기준 좌표를 넣어야 함에 유의하라
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }
        _RefreshChunkMeshData(null);
        IsMapInit = true;
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
        voxelMap[xP, yP, zP] = id;

        //EditVoxel은, 특별히 병렬 처리 안해도 됨
        _RefreshChunkMeshData(null);

        //인접한 청크 조건에 따라 갱신
        RefreshAdjacentChunk(xP, yP, zP);
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
                world.GetChunkFromVector3(temp + position).RefreshChunkMeshData();
			}
		}
	}

    /// <summary>
    /// 지정 좌표의 블록 isSolid를 반환
    /// </summary>
    /// <param name="pos">좌표</param>
    /// <returns></returns>
    bool CheckVoxel(Vector3 pos)
	{
        //좌표값 정수로
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //만약 복셀이 청크 내부에 있지 않으면 
        //청크의 좌표를 pos에 더해서 블럭 타입의 isSolid 참조
        //즉, 다른 청크의 블럭 여부를 확인하기 위함이다.
        //바깥면 여부를 판단할 때 필요함
        //Air는 isSolid가 false 이므로 그려지지 않을 것
        if (!IsVoxelInChunk(x, y, z))
            //월드 좌표로 변환함에 유의
            return world.CheckVoxelSolid(pos + position);

        //voxemMap에 저장된 블럭 타입을 참고로 World의 BlockType[]의 isSolid를 참조
        return world.blockTypes[voxelMap[x, y, z]].isSolid;
	}
    /// <summary>
    /// 지정 좌표의 블록 isTransparent를 반환
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
        //청크의 좌표를 pos에 더해서 블럭 타입의 isTransparent 참조
        //즉, 다른 청크의 블럭 여부를 확인하기 위함이다.
        //바깥면 여부를 판단할 때 필요함
        //Air는 isSolid가 false 이므로 그려지지 않을 것
        if (!IsVoxelInChunk(x, y, z))
            //월드 좌표로 변환함에 유의
            return world.CheckVoxelTransparent(pos + position);

        //voxemMap에 저장된 블럭 타입을 참고로 World의 BlockType[]의 isTransprent를 참조
        return world.blockTypes[voxelMap[x, y, z]].isTransparent;
    }

    /// <summary>
    /// 월드 기준 좌표를 받아서 자신의 맵 참조, 그 위치의 블럭 타입 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public byte GetVoxelFromGlobalVector3(Vector3 pos)
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
    /// <summary>
    /// 외부에서 참조할 예정
    /// </summary>
    public void RefreshChunkMeshData()
	{
        ThreadPool.QueueUserWorkItem(_RefreshChunkMeshData);
       
	}
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
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z] = v.id;


		}
        
        ClearMeshData();
        Vector3 temp = new Vector3();
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    //블럭이 Solid 일때만 그림
                    if(world.blockTypes[voxelMap[x, y, z]].isSolid)
					{
                        temp.x = x; temp.y = y; temp.z = z;
                        //AddVoxelDataToChunk(new Vector3(x, y, z));
                        AddVoxelDataToChunk(temp);
                    }
                        
                }
            }
        }

        //갱신을 완료한 뒤 그려낼 청크 목록에 추가한다.
        //한번에 한 스레드만 이 코드에 접근할 수 있다.
        lock(world.lockObject)
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
        TransparentTriangles.Clear();
        uvs.Clear();
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
            //만약 맵이 세팅되지 않았거나
            //스레드에 의해 간섭중이라면
            if(!IsMapInit || IsLockedMeshThread)
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
    /// </summary>
    /// <param name="pos">복셀 데이터의 위치</param>
    void AddVoxelDataToChunk(Vector3 pos)
	{
        //맵에서 인자로 넘어온 pos의 블럭 아이디를 조회한다.
        byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

        //pos좌표의 투명여부를 확인
        bool isTransparent = world.blockTypes[blockID].isTransparent;

        for (int p = 0; p < 6; p++)
        {
            //각 면의 방향으로 한칸 갔을 때 블럭이 없을 때만
            //(=즉, 보이는 면일때만 그린다.) 
            //if (!CheckVoxel(pos + VoxelData.faceChecks[p]))

            //---------수정됨--------------
            //각 면의 방향으로 한칸 간 곳에 있는 블럭이 투명블럭일때만 그린다.
            //Air는 투명블록이므로 그려지지 않는다.
            if(CheckVoxelTransparent(pos + VoxelData.faceChecks[p]))
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

                //만약 투명하지 않다면, 기본 삼각형 리스트에 넣는다.
                if (!isTransparent)
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
        mesh.subMeshCount = 2;
        
        //기본 마테리얼을 사용할 삼각형들
        mesh.SetTriangles(triangles.ToArray(), 0);

        //투명 마테리얼을 사용할 삼각형들
        mesh.SetTriangles(TransparentTriangles.ToArray(), 1);

        mesh.uv = uvs.ToArray();

        //큐브를 깔끔하게 그리기 위해 필요한 연산
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
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

