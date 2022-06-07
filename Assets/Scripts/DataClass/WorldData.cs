using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization; //OnDeserializedAttribute�� ����ϱ� ����
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Concurrent;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Linq;

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
    [System.NonSerialized] //Vector2Int�� ���� ������ ���� ���� ����ȭ���� ����
    public Dictionary<Vector2Int, ChunkData> chunks = 
        new Dictionary<Vector2Int, ChunkData>();

    /// <summary>
    /// ����� ûũ���� �����ϴ� HashSet
    /// </summary>
    [System.NonSerialized]
    private HashSet<ChunkData> changedChunks = new HashSet<ChunkData>();

    /// <summary>
    /// changedChunks�� DeepCopy�� ��ȯ�ϰ�, ������ ���� ����.
    /// </summary>
    [JsonIgnore] //Json ����ȭ���� �����Ѵٴ� ��(Newtonsoft.Json�� ������Ƽ�� ����ȭ �Ѵ�.)
    public HashSet<ChunkData> CopyOfChangedChunks
    {
        get
        {
            lock(changedChunks)
            {
                //changedChunks�� ���� ������ ��, ������ ���� ����.
                HashSet<ChunkData> temp = new HashSet<ChunkData>(changedChunks);
                changedChunks.Clear();
                return temp;
            }
        }
    }

    public WorldData(string worldName, int seed)
    {
        this.worldName = worldName;
        this.seed = seed;
    }

    /// <summary>
    /// Deserialize�� �Ϸ�Ǿ��� �� ȣ��Ǵ� �޼ҵ�
    /// Serializze�� ���� ���� �ڷḦ �ʱ�ȭ�Ѵ�.
    /// </summary>
    /// <param name="context"></param>
    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        chunks = new Dictionary<Vector2Int, ChunkData>();
        changedChunks = new HashSet<ChunkData>();
    }

    /// <summary>
    /// ������ ûũ������ HashSet�� �߰���
    /// </summary>
    /// <param name="chunkData"></param>
    public void AddToChanged(ChunkData chunkData)
    {
        lock (changedChunks)
        {
            changedChunks.Add(chunkData);
        }
    }

    /// <summary>
    /// ûũ�� ��� ��ǥ�� �޾� ChunkData�� ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="coord">ûũ�� ��� ��ǥ</param>
    /// <param name="load">�������� ���� �� �ε����� ����</param>
    /// <returns></returns>
    public ChunkData GetChunkData(Vector2Int coord, bool load)
    {
        ChunkData temp;

        //�����Ѵٸ� �װ��� �����Ѵ�.
        if (chunks.ContainsKey(coord))
            temp = chunks[coord];
        //�������� ���� ��, load�� false��� null�� ��ȯ
        else if (!load)
            temp = null;
        //�������� ���� ��, load�� true��� �ε��� �� �ε��� �� ��ȯ
        else
        {
            LoadChunks(coord);
            temp = chunks[coord];
        }

        return temp;
    }

    /// <summary>
    /// ûũ ������ ��ųʸ��� ������ ��� ��ǥ�� ûũ�� �߰��Ѵ�.
    /// ��ũ�� ����Ǿ��ٸ� �״�� �ε��ϰ� �ƴϸ� ���� �����
    /// </summary>
    /// <param name="coord">ûũ�� ��� ��ǥ</param>
    public void LoadChunks(Vector2Int coord)
    {
        //Debug.Log($"LoadChunks Called : {coord}");

        //chunks���� ûũ���� ���� ������ �����ϱ� ����
        //lock�� �Ǵ�. �ȱ׷��� �ߺ��Ǿ� ���ų� ���Ἲ�� �ı��ȴ�.
        lock (chunks)
        {
            //�̹� �����Ѵٸ� �����Ѵ�.
            if (chunks.ContainsKey(coord))
            return;

            //��ũ�� ����� ChunkData�� �ִ��� ã�´�.
            ChunkData chunkData = SaveManager.LoadChunkJson(worldName, coord);
            
            //����� ChunkData�� �ִٸ� �߰��Ѵ�.
            if(chunkData != null)
            {
                chunks.Add(coord, chunkData);
            }
            //����� ChunkData�� ���ٸ� ���� �����Ѵ�.
            else
            {
                //���� ChunkData�� �����ϰ� ���� �����Ѵ�.
                chunkData = new ChunkData(coord);
                chunkData.PopulateMap();

                //Dictionary�� ChunkData�� �߰��Ѵ�.
                chunks.Add(coord, chunkData);
            }
        }
    }

    /// <summary>
    /// ���� ��ǥ�� �������� LoadDistanceInChunks ��ŭ�� ûũ�� ��ųʸ��� �����Ѵ�.
    /// �ʱ� ���� ���� �ÿ��� ���ǹǷ� �ߺ�Ȯ���� ���� �ʴ´�.
    /// </summary>
    /// <param name="spawnPos">������� ���� ��ǥ</param>
    public void LoadAllChunks(Vector3Int spawnPos)
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        int xMin = Mathf.Clamp((spawnPos.x / VoxelData.ChunkWidth) - GameManager.Mgr.settings.LoadDistanceInChunks
            , 0, VoxelData.WorldSizeInChunks);
        int zMin = Mathf.Clamp((spawnPos.z / VoxelData.ChunkWidth) - GameManager.Mgr.settings.LoadDistanceInChunks
            , 0, VoxelData.WorldSizeInChunks);

        int xMax = Mathf.Clamp((spawnPos.x / VoxelData.ChunkWidth) + GameManager.Mgr.settings.LoadDistanceInChunks
            , 0, VoxelData.WorldSizeInChunks);
        int zMax = Mathf.Clamp((spawnPos.z / VoxelData.ChunkWidth) + GameManager.Mgr.settings.LoadDistanceInChunks
            , 0, VoxelData.WorldSizeInChunks);

        //�ε��� ������ ��ǥ�� ����
        //���� �̸� ����
        List<Vector2Int> chunkPositions = 
            new List<Vector2Int>((GameManager.Mgr.settings.LoadDistanceInChunks * 2) * (GameManager.Mgr.settings.LoadDistanceInChunks * 2));

        for (int x = xMin; x < xMax; x++)
        {
            for (int z = zMin; z < zMax; z++)
            {
                chunkPositions.Add(new Vector2Int(x, z));
            }
        }

        Parallel.ForEach(chunkPositions, chunkPos =>
        {
            ChunkData chunkData = SaveManager.LoadChunkJson(worldName, chunkPos);

            //����� ûũ�� �ε��ߴٸ� ��ųʸ��� �߰��Ѵ�.
            if (chunkData != null)
                lock (chunks) { chunks.Add(chunkData.position, chunkData); }
            //�ƴϸ� ���� ���� ���� �����ϰ� ��ųʸ��� �߰��Ѵ�.
            else
            {
                chunkData = new ChunkData(chunkPos);
                chunkData.PopulateMap();
                lock(chunks) { chunks.Add(chunkData.position, chunkData); }
            }
        });

        stopwatch.Stop();
        Debug.Log($"LoadAllChunkJson Elapsed : {stopwatch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// �ش� ���� ���� ��ǥ�� �� ���̵� v�� �����Ѵ�.
    /// ���ÿ� �����ϸ� false�� ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="pos">������ ���� ���� ���� ��ǥ</param>
    /// <param name="id">������ �� id</param>
    public bool SetVoxel(Vector3 pos, byte id)
    {

        //������ ���� ���� ���� ��ǥ�� id�� �޾Ƽ�
        //�ش� ChunkData�� ã�Ƽ� map�� �����Ѵ�.

        //�ش� ��ǥ�� ��ȿ�� �˻�
        if (!IsVoxelInWorld(pos))
            return false;

        //���� ���� ��ǥ�� �ش��ϴ� ûũ�� coord�� ã�´�.
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        //ChunkData�� �޾ƿ´�. ������ �ε��� �� �޾ƿ´�.
        ChunkData temp = GetChunkData(new Vector2Int(x, z), true);

        //ûũ ���� ��ŭ ���ؼ� ûũ�� ���� ���� ��ǥ�� ��´�.
        //ûũ�� transform.position�� x, z���� ���� ���� ���� ��
        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        //pos�� ���� ���ؿ��� ûũ ���� �����ǥ�� ��ȯ�� ���̴�.
        Vector3Int vPos = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        //������ ��ġ�� null�̸� false ����
        if (temp.map[vPos.x, vPos.y, vPos.z] == null)
            return false;

        //�ش� �ϴ� ûũ�� �� ���̵� �����Ѵ�.
        temp.map[vPos.x, vPos.y, vPos.z].id = id;
        
        //����� ��Ͽ� ChunkData �߰�
        changedChunks.Add(temp);

        return true;

    }

    /// <summary>
    /// �ش� ���� ���� ��ǥ�� VoxelState�� ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="pos">���� ���� ���� ��ǥ</param>
    /// <returns></returns>
    public VoxelState GetVoxelState(Vector3 pos)
    {

        //�ش� ��ǥ�� ��ȿ�� �˻�
        if (!IsVoxelInWorld(pos))
            return null;

        //���� ���� ��ǥ�� �ش��ϴ� ûũ�� ��ǥ�� ã�´�.
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        //ChunkData�� �޾ƿ´�. ������ �ε��� �� �޾ƿ´�.
        ChunkData temp = GetChunkData(new Vector2Int(x, z), true);

        //ûũ ���� ��ŭ ���ؼ� ûũ�� ���� ���� ��ǥ�� ��´�.
        //ûũ�� transform.position�� x, z���� ���� ���� ���� ��
        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkWidth;

        
        //pos�� ���� ���ؿ��� ûũ ���� ���� �����ǥ�� ��ȯ�� ���̴�.
        Vector3Int vPos = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        //�ش� ûũ�� �ʿ��� �ش� ��ǥ�� VoxelState�� ��ȯ�Ѵ�.
        return temp.map[vPos.x, vPos.y, vPos.z];
    }

    /// <summary>
    /// ������ ���� ���ο� �ִ��� ���� ��ȯ
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
