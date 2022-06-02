using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

/// <summary>
/// World�� ������ ����ϴ� ���� Ŭ����
/// </summary>
public static class SaveManager
{
    /// <summary>
    /// worldData�� �̸����� �� ���͸��� �����Ѵ�.
    /// </summary>
    /// <param name="worldData">������ ������</param>
    public static void SaveWorld(WorldData worldData)
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

        int num = 0;
        //changedChunks���� ��� ûũ�� �����Ѵ�.
        foreach(ChunkData chunk in chunks)
        {
            //SaveChunk(chunk, worldData.worldName);
            ThreadPool.QueueUserWorkItem((obj) => SaveChunk(chunk, worldData.worldName));
            num++;
        }
        
        Debug.Log($"{num} chunks Saved!");
    }

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
            SaveWorld(world);

            return world;
        }

    }

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
        {
            Directory.CreateDirectory(savePath);
        }

        BinaryFormatter binaryFormatter = new BinaryFormatter();


        //BinaryFormatter�� ���� binary�� ����ȭ �� ��, 
        //FileStream�� ���� chunkSaveName �̸��� ���Ͽ� ����. (������ ����)
        using (BufferedStream stream =
            new BufferedStream(new FileStream(savePath + chunkSaveName, FileMode.Create)))
        {
            binaryFormatter.Serialize(stream, chunkData);
        }
    }


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
                //stream�� ������ ������ȭ �ؼ� Ŭ������ �ҷ��´�.
                chunk = binaryFormatter.Deserialize(stream) as ChunkData;

            }

            return chunk;
        }

        //������ ������ null�� �����Ѵ�.
        return null;

    }
}
