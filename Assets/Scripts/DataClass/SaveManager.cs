using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// World�� ������ ����ϴ� ���� Ŭ����
/// </summary>
public static class SaveManager
{
    
    private static bool _IsSaving = false;

    /// <summary>
    /// ���� ������ ���� ������ ����
    /// </summary>
    public static bool IsSaving
    {
        get { return _IsSaving; }
    }

    /// <summary>
    /// �񵿱�� World�� �����ϰ� IAsyncResult�� return �Ѵ�.
    /// </summary>
    /// <param name="worldData">������ ������</param>
    /// <param name="IsSeq">���� ó�� ����</param>
    public static System.IAsyncResult SaveWorldAsync(WorldData worldData, bool IsSeq = true)
    {
        //IsSeq�� true�̸� ���������� ó�� �ǰ�, �ƴϸ� ���� ó�� �ȴ�.
        //���� ó���� �ϸ� ���� ��� ���� �����忡 ������ �� �� �ִ�.
        //�ݴ�� ���� ó���ϸ� �������� ��� ���� �����忡 ������ ����.

        //���� ���̸� ������ ĵ���Ѵ�.
        if (_IsSaving)
        {
            Debug.Log("SaveWorldAsync is Canceld");
            return null;
        }

        //���� ������ �˸���.
        _IsSaving = true;

        Debug.Log("SaveWorldAsync running...");

        System.Action<WorldData, bool> saveTask = SaveWorldJson;

        //������ �Ϸ�Ǹ� �ݹ����� ���� ������ false�� �����Ѵ�.
        //IAsyncResult�� return �Ѵ�.
        return saveTask.BeginInvoke(worldData, IsSeq, (ar) => { _IsSaving = false; }, null);

    }

    

    /// <summary>
    /// worldData�� �̸����� �� ���͸��� �����Ѵ�.
    /// ���� ó���� ���� ó���� ������ �� �ִ�.
    /// </summary>
    /// <param name="worldData">������ ������</param>
    /// <param name="IsSeq">���� ó�� ����</param>
    public static void SaveWorldJson(WorldData worldData, bool IsSeq = true)
    {
        //���� ���, ���� �̸����ε� ������ ���̺� ������ ����
        string savePath = GamePaths.SavePath + worldData.worldName + "/";

        //���͸��� �������� ������ �����.
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        //���� ������ ����
        using(var fStream = new FileStream(savePath + "world.json", FileMode.Create))
        {
            using(var bStream = new BufferedStream(fStream))
            {
                //WorldData -> Json
                string json = JsonConvert.SerializeObject(worldData);

                //Json -> byte[]
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                //byte[]�� BufferedStream�� ����.
                bStream.Write(bytes, 0, bytes.Length);

                //���۸� ����.
                bStream.Flush();
            }
        }

        //ûũ�� �����Ѵ�.

        //worldData�� changedChunks�� ���� �����Ͽ� �����µ� ����.
        HashSet<ChunkData> chunks = worldData.CopyOfChangedChunks;

        //����ó���� ����ó���� �����Ѵ�.
        if (IsSeq)
        {
            foreach (ChunkData chunk in chunks)
            {
                SaveChunkJson(chunk, worldData.worldName);
            }
        }
        else
        {
            //Parallel.ForEach�� ����ó���Ѵ�.
            System.Threading.Tasks.Parallel.ForEach(chunks,
                chunk => { SaveChunkJson(chunk, worldData.worldName); });
        }


        Debug.Log($"Save Json complete! : {chunks.Count}");

    }

    

    /// <summary>
    /// worldName�� �ش��ϴ� World�� �ҷ��´�.
    /// </summary>
    public static WorldData LoadWorldJson(string worldName, int seed = 0)
    {
        //World ������ ����� ������ �̸����� �ҷ���
        string worldPath = GamePaths.SavePath + worldName + "/";

        //���� ���� ���� Ȯ��
        if (File.Exists(worldPath + "world.json"))
        {
            Debug.Log($"Loading {worldName}/world.json");

            WorldData world;

            using(var fStream = new FileStream(worldPath + "world.json", FileMode.Open))
            {
                using(var bStream = new BufferedStream(fStream))
                {
                    byte[] bytes = new byte[bStream.Length];
                    bStream.Read(bytes, 0, bytes.Length);

                    string json = Encoding.UTF8.GetString(bytes);

                    world = JsonConvert.DeserializeObject<WorldData>(json);
                }
            }

            return world;
        }
        else
        {
            //������ ������, ���� ����� ������ �� �����Ѵ�.
            Debug.Log($"Create New World : {worldName}");

            WorldData world = new WorldData(worldName, seed);
            SaveWorldJson(world);

            return world;
        }
    }

    

