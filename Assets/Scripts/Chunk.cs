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

    //bool 값으로 구성된 맵, 블럭의 유무만 저장한다.
    bool[,,] voxelMap = 
        new bool[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];


    void Start()
    {
        PopulateVoxelMap();
        CreateMeshData();
        CreateChunkMesh();
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
                    if(y <= 3)
                        voxelMap[x, y, z] = true;
                }
            }
        }
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

        //맵 이탈 체크
        if (x < 0 || x >= VoxelData.ChunkWidth
            || y < 0 || y >= VoxelData.ChunkHeight
            || z < 0 || z >= VoxelData.ChunkWidth)
            return false;

        return voxelMap[x, y, z];
	}

    /// <summary>
    /// 메쉬 데이터를 만든다.
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
    /// 위치값을 받아 복셀 데이터를 메쉬 데이터에 추가합니다.
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
                    //각 면의 삼각형 그리기
                    for (int i = 0; i < 6; i++, vertexIndex++)
                    {
                        //삼각형의 인덱스, 정점을 삼각형 인덱스의 순서에 맞춰서 넣는다.
                        int triangleIndex = VoxelData.voxelTris[p, i];

                        //상대 위치인 정점 좌표에 매개변수의 좌표를 더한다.
                        vertices.Add(VoxelData.voxelVerts[triangleIndex] + pos);
                        triangles.Add(vertexIndex);
                        uvs.Add(VoxelData.voxelUvs[i]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 청크의 메쉬를 만듭니다.(메쉬 데이터 반영)
    /// </summary>
    void CreateChunkMesh()
	{
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        //큐브를 깔끔하게 그리기 위해 필요한 연산
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

}
