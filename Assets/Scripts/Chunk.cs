using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���忡���� ûũ�� ��ǥ
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
    /// ��ǥ�� �޾� �� ��ǥ�� ���� ûũ�� ��ǥ�� �ʱ�ȭ
    /// </summary>
    /// <param name="pos"></param>
    public ChunkCoord(Vector3 pos)
	{
        int xP = Mathf.FloorToInt(pos.x);
        int zP = Mathf.FloorToInt(pos.z);

        x = xP / VoxelData.ChunkWidth;
        z = zP / VoxelData.ChunkWidth;
	}
	//�񱳹��� ���� Equals �������̵�
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
/// ûũ�� �⺻ Ʋ, �Ž� ������ ���� ������Ʈ�� ûũ ���� �� ������ ������.
/// </summary>
public class Chunk
{
    public ChunkCoord coord;

    //�޽� ���Ϳ� �޽� �������� ��� �����̴�.
    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    //MeshCollider meshCollider;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    //������� �ﰢ�� ��ǥ �����ϴ� ����Ʈ
    List<int> TransparentTriangles = new List<int>();

    //���׸��� ���� ������ ���� ���׸��� �迭
    Material[] materials = new Material[2];

    //byte ������ ������ ��, ���� �ڵ带 �����Ѵ�.
    public byte[,,] voxelMap = 
        new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    //�� Ÿ�Ե��� ������ ���� World ����
    World world;

    private bool _IsActive;

    //voxelMap�� �ʱ�ȭ �Ǿ����� ����
    public bool IsMapInit = false;

    //World�� ���ڷ� �޴´�.(find�� ���(expansive�� �۾�))
    /// <summary>
    /// ûũ�� ������, ��� �ʱ�ȭ���� ���θ� ��������
    /// </summary>
    /// <param name="_coord">������ ûũ�� ��ǥ</param>
    /// <param name="_world">World�� ���� ����</param>
    /// <param name="genOnLoad">�������ڸ��� Init�� ȣ�� ���� ����</param>
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
    /// ûũ�� �ʱ�ȭ. ���� �����ϰ� �޽��� ���� �߰��ϴ� ����
    /// </summary>
    public void Init()
	{
        chunkObject = new GameObject();
        //IsActive = true;
        //�ν����Ϳ��� �ϴ��� �ڵ�� �ű� ���̴�.
        //chunkObject�� �޽� ���Ϳ� �޽� �������� �߰��ϰ� ������ �����Ѵ�.
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        //���׸����� �����Ѵ�.
        //meshRenderer.material = world.material;

        //���� �� ������ ���� ���׸��� ������ ����
        materials[0] = world.material;
        materials[1] = world.TransparentMaterial;

        meshRenderer.materials = materials;

        //meshCollider = chunkObject.AddComponent<MeshCollider>();

        //���� ���� ���� �θ� �����ϵ��� ����.
        chunkObject.transform.SetParent(world.transform);
        //�����ġChunkCoord�� ������� ���� ��ġ �ݿ�
        chunkObject.transform.position
            = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = string.Format("Chunk {0}, {1}", coord.x, coord.z);

        PopulateVoxelMap();
        RefreshChunkMeshData();

        //meshCollider.sharedMesh = meshFilter.mesh;
    }