    /// <summary>
    /// ChunkData�� worldName���� �� ���͸� �Ʒ��� �����Ѵ�.
    /// </summary>
    /// <param name="chunkData">������ ������</param>
    public static void SaveChunkJson(ChunkData chunkData, string worldName)
    {
        //���� ���, ���� �̸����ε� ���͸� �Ʒ��� chunks ������ ����
        string savePath = GamePaths.SavePath + worldName + "/chunks/";

        //����� ûũ ������ �̸�
        string chunkSaveName =
            string.Format("{0}-{1}.json", chunkData.position.x, chunkData.position.y);

        //���͸��� �������� ������ �����.
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        //Json���� ����
        using(var fStream = new FileStream(savePath + chunkSaveName, FileMode.Create))
        {
            using(var bStream = new BufferedStream(fStream))
            {
                //ChunkData -> Json string
                string json = JsonConvert.SerializeObject(chunkData);

                //Json string -> byte[]
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                //byte[] -> Json File
                bStream.Write(bytes, 0, bytes.Length);

                bStream.Flush();
            }
        }
    }

    

    /// <summary>
    /// �ش� worldName�� position�� ChunkData�� �ε��Ѵ�.
    /// �������� ������ null�� �����Ѵ�.
    /// </summary>
    public static ChunkData LoadChunkJson(string worldName, Vector2Int pos)
    {
        //��ǥ�� ���� ûũ �̸� ����
        string chunkName =
            string.Format("{0}-{1}.json", pos.x, pos.y);

        //chunk ������ ��θ� pos�� ���� Ư��
        string chunkPath = GamePaths.SavePath + worldName + "/chunks/" + chunkName;


        //���� ���� ���� Ȯ��
        if (File.Exists(chunkPath))
        {
            ChunkData chunk = null;

            using(var fStream = new FileStream(chunkPath, FileMode.Open))
            {
                using(var bStream = new BufferedStream(fStream))
                {
                    //Json File -> byte[]
                    byte[] bytes = new byte[bStream.Length];
                    bStream.Read(bytes, 0, bytes.Length);

                    //byte[] -> Json string
                    string json = Encoding.UTF8.GetString(bytes);

                    //Json string -> ChunkData
                    chunk = JsonConvert.DeserializeObject<ChunkData>(json);
                }
            }

            return chunk;
        }

        //������ ������ null�� �����Ѵ�.
        return null;

    }


    #region BinaryFormatter�� ����� ����/�ε� (������)
    /*
    /// <summary>
    /// �ش� worldName�� position�� ChunkData�� �ε��Ѵ�.
    /// �������� ������ null�� �����Ѵ�.
    /// </summary>
    public static ChunkData LoadChunk(string worldName, Vector2Int pos)
    {
        //��ǥ�� ���� ûũ �̸� ����
        string chunkName =
            string.Format("{0}-{1}.dat", pos.x, pos.y);

        //chunk ������ ��θ� pos�� ���� Ư��
        string chunkPath = GamePaths.SavePath + worldName + "/chunks/" + chunkName;


        //���� ���� ���� Ȯ��
        if (File.Exists(chunkPath))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            ChunkData chunk;

            //World�� ������ ������ ����.
            using (BufferedStream stream =
            new BufferedStream(new FileStream(chunkPath, FileMode.Open)))
            {
                stream.Position = 0;
                //stream�� ������ ������ȭ �ؼ� Ŭ������ �ҷ��´�.
                chunk = binaryFormatter.Deserialize(stream) as ChunkData;

            }

            return chunk;
        }

        //������ ������ null�� �����Ѵ�.
        return null;

    }
    */

