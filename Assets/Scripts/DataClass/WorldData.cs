using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 월드에 대한 데이터가 저장되는 클래스(틀)
/// </summary>
[System.Serializable]
public class WorldData
{
    public string worldName = "Default";
    public int seed;

    /// <summary>
    /// Vector2Int 타입의 청크 좌표를 Key로, 
    /// ChunkData를 Value로 삼는 Dictionary이다.
    /// </summary>
    public Dictionary<Vector2Int, ChunkData> chunks = 
        new Dictionary<Vector2Int, ChunkData>();
    
    /// <summary>
    /// 청크의 좌표를 받아서 ChunkData를 반환한다.
    /// </summary>
    /// <returns></returns>
    public ChunkData GetChunkData(Vector2Int coord)
    {
        //해당 좌표에 대한 데이터가 있다면 Dictionary에서 찾아서 리턴
        //아니면 null 리턴
        return chunks.ContainsKey(coord) ? chunks[coord] : null;
    }
}
