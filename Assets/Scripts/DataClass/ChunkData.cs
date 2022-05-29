using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각 청크에 대한 데이터가 저장되는 클래스(틀)
/// </summary>
[System.Serializable]
public class ChunkData
{
    //청크의 좌표를 나타낸다. 
    //Vector2Int는 직렬화 과정에서 복잡해지므로 
    //저장은 각각 int로 하고, 실제 접근은 Vector2Int로 접근한다.
    int x;
    int y;

    //각 좌표에 대한 접근 Vector2Int로 한다.
    public Vector2Int position
    {
        get { return new Vector2Int(x, y); }
        set { x = value.x; y = value.y; }
    }

    //VoxelState가 Serializable하고 이 변수가 public이므로
    //인스펙터에서 나타나는 조건을 만족하지만, 나타내기에는 너무 크므로
    //인스펙터에서 숨기라는 정보를 남겼다.
    [HideInInspector]
    public VoxelState[,,] map = 
        new VoxelState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];


    //생성자
    public ChunkData(Vector2Int pos) { position = pos; }

    public ChunkData(int x, int y) { this.x = x; this.y = y; }

    /// <summary>
    /// 맵을 세팅한다.
    /// </summary>
    void PopulateMap(object obj)
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
                    //map[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + position));
                }
            }
        }
    }
}
