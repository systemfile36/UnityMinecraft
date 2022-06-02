using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

/// <summary>
/// World의 저장을 담당하는 정적 클래스
/// </summary>
public static class SaveManager
{
    /// <summary>
    /// worldData를 이름으로 된 디렉터리에 저장한다.
    /// </summary>
    /// <param name="worldData">저장할 데이터</param>
    public static void SaveWorld(WorldData worldData)
    {
        //저장 경로, 월드 이름으로된 폴더를 세이브 폴더에 생성
        string savePath = GamePaths.SavePath + worldData.worldName + "/";

        //디렉터리가 존재하지 않으면 만든다.
        if(!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        Debug.Log($"saved in {savePath}");


        BinaryFormatter binaryFormatter = new BinaryFormatter();


        //BinaryFormatter를 통해 binary로 직렬화 한 후, 
        //FileStream을 통해 world.dat라는 이름의 파일에 쓴다. (없으면 만듬)
        using (BufferedStream stream = 
            new BufferedStream(new FileStream(savePath + "world.dat", FileMode.Create)))
        {
            binaryFormatter.Serialize(stream, worldData);
        }

        //청크를 저장한다.

        //worldData의 changedChunks를 깊은 복사하여 가져온뒤 비운다.
        HashSet<ChunkData> chunks = worldData.CopyOfChangedChunks;

        int num = 0;
        //changedChunks내의 모든 청크를 저장한다.
        foreach(ChunkData chunk in chunks)
        {
            //SaveChunk(chunk, worldData.worldName);
            ThreadPool.QueueUserWorkItem((obj) => SaveChunk(chunk, worldData.worldName));
            num++;
        }
        
        Debug.Log($"{num} chunks Saved!");
    }

    /// <summary>
    /// worldName에 해당하는 World를 불러온다.
    /// </summary>
    public static WorldData LoadWorld(string worldName, int seed = 0)
    {
        //World 파일이 저장된 폴더를 이름으로 불러옴
        string worldPath = GamePaths.SavePath + worldName + "/";

        //파일 존재 여부 확인
        if (File.Exists(worldPath + "world.dat"))
        {
            Debug.Log($"Loading {worldName}/world.dat");

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            WorldData world;

            //World의 데이터 파일을 연다.
            using (BufferedStream stream =
            new BufferedStream(new FileStream(worldPath + "world.dat", FileMode.Open)))
            {
                //stream의 내용을 역직렬화 해서 클래스로 불러온다.
                world = binaryFormatter.Deserialize(stream) as WorldData;

            }

            return world;
        }
        else
        {
            //파일이 없으면, 새로 만들고 저장한 후 리턴한다.
            Debug.Log($"Create New World : {worldName}");

            WorldData world = new WorldData(worldName, seed);
            SaveWorld(world);

            return world;
        }

    }

    /// <summary>
    /// ChunkData를 worldName으로 된 디렉터리 아래에 저장한다.
    /// </summary>
    /// <param name="chunkData">저장할 데이터</param>
    public static void SaveChunk(ChunkData chunkData, string worldName)
    {
        //저장 경로, 월드 이름으로된 디렉터리 아래에 chunks 폴더로 지정
        string savePath = GamePaths.SavePath + worldName + "/chunks/";

        //저장될 청크 파일의 이름
        string chunkSaveName = 
            string.Format("{0}-{1}.dat", chunkData.position.x, chunkData.position.y);

        //디렉터리가 존재하지 않으면 만든다.
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        BinaryFormatter binaryFormatter = new BinaryFormatter();


        //BinaryFormatter를 통해 binary로 직렬화 한 후, 
        //FileStream을 통해 chunkSaveName 이름의 파일에 쓴다. (없으면 만듬)
        using (BufferedStream stream =
            new BufferedStream(new FileStream(savePath + chunkSaveName, FileMode.Create)))
        {
            binaryFormatter.Serialize(stream, chunkData);
        }
    }


    /// <summary>
    /// 해당 worldName의 position의 ChunkData를 로드한다.
    /// 존재하지 않으면 null을 리턴한다.
    /// </summary>
    public static ChunkData LoadChunk(string worldName, Vector2Int pos)
    {
        //좌표에 따른 청크 이름 지정
        string chunkName =
            string.Format("{0}-{1}.dat", pos.x, pos.y);

        //chunk 파일의 경로를 pos를 통해 특정
        string chunkPath = GamePaths.SavePath + worldName + "/chunks/" + chunkName;


        //파일 존재 여부 확인
        if (File.Exists(chunkPath))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            ChunkData chunk;

            //World의 데이터 파일을 연다.
            using (BufferedStream stream =
            new BufferedStream(new FileStream(chunkPath, FileMode.Open)))
            {
                //stream의 내용을 역직렬화 해서 클래스로 불러온다.
                chunk = binaryFormatter.Deserialize(stream) as ChunkData;

            }

            return chunk;
        }

        //파일이 없으면 null을 리턴한다.
        return null;

    }
}