    /// <summary>
    /// �������� �����Ѵ�.
    /// </summary>
    void PopulateVoxelMap()
	{
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    //GetVoxel�� �ش� ��ǥ�� �� ���̵� �޾Ƽ� �ʿ� �ִ´�.
                    //�̶� ���� ���� ��ǥ�� �־�� �Կ� �����϶�
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }
        IsMapInit = true;
    }
    /// <summary>
    /// ������ ûũ ���ο� �ִ��� ���� ��ȯ
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
    /// ������ ��ǥ�� ���� id�� �ٲ۴�.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="id"></param>
    public void EditVoxel(Vector3 pos, byte id)
	{
        //��ǥ�� ������
        int xP = Mathf.FloorToInt(pos.x);
        int yP = Mathf.FloorToInt(pos.y);
        int zP = Mathf.FloorToInt(pos.z);

        //���� ���� ��ǥ�� �� ûũ�� �ʿ� ���Ǵ� ��ǥ�� ��ȯ
        //ûũ�� ���� ��ǥ�� ���� ������ ������
        xP -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zP -= Mathf.FloorToInt(chunkObject.transform.position.z);


        //�ʿ� ����� id�� ����
        voxelMap[xP, yP, zP] = id;

        RefreshChunkMeshData();

        //������ ûũ ���ǿ� ���� ����
        RefreshAdjacentChunk(xP, yP, zP);
    }
    /// <summary>
    /// ������ ���� ��ǥ�� ���ڷ� �޾� �� ���� ������ ûũ ����
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    private void RefreshAdjacentChunk(int x, int y, int z)
	{
        Vector3 tempV = new Vector3(x, y, z);

        //������ ���� �� ���� ���⿡ ���Ͽ� �ݺ�
        for(int p = 0; p < 6; p++)
		{
            //�� ���� �������� ��ĭ �̵��� ��ǥ
            Vector3 temp = tempV + VoxelData.faceChecks[p];

            //������ ������ ��ĭ �� �̵��ؼ� �� ���� ûũ ���ο� �ִ��� üũ�Ѵ�.
            if (!IsVoxelInChunk((int)temp.x, (int)temp.y, (int)temp.z))
			{
                //���� ���ο� ���ٸ�, �ٸ� ûũ�� �ְ�, ������ ���� �����ִٴ� ��
                //���� �� ûũ�� �����Ѵ�.
                world.GetChunkFromVector3(temp + position).RefreshChunkMeshData();
			}
		}
	}

    /// <summary>
    /// ���� ��ǥ�� ��� isSolid�� ��ȯ
    /// </summary>
    /// <param name="pos">��ǥ</param>
    /// <returns></returns>
    bool CheckVoxel(Vector3 pos)
	{
        //��ǥ�� ������
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //���� ������ ûũ ���ο� ���� ������ 
        //ûũ�� ��ǥ�� pos�� ���ؼ� �� Ÿ���� isSolid ����
        //��, �ٸ� ûũ�� �� ���θ� Ȯ���ϱ� �����̴�.
        //�ٱ��� ���θ� �Ǵ��� �� �ʿ���
        //Air�� isSolid�� false �̹Ƿ� �׷����� ���� ��
        if (!IsVoxelInChunk(x, y, z))
            //���� ��ǥ�� ��ȯ�Կ� ����
            return world.CheckVoxelSolid(pos + position);

        //voxemMap�� ����� �� Ÿ���� ����� World�� BlockType[]�� isSolid�� ����
        return world.blockTypes[voxelMap[x, y, z]].isSolid;
	}
    /// <summary>
    /// ���� ��ǥ�� ��� isTransparent�� ��ȯ
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool CheckVoxelTransparent(Vector3 pos)
    {
        //��ǥ�� ������
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //���� ������ ûũ ���ο� ���� ������ 
        //ûũ�� ��ǥ�� pos�� ���ؼ� �� Ÿ���� isTransparent ����
        //��, �ٸ� ûũ�� �� ���θ� Ȯ���ϱ� �����̴�.
        //�ٱ��� ���θ� �Ǵ��� �� �ʿ���
        //Air�� isSolid�� false �̹Ƿ� �׷����� ���� ��
        if (!IsVoxelInChunk(x, y, z))
            //���� ��ǥ�� ��ȯ�Կ� ����
            return world.CheckVoxelTransparent(pos + position);

        //voxemMap�� ����� �� Ÿ���� ����� World�� BlockType[]�� isTransprent�� ����
        return world.blockTypes[voxelMap[x, y, z]].isTransparent;
    }

    /// <summary>
    /// ���� ���� ��ǥ�� �޾Ƽ� �ڽ��� �� ����, �� ��ġ�� �� Ÿ�� ��ȯ
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public byte GetVoxelFromGlobalVector3(Vector3 pos)
	{
        //��ǥ�� ������
        int xP = Mathf.FloorToInt(pos.x);
        int yP = Mathf.FloorToInt(pos.y);
        int zP = Mathf.FloorToInt(pos.z);

        //���� ���� ��ǥ�� �� ûũ�� �ʿ� ���Ǵ� ��ǥ�� ��ȯ
        //ûũ�� ���� ��ǥ�� ���� ������ ������
        xP -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zP -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xP, yP, zP];

    }

    /// <summary>
    /// �޽� �����͸� �����Ѵ�.
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
                    //���� Solid �϶��� �׸�
                    if(world.blockTypes[voxelMap[x, y, z]].isSolid)
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
        ApplyChunkMesh();
    }

    /// <summary>
    /// �޽� �����͸� �ʱ�ȭ �Ѵ�. ������ ����
    /// </summary>
    void ClearMeshData()
	{
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
	}

    /// <summary>
    /// ûũ�� Ȱ��ȭ ���θ� �������ų� ������
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
    /// ûũ�� ��ǥ ��ȯ
    /// </summary>
    public Vector3 position
	{
        get
		{
            return chunkObject.transform.position;
		}
	}

    /// <summary>
    /// ��ġ���� �޾� ���� �����͸� �޽� ������ ����Ʈ�� �߰��մϴ�.
    /// </summary>
    /// <param name="pos">���� �������� ��ġ</param>
    void AddVoxelDataToChunk(Vector3 pos)
	{
        //�ʿ��� ���ڷ� �Ѿ�� pos�� �� ���̵� ��ȸ�Ѵ�.
        byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

        //pos��ǥ�� �����θ� Ȯ��
        bool isTransparent = world.blockTypes[blockID].isTransparent;

        for (int p = 0; p < 6; p++)
        {
            //�� ���� �������� ��ĭ ���� �� ���� ���� ����
            //(=��, ���̴� ���϶��� �׸���.) 
            //if (!CheckVoxel(pos + VoxelData.faceChecks[p]))

            //---------������--------------
            //�� ���� �������� ��ĭ �� ���� �ִ� ���� ������϶��� �׸���.
            //Air�� �������̹Ƿ� �׷����� �ʴ´�.
            if(CheckVoxelTransparent(pos + VoxelData.faceChecks[p]))
            {
                
                //���������� ���� ���ؼ� for���� �ț���.
                //���� 4���� �׿� �´� uv�� �ִ´�.
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                //p�� ���� 0 ~ 5�� ��ȭ�ϸ� �� ���� �׸���.
                //�׿� ���� ������ ���߾��� �����Ƿ� faceIndex�� p�� �ѱ��.
                AddTexture(world.blockTypes[blockID].GetTextureID(p));

                //���� �������� �ʴٸ�, �⺻ �ﰢ�� ����Ʈ�� �ִ´�.
                if (!isTransparent)
                {
                    //�ﰢ���� �� �������� ���� 4���� �°� ������ �ִ´�.
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }
                //������ �ƴϸ� ���� �ﰢ�� �迭�� �ִ´�.
                else
				{
                    //�ﰢ���� �� �������� ���� 4���� �°� ������ �ִ´�.
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
    /// ûũ�� �޽��� ����ϴ�.(�޽� ������ �ݿ�)
    /// </summary>
    void ApplyChunkMesh()
	{
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        //mesh.triangles = triangles.ToArray();

        //�� �޽��� ����� ���׸����� ���� ����
        mesh.subMeshCount = 2;
        
        //�⺻ ���׸����� ����� �ﰢ����
        mesh.SetTriangles(triangles.ToArray(), 0);

        //���� ���׸����� ����� �ﰢ����
        mesh.SetTriangles(TransparentTriangles.ToArray(), 1);

        mesh.uv = uvs.ToArray();

        //ť�긦 ����ϰ� �׸��� ���� �ʿ��� ����
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    /// <summary>
    /// �ؽ��� ID�� �޾� uv���� �߰��մϴ�.
    /// </summary>
    /// <param name="textureID"></param>
    void AddTexture(int textureID)
	{
        //�ؽ���ID�� ���� x, y��ǥ
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);
     
        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;
        
        //��ǥ�� UV�� ��ġ�� �°� �������ִ� ����
        y = 1f - y - VoxelData.NormalizedBlockTextureSize;
        
        //�̸� ������ ������ ����(uv ���̺��� �ﰢ���� ������ �����Ƿ�)
        uvs.Add(new Vector2(x, y)); //0, 0
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize)); //0, 1
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y)); //1, 0
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize)); //1, 1
    }

}

