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

    public ChunkCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
	//�񱳹��� ���� Equals �������̵�
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
/// ûũ�� �⺻ Ʋ, �Ž� ������ ���� ������Ʈ�� ûũ ���� �� ������ ������.
/// </summary>
public class Chunk
{
    public ChunkCoord coord;

    //�޽� ���Ϳ� �޽� �������� ��� �����̴�.
    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    //byte ������ ������ ��, ���� �ڵ带 �����Ѵ�.
    byte[,,] voxelMap = 
        new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    //�� Ÿ�Ե��� ������ ���� World ����
    World world;

    //World�� ���ڷ� �޴´�.(find�� ���(expansive�� �۾�))
    public Chunk (ChunkCoord _coord, World _world)
	{
        this.world = _world;
        this.coord = _coord;
        chunkObject = new GameObject();

        //�ν����Ϳ��� �ϴ��� �ڵ�� �ű� ���̴�.
        //chunkObject�� �޽� ���Ϳ� �޽� �������� �߰��ϰ� ������ �����Ѵ�.
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        //���׸����� �����Ѵ�.
        meshRenderer.material = world.material;

        //���� ���� ���� �θ� �����ϵ��� ����.
        chunkObject.transform.SetParent(world.transform);
        //�����ġChunkCoord�� ������� ���� ��ġ �ݿ�
        chunkObject.transform.position
            = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = string.Format("Chunk {0}, {1}", coord.x, coord.z);

        PopulateVoxelMap();
        CreateMeshData();
        CreateChunkMesh();
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
    /// ���� ��ǥ�� ��� ������ ��ȯ
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
        //Air�� isSolid�� false �̹Ƿ� �׷����� ���� ��
        if (!IsVoxelInChunk(x, y, z))
            return world.blockTypes[world.GetVoxel(pos + position)].isSolid;

        //voxemMap�� ����� �� Ÿ���� ����� World�� BlockType[]�� isSolid�� ����
        return world.blockTypes[voxelMap[x, y, z]].isSolid;
	}

    /// <summary>
    /// �޽� �����͸� �����.
    /// </summary>
    void CreateMeshData()
	{
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
    }
    /// <summary>
    /// ûũ�� Ȱ��ȭ ���θ� �������ų� ������
    /// </summary>
    public bool IsActive
	{
        get
		{
            return chunkObject.activeSelf;
		}
        set
		{
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
        //���� �������� �Ž� �����Ϳ� �ִ´�.
        if (CheckVoxel(pos))
        {
            for (int p = 0; p < 6; p++)
            {
                //�� ���� �������� ��ĭ ���� �� ���� ���� ����
                //(=��, ���̴� ���϶��� �׸���.
                if (!CheckVoxel(pos + VoxelData.faceChecks[p]))
                {
                    //�ʿ��� ���ڷ� �Ѿ�� pos�� �� ���̵� ��ȸ�Ѵ�.
                    byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
                    //���������� ���� ���ؼ� for���� �ț���.
                    //���� 4���� �׿� �´� uv�� �ִ´�.
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                    //p�� ���� 0 ~ 5�� ��ȭ�ϸ� �� ���� �׸���.
                    //�׿� ���� ������ ���߾��� �����Ƿ� faceIndex�� p�� �ѱ��.
                    AddTexture(world.blockTypes[blockID].GetTextureID(p));

                    //�ﰢ���� �� �������� ���� 4���� �°� ������ �ִ´�.
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
    /// ûũ�� �޽��� ����ϴ�.(�޽� ������ �ݿ�)
    /// </summary>
    void CreateChunkMesh()
	{
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
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

