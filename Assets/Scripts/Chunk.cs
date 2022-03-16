using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        else if (c.x == x && c.z == z)
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

    //byte 값으로 구성된 맵, 블럭의 코드를 저장한다.
    public byte[,,] voxelMap = 
        new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    //블럭 타입등의 참조를 위한 World 참조
    World world;

    private bool _IsActive;

    //voxelMap이 초기화 되었는지 여부
    public bool IsMapInit = false;

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

        //인스펙터에서 하던걸 코드로 옮긴 것이다.
        //chunkObject에 메쉬 필터와 메쉬 렌더러를 추가하고 변수에 저장한다.
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        //마테리얼을 설정한다.
        meshRenderer.material = world.material;

        //meshCollider = chunkObject.AddComponent<MeshCollider>();

        //보기 좋기 위해 부모를 설정하도록 하자.
        chunkObject.transform.SetParent(world.transform);
        //상대위치ChunkCoord를 기반으로 실제 위치 반영
        chunkObject.transform.position
            = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = string.Format("Chunk {0}, {1}", coord.x, coord.z);

        PopulateVoxelMap();
        RefreshChunkMeshData();

        //meshCollider.sharedMesh = meshFilter.mesh;
    }

    /// <summary>
    /// 복셀맵을 세팅한다.
    /// </summary>
    void PopulateVoxelMap()
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
    /// 지정 좌표의 블록 유무를 반환
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
            return world.CheckForVoxel(pos + position);

        //voxemMap에 저장된 블럭 타입을 참고로 World의 BlockType[]의 isSolid를 참조
        return world.blockTypes[voxelMap[x, y, z]].isSolid;
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
        xP -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zP -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xP, yP, zP];

    }

    /// <summary>
    /// 메쉬 데이터를 갱신한다.
    /// </summary>
    void RefreshChunkMeshData()
	{
        ClearMeshData();
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    //블럭이 Solid 일때만 그림
                    if(world.blockTypes[voxelMap[x, y, z]].isSolid)
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
        ApplyChunkMesh();
    }

    /// <summary>
    /// 메쉬 데이터를 초기화 한다. 갱신을 위함
    /// </summary>
    void ClearMeshData()
	{
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
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
    /// 청크의 좌표 반환
    /// </summary>
    public Vector3 position
	{
        get
		{
            return chunkObject.transform.position;
		}
	}

    /// <summary>
    /// 위치값을 받아 복셀 데이터를 메쉬 데이터 리스트에 추가합니다.
    /// </summary>
    /// <param name="pos">복셀 데이터의 위치</param>
    void AddVoxelDataToChunk(Vector3 pos)
	{
        //블럭이 있을때만 매쉬 데이터에 넣는다.
        if (CheckVoxel(pos))
        {
            for (int p = 0; p < 6; p++)
            {
                //각 면의 방향으로 한칸 갔을 때 블럭이 없을 때만
                //(=즉, 보이는 면일때만 그린다.
                if (!CheckVoxel(pos + VoxelData.faceChecks[p]))
                {
                    //맵에서 인자로 넘어온 pos의 블럭 아이디를 조회한다.
                    byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
                    //개인적으로 보기 편해서 for문은 안썻다.
                    //정점 4개과 그에 맞는 uv를 넣는다.
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                    //p의 값은 0 ~ 5로 변화하면 각 면을 그린다.
                    //그에 따른 순서도 맞추어져 있으므로 faceIndex로 p를 넘긴다.
                    AddTexture(world.blockTypes[blockID].GetTextureID(p));

                    //삼각형의 각 꼭짓점을 정점 4개에 맞게 정수로 넣는다.
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);

                    vertexIndex += 4;
                }
            }
        }
    }

    /// <summary>
    /// 청크의 메쉬를 만듭니다.(메쉬 데이터 반영)
    /// </summary>
    void ApplyChunkMesh()
	{
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
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

