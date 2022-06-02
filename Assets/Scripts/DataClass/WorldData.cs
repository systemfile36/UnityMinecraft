using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization; //OnDeserializedAttribute를 사용하기 위함
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
    [System.NonSerialized] //Vector2Int로 인한 오류를 막기 위해 직렬화에서 제외
    public Dictionary<Vector2Int, ChunkData> chunks = 
        new Dictionary<Vector2Int, ChunkData>();

    /// <summary>
    /// 변경된 청크들을 저장하는 HashSet
    /// </summary>
    [System.NonSerialized]
    private HashSet<ChunkData> changedChunks = new HashSet<ChunkData>();

    /// <summary>
    /// changedChunks의 DeepCopy를 반환하고, 기존의 것을 비운다.
    /// </summary>
    public HashSet<ChunkData> CopyOfChangedChunks
    {
        get
        {
            //changedChunks를 깊은 복사한 뒤, 기존의 것을 비운다.
            HashSet<ChunkData> temp = new HashSet<ChunkData>(changedChunks);
            changedChunks.Clear();
            return temp;
        }
    }

    public WorldData(string worldName, int seed)
    {
        this.worldName = worldName;
        this.seed = seed;
    }

    /// <summary>
    /// Deserialize가 완료되었을 때 호출되는 메소드
    /// Serializze가 되지 않은 자료를 초기화한다.
    /// </summary>
    /// <param name="context"></param>
    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        chunks = new Dictionary<Vector2Int, ChunkData>();
        changedChunks = new HashSet<ChunkData>();
    }

    /// <summary>
    /// 수정된 청크데이터 HashSet에 추가함
    /// </summary>
    /// <param name="chunkData"></param>
    public void AddToChanged(ChunkData chunkData)
    {

        changedChunks.Add(chunkData);
    }

    /// <summary>
    /// 청크의 상대 좌표를 받아 ChunkData를 반환한다.
    /// </summary>
    /// <param name="coord">청크의 상대 좌표</param>
    /// <param name="load">존재하지 않을 때 로드할지 여부</param>
    /// <returns></returns>
    public ChunkData GetChunkData(Vector2Int coord, bool load)
    {
        ChunkData temp;

        //존재한다면 그것을 리턴한다.
        if (chunks.ContainsKey(coord))
            temp = chunks[coord];
        //존재하지 않을 때, load가 false라면 null을 반환
        else if (!load)
            temp = null;
        //존재하지 않을 때, load가 true라면 로드한 후 로드한 것 반환
        else
        {
            LoadChunks(coord);
            temp = chunks[coord];
        }

        return temp;
    }

    /// <summary>
    /// 청크 데이터 딕셔너리에 지정된 상대 좌표의 청크를 추가한다.
    /// 디스크에 저장되었다면 그대로 로드하고 아니면 새로 만든다
    /// </summary>
    /// <param name="coord">청크의 상대 좌표</param>
    public void LoadChunks(Vector2Int coord)
    {
        //chunks안의 청크들의 접근 가능을 보장하기 위해
        //lock을 건다. 안그러면 중복되어 들어가거나 무결성이 파괴된다.
        lock (chunks)
        {
            //이미 존재한다면 생략한다.
            if (chunks.ContainsKey(coord))
            return;

            //디스크에 저장된 ChunkData가 있는지 찾는다.
            ChunkData chunkData = SaveManager.LoadChunk(worldName, coord);
            
            //저장된 ChunkData가 있다면 추가한다.
            if(chunkData != null)
            {
                chunks.Add(coord, chunkData);
            }
            //저장된 ChunkData가 없다면 새로 생성한다.
            else
            {
                //새로 ChunkData를 생성하고 맵을 구성한다.
                chunkData = new ChunkData(coord);
                chunkData.PopulateMap();

                //Dictionary에 ChunkData를 추가한다.
                chunks.Add(coord, chunkData);
            }
        }
    }

    /// <summary>
    /// 해당 월드 기준 좌표의 블럭 아이디를 v로 세팅한다.
    /// 세팅에 실패하면 false를 반환한다.
    /// </summary>
    /// <param name="pos">수정할 블럭의 월드 기준 좌표</param>
    /// <param name="id">설정할 블럭 id</param>
    public bool SetVoxel(Vector3 pos, byte id)
    {

        //수정할 블럭의 월드 기준 좌표와 id를 받아서
        //해당 ChunkData를 찾아서 map을 수정한다.

        //해당 좌표의 유효성 검사
        if (!IsVoxelInWorld(pos))
            return false;

        //월드 기준 좌표로 해당하는 청크의 coord를 찾는다.
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        //ChunkData를 받아온다. 없으면 로드한 후 받아온다.
        ChunkData temp = GetChunkData(new Vector2Int(x, z), true);

        //청크 넓이 만큼 곱해서 청크의 월드 기준 좌표를 얻는다.
        //청크의 transform.position의 x, z값과 같은 값을 가질 것
        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        //pos를 월드 기준에서 청크 기준 상대좌표로 변환한 값이다.
        Vector3Int vPos = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        //수정할 위치가 null이면 false 리턴
        if (temp.map[vPos.x, vPos.y, vPos.z] == null)
            return false;

        //해당 하는 청크의 블럭 아이디를 변경한다.
        temp.map[vPos.x, vPos.y, vPos.z].id = id;
        
        //변경된 목록에 ChunkData 추가
        changedChunks.Add(temp);

        return true;

    }

    /// <summary>
    /// 해당 월드 기준 좌표의 VoxelState를 반환한다.
    /// </summary>
    /// <param name="pos">블럭의 월드 기준 좌표</param>
    /// <returns></returns>
    public VoxelState GetVoxelState(Vector3 pos)
    {

        //해당 좌표의 유효성 검사
        if (!IsVoxelInWorld(pos))
            return null;

        //월드 기준 좌표로 해당하는 청크의 좌표를 찾는다.
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        //ChunkData를 받아온다. 없으면 로드한 후 받아온다.
        ChunkData temp = GetChunkData(new Vector2Int(x, z), true);

        //청크 넓이 만큼 곱해서 청크의 월드 기준 좌표를 얻는다.
        //청크의 transform.position의 x, z값과 같은 값을 가질 것
        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        
        //pos를 월드 기준에서 청크 내부 기준 상대좌표로 변환한 값이다.
        Vector3Int vPos = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        //해당 청크의 맵에서 해당 좌표의 VoxelState를 반환한다.
        return temp.map[vPos.x, vPos.y, vPos.z];
    }

    /// <summary>
    /// 복셀이 월드 내부에 있는지 여부 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels &&
            pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
            pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;
    }
}