    /*
    /// <summary>
    /// ChunkData�� worldName���� �� ���͸� �Ʒ��� �����Ѵ�.
    /// </summary>
    /// <param name="chunkData">������ ������</param>
    public static void SaveChunk(ChunkData chunkData, string worldName)
    {
        //���� ���, ���� �̸����ε� ���͸� �Ʒ��� chunks ������ ����
        string savePath = GamePaths.SavePath + worldName + "/chunks/";

        //����� ûũ ������ �̸�
        string chunkSaveName = 
            string.Format("{0}-{1}.dat", chunkData.position.x, chunkData.position.y);

        //���͸��� �������� ������ �����.
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        BinaryFormatter binaryFormatter = new BinaryFormatter();


        //BinaryFormatter�� ���� binary�� ����ȭ �� ��, 
        //FileStream�� ���� chunkSaveName �̸��� ���Ͽ� ����. (������ ����)
        using (BufferedStream stream =
            new BufferedStream(new FileStream(savePath + chunkSaveName, FileMode.Create)))
        {
            binaryFormatter.Serialize(stream, chunkData);
        }
    }

    */

    /*
    /// <summary>
    /// worldName�� �ش��ϴ� World�� �ҷ��´�.
    /// </summary>
    public static WorldData LoadWorld(string worldName, int seed = 0)
    {
        //World ������ ����� ������ �̸����� �ҷ���
        string worldPath = GamePaths.SavePath + worldName + "/";

        //���� ���� ���� Ȯ��
        if (File.Exists(worldPath + "world.dat"))
        {
            Debug.Log($"Loading {worldName}/world.dat");

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            WorldData world;

            //World�� ������ ������ ����.
            using (BufferedStream stream =
            new BufferedStream(new FileStream(worldPath + "world.dat", FileMode.Open)))
            {
                //stream�� ������ ������ȭ �ؼ� Ŭ������ �ҷ��´�.
                world = binaryFormatter.Deserialize(stream) as WorldData;

            }

            return world;
        }
        else
        {
            //������ ������, ���� ����� ������ �� �����Ѵ�.
            Debug.Log($"Create New World : {worldName}");

            WorldData world = new WorldData(worldName, seed);
            SaveWorldJson(world);

            return world;
        }

    }
    */

    /*
    /// <summary>
    /// worldData�� �̸����� �� ���͸��� �����Ѵ�.
    /// ���� ó���� ���� ó���� ������ �� �ִ�.
    /// </summary>
    /// <param name="worldData">������ ������</param>
    /// <param name="IsSeq">���� ó�� ����</param>
    public static void SaveWorld(WorldData worldData, bool IsSeq = true)
    {
        //���� ���, ���� �̸����ε� ������ ���̺� ������ ����
        string savePath = GamePaths.SavePath + worldData.worldName + "/";

        //���͸��� �������� ������ �����.
        if(!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        Debug.Log($"saved in {savePath}");


        BinaryFormatter binaryFormatter = new BinaryFormatter();


        //BinaryFormatter�� ���� binary�� ����ȭ �� ��, 
        //FileStream�� ���� world.dat��� �̸��� ���Ͽ� ����. (������ ����)
        using (BufferedStream stream = 
            new BufferedStream(new FileStream(savePath + "world.dat", FileMode.Create)))
        {
            binaryFormatter.Serialize(stream, worldData);
        }


        //ûũ�� �����Ѵ�.

        //worldData�� changedChunks�� ���� �����Ͽ� �����µ� ����.
        HashSet<ChunkData> chunks = worldData.CopyOfChangedChunks;

        //����ó���� ����ó���� �����Ѵ�.
        if(IsSeq)
        {
            foreach (ChunkData chunk in chunks)
            {
                SaveChunk(chunk, worldData.worldName);
            }
        }
        else
        {
            //Parallel.ForEach�� ����ó���Ѵ�.
            System.Threading.Tasks.Parallel.ForEach(chunks,
                chunk => { SaveChunk(chunk, worldData.worldName); });
        }


        Debug.Log($"Save complete! : {chunks.Count}");
        
    }
    */
    #endregion
}
