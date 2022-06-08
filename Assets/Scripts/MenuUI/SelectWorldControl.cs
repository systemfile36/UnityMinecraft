using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using UnityEngine.UI;
using UnityEngine.EventSystems;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

public class SelectWorldControl : MonoBehaviour
{
    /// <summary>
    /// WorldNode 프리펩
    /// </summary>
    [Tooltip("WorldNode Prefab")]
    public GameObject worldNode;

    /// <summary>
    /// WorldList의 스크롤 뷰 참조
    /// </summary>
    [Tooltip("ScrollRect of WorldList")]
    public ScrollRect scrollRect;

    /// <summary>
    /// 현재 선택된 WorldNode
    /// </summary>
    public WorldNodeControl currentSelected = null;

    /// <summary>
    /// 로딩창 참조
    /// </summary>
    [Tooltip("LoadingControl of Current Scene")]
    public LoadingControl loadingControl;

    [Tooltip("MenuControl of Current Scene")]
    public MenuControl menuControl;

    //세이브 폴더에서 world.json들을 찾아서 리스트에 세팅함
    void Start()
    {
        LoadWorldList();

        
    }

    /// <summary>
    /// PlaySelected 버튼이 클릭 되었을 때의 이벤트
    /// </summary>
    public void OnClickPlaySelected()
    {
        if (currentSelected != null)
        {
            //현재 선택된 노드의 World 정보를 World에 넘김
            World.worldName = currentSelected.WorldName;
            World.seed = currentSelected.Seed;

            //World를 비동기로 로딩하고 로딩창을 띄운다.
            loadingControl.LoadingStart(GameManager.Mgr.LoadWorld());
        }
    }

    /// <summary>
    /// DeleteSelected 버튼이 클릭 되었을 때의 이벤트
    /// </summary>
    public void OnDeleteSelected()
    {
        if(currentSelected != null)
        {
            //현재 선택된 노드의 WorldName으로 저장된 디렉터리를 찾는다.
            string worldDirectory = GamePaths.SavePath + currentSelected.WorldName + "/";

            //저장된 디렉터리가 있다면
            if(Directory.Exists(worldDirectory))
            {
                //월드 디렉터리와 하위 파일을 모두 삭제한다.
                Directory.Delete(worldDirectory, true);
            }

            //현재 콘텐츠의 WorldNode를 모두 삭제한다.
            for(int i = 0; i < scrollRect.content.childCount; i++)
            {
                Destroy(scrollRect.content.GetChild(i).gameObject);
            }

            //월드 목록을 다시 로드한다.
            LoadWorldList();
        }
    }

    /// <summary>
    /// CreateNew 버튼이 클릭 되었을 때의 이벤트
    /// </summary>
    public void OnCreateNew()
    {
        //CreateNewWorld로 이동
        menuControl.MoveToMenu(transform.name, "CreateNewWorld");
    }

    /// <summary>
    /// Cancel 버튼이 클릭 되었을 때의 이벤트
    /// </summary>
    public void OnCancel()
    {
        //MainMenu로 이동
        menuControl.MoveToMenu(transform.name, "MainMenu");
    }

    /// <summary>
    /// 세이브 폴더의 월드들을 WorldList에 추가한다.
    /// </summary>
    void LoadWorldList()
    {
        string savePath = GamePaths.SavePath;

        //세이브 폴더의 디렉터리 목록을 불러온다.
        string[] worlds = Directory.GetDirectories(savePath);

        foreach (string worldsPath in worlds)
        {
            Debug.Log(worldsPath);

            //world.json 파일이 존재한다면
            if (File.Exists(worldsPath + "/world.json"))
            {

                using(var fStream = new FileStream(worldsPath + "/world.json", FileMode.Open))
                {
                    using (var bStream = new BufferedStream(fStream))
                    {
                        //파일에서 바이트로 읽어옴
                        byte[] bytes = new byte[bStream.Length];
                        bStream.Read(bytes, 0, bytes.Length);

                        //byte[]를 json 스트링으로 변경
                        string json = Encoding.UTF8.GetString(bytes);

                        //Json String을 JObject로 변경 (Linq 처럼 쓰기 위해)
                        JObject worldJson = JObject.Parse(json);

                        //WorldNode를 Scroll Rect의 Content의 자식으로 인스턴스화 하고
                        //WorldNodeControl 컴포넌트를 불러온다.
                        WorldNodeControl worldNodeControl = 
                            Instantiate(worldNode, scrollRect.content.transform)
                            .GetComponent<WorldNodeControl>();

                        //Json String에서 worldName과 시드를 읽어서 WorldNode에 텍스트로 세팅한다.
                        worldNodeControl.SetWorldText((string)worldJson["worldName"], (int)worldJson["seed"]);

                        //클릭 시의 동작 연결
                        worldNodeControl.SetOnClick(OnClickWorldNode);
                    }
                }
            }
        }
    }

    /// <summary>
    /// WorldNode가 눌렸을 때의 동작
    /// </summary>
    void OnClickWorldNode()
    {
        GameObject temp = EventSystem.current.currentSelectedGameObject;

        if (temp != null)
            currentSelected = temp.GetComponent<WorldNodeControl>();

        if (currentSelected != null)
            Debug.Log(currentSelected.WorldName);
    }


    

    
}
