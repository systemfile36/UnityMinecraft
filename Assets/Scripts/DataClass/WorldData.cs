using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���忡 ���� �����Ͱ� ����Ǵ� Ŭ����(Ʋ)
/// </summary>
[System.Serializable]
public class WorldData
{
    public string worldName = "Default";
    public int seed;

    /// <summary>
    /// Vector2Int Ÿ���� ûũ ��ǥ�� Key��, 
    /// ChunkData�� Value�� ��� Dictionary�̴�.
    /// </summary>
    public Dictionary<Vector2Int, ChunkData> chunks = 
        new Dictionary<Vector2Int, ChunkData>();
    
    /// <summary>
    /// ûũ�� ��ǥ�� �޾Ƽ� ChunkData�� ��ȯ�Ѵ�.
    /// </summary>
    /// <returns></returns>
    public ChunkData GetChunkData(Vector2Int coord)
    {
        //�ش� ��ǥ�� ���� �����Ͱ� �ִٸ� Dictionary���� ã�Ƽ� ����
        //�ƴϸ� null ����
        return chunks.ContainsKey(coord) ? chunks[coord] : null;
    }
}
