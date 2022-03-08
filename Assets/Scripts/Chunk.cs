using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    //bool ������ ������ ��, ���� ������ �����Ѵ�.
    bool[,,] voxelMap = 
        new bool[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];


    void Start()
    {
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
                    if(y <= 3)
                        voxelMap[x, y, z] = true;
                }
            }
        }
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

        //�� ��Ż üũ
        if (x < 0 || x >= VoxelData.ChunkWidth
            || y < 0 || y >= VoxelData.ChunkHeight
            || z < 0 || z >= VoxelData.ChunkWidth)
            return false;

        return voxelMap[x, y, z];
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
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }


    /// <summary>
    /// ��ġ���� �޾� ���� �����͸� �޽� �����Ϳ� �߰��մϴ�.
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
                    //�� ���� �ﰢ�� �׸���
                    for (int i = 0; i < 6; i++, vertexIndex++)
                    {
                        //�ﰢ���� �ε���, ������ �ﰢ�� �ε����� ������ ���缭 �ִ´�.
                        int triangleIndex = VoxelData.voxelTris[p, i];

                        //��� ��ġ�� ���� ��ǥ�� �Ű������� ��ǥ�� ���Ѵ�.
                        vertices.Add(VoxelData.voxelVerts[triangleIndex] + pos);
                        triangles.Add(vertexIndex);
                        uvs.Add(VoxelData.voxelUvs[i]);
                    }
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

}
