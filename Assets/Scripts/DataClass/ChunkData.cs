using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각 청크에 대한 데이터가 저장되는 클래스(틀)
/// </summary>
[System.Serializable]
public class ChunkData
{
    //청크의 상대 좌표를 나타낸다. 
    //Vector2Int는 직렬화 하기 어려우므로 정수 좌표로 한다.
    int x;
    int y;

    //각 좌표에 대한 접근은 Vector2Int로 한다.
    /// <summary>
    /// 청크의 상대 좌표
    /// </summary>
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
    public ChunkData(Vector2Int pos) { position = pos;}

    public ChunkData(int x, int y) { this.x = x; this.y = y; }

    //동일성 검사, 좌표가 같으면 같다.
    public override bool Equals(object obj)
    {
        if(!(obj is ChunkData)) 
            return false;

        ChunkData data = (ChunkData)obj; 

        return x == data.x && y == data.y;
    }

    //HashSet에서 사용하기 위한 해시코드 생성함수
    public override int GetHashCode()
    {
        //오버 플로우가 발생해도 그냥 무시한다는 뜻이다.
        unchecked
        {
            int hash = 7;

            hash = hash * 23 + x;
            hash = hash * 83 + y;

            return hash;
        }
    }

    /// <summary>
    /// 맵을 세팅한다.
    /// </summary>
    public void PopulateMap()
    {
        //청크의 상대 좌표를 실제 좌표로 변환한다.
        Vector2Int tempPos = new Vector2Int(x * 16, y * 16);

        //y 순방향 루프를 먼저 함으로서 아래부터 위로 설정되어 감
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    //GetVoxel로 해당 좌표의 블럭 아이디를 받아서 맵에 넣는다.
                    //현재 순회중인 블럭 좌표에
                    //현재 청크의 월드 기준 좌표를 더해서 실제 좌표를 넘긴다.
                    map[x, y, z] = new VoxelState(GameManager.Mgr.World.GetVoxel(new Vector3(x + tempPos.x, y, z + tempPos.y)));
                }
            }
        }

        //변경된 목록에 추가한다.
        GameManager.Mgr.World.worldData.AddToChanged(this);
    }
}
