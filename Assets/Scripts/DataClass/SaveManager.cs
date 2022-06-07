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
/// World의 저장을 담당하는 정적 클래스
/// </summary>
public static class SaveManager
{
    
    private static bool _IsSaving = false;

    /// <summary>
    /// 현재 저장이 진행 중인지 여부
    /// </summary>
    public static bool IsSaving
    {
        get { return _IsSaving; }
    }

    /// <summary>
    /// 비동기로 World를 저장하고 IAsyncResult를 return 한다.
    /// </summary>
    /// <param name="worldData">저장할 데이터</param>
    /// <param name="IsSeq">순차 처리 여부</param>
    public static System.IAsyncResult SaveWorldAsync(WorldData worldData, bool IsSeq = true)
    {
        //IsSeq가 true이면 순차적으로 처리 되고, 아니면 병렬 처리 된다.
        //병렬 처리를 하면 빠른 대신 메인 스레드에 영향이 갈 수 있다.
        //반대로 순차 처리하면 느려지는 대신 메인 스레드에 영향이 적다.

        //저장 중이면 저장을 캔슬한다.
        if (_IsSaving)
        {
            Debug.Log("SaveWorldAsync is Canceld");
            return null;
        }

        //저장 중임을 알린다.
        _IsSaving = true;

        Debug.Log("SaveWorldAsync running...");

        System.Action<WorldData, bool> saveTask = SaveWorldJson;

        //저장이 완료되면 콜백으로 저장 중임을 false로 세팅한다.
        //IAsyncResult도 return 한다.
        return saveTask.BeginInvoke(worldData, IsSeq, (ar) => { _IsSaving = false; }, null);

    }

    

    /// <summary>
    /// worldData를 이름으로 된 디렉터리에 저장한다.
    /// 순차 처리와 병렬 처리를 선택할 수 있다.
    /// </summary>
    /// <param name="worldData">저장할 데이터</param>
    /// <param name="IsSeq">순차 처리 여부</param>
    public static void SaveWorldJson(WorldData worldData, bool IsSeq = true)
    {
        //저장 경로, 월드 이름으로된 폴더를 세이브 폴더에 생성
        string savePath = GamePaths.SavePath + worldData.worldName + "/";

        //디렉터리가 존재하지 않으면 만든다.
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        //월드 데이터 저장
        using(var fStream = new FileStream(savePath + "world.json", FileMode.Create))
        {
            using(var bStream = new BufferedStream(fStream))
            {
                //WorldData -> Json
                string json = JsonConvert.SerializeObject(worldData);

                //Json -> byte[]
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                //byte[]를 BufferedStream에 쓴다.
                bStream.Write(bytes, 0, bytes.Length);

                //버퍼를 비운다.
                bStream.Flush();
            }
        }

        //청크를 저장한다.

        //worldData의 changedChunks를 깊은 복사하여 가져온뒤 비운다.
        HashSet<ChunkData> chunks = worldData.CopyOfChangedChunks;

        //순차처리와 병렬처리를 선택한다.
        if (IsSeq)
        {
            foreach (ChunkData chunk in chunks)
            {
                SaveChunkJson(chunk, worldData.worldName);
            }
        }
        else
        {
            //Parallel.ForEach로 병렬처리한다.
            System.Threading.Tasks.Parallel.ForEach(chunks,
                chunk => { SaveChunkJson(chunk, worldData.worldName); });
        }


        Debug.Log($"Save Json complete! : {chunks.Count}");

    }

    

    /// <summary>
    /// worldName에 해당하는 World를 불러온다.
    /// </summary>
    public static WorldData LoadWorldJson(string worldName, int seed = 0)
    {
        //World 파일이 저장된 폴더를 이름으로 불러옴
        string worldPath = GamePaths.SavePath + worldName + "/";

        //파일 존재 여부 확인
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
            //파일이 없으면, 새로 만들고 저장한 후 리턴한다.
            Debug.Log($"Create New World : {worldName}");

            WorldData world = new WorldData(worldName, seed);
            SaveWorldJson(world);

            return world;
        }
    }

    

    /// <summary>
    /// ChunkData를 worldName으로 된 디렉터리 아래에 저장한다.
    /// </summary>
    /// <param name="chunkData">저장할 데이터</param>
    public static void SaveChunkJson(ChunkData chunkData, string worldName)
    {
        //저장 경로, 월드 이름으로된 디렉터리 아래에 chunks 폴더로 지정
        string savePath = GamePaths.SavePath + worldName + "/chunks/";

        //저장될 청크 파일의 이름
        string chunkSaveName =
            string.Format("{0}-{1}.json", chunkData.position.x, chunkData.position.y);

        //디렉터리가 존재하지 않으면 만든다.
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        //Json으로 저장
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
    /// 해당 worldName의 position의 ChunkData를 로드한다.
    /// 존재하지 않으면 null을 리턴한다.
    /// </summary>
    public static ChunkData LoadChunkJson(string worldName, Vector2Int pos)
    {
        //좌표에 따른 청크 이름 지정
        string chunkName =
            string.Format("{0}-{1}.json", pos.x, pos.y);

        //chunk 파일의 경로를 pos를 통해 특정
        string chunkPath = GamePaths.SavePath + worldName + "/chunks/" + chunkName;


        //파일 존재 여부 확인
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

        //파일이 없으면 null을 리턴한다.
        return null;

    }


    #region BinaryFormatter를 사용한 저장/로드 (삭제됨)
    /*
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
                stream.Position = 0;
                //stream의 내용을 역직렬화 해서 클래스로 불러온다.
                chunk = binaryFormatter.Deserialize(stream) as ChunkData;

            }

            return chunk;
        }

        //파일이 없으면 null을 리턴한다.
        return null;

    }
    */

    /*
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
            Directory.CreateDirectory(savePath);

        BinaryFormatter binaryFormatter = new BinaryFormatter();


        //BinaryFormatter를 통해 binary로 직렬화 한 후, 
        //FileStream을 통해 chunkSaveName 이름의 파일에 쓴다. (없으면 만듬)
        using (BufferedStream stream =
            new BufferedStream(new FileStream(savePath + chunkSaveName, FileMode.Create)))
        {
            binaryFormatter.Serialize(stream, chunkData);
        }
    }

    */

    /*
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
            SaveWorldJson(world);

            return world;
        }

    }
    */

    /*
    /// <summary>
    /// worldData를 이름으로 된 디렉터리에 저장한다.
    /// 순차 처리와 병렬 처리를 선택할 수 있다.
    /// </summary>
    /// <param name="worldData">저장할 데이터</param>
    /// <param name="IsSeq">순차 처리 여부</param>
    public static void SaveWorld(WorldData worldData, bool IsSeq = true)
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

        //순차처리와 병렬처리를 선택한다.
        if(IsSeq)
        {
            foreach (ChunkData chunk in chunks)
            {
                SaveChunk(chunk, worldData.worldName);
            }
        }
        else
        {
            //Parallel.ForEach로 병렬처리한다.
            System.Threading.Tasks.Parallel.ForEach(chunks,
                chunk => { SaveChunk(chunk, worldData.worldName); });
        }


        Debug.Log($"Save complete! : {chunks.Count}");
        
    }
    */
    #endregion
}
