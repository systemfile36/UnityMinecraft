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
    /// WorldNode ������
    /// </summary>
    [Tooltip("WorldNode Prefab")]
    public GameObject worldNode;

    /// <summary>
    /// WorldList�� ��ũ�� �� ����
    /// </summary>
    [Tooltip("ScrollRect of WorldList")]
    public ScrollRect scrollRect;

    /// <summary>
    /// ���� ���õ� WorldNode
    /// </summary>
    public WorldNodeControl currentSelected = null;

    /// <summary>
    /// �ε�â ����
    /// </summary>
    [Tooltip("LoadingControl of Current Scene")]
    public LoadingControl loadingControl;

    [Tooltip("MenuControl of Current Scene")]
    public MenuControl menuControl;

    //���̺� �������� world.json���� ã�Ƽ� ����Ʈ�� ������
    void Start()
    {
        LoadWorldList();

        
    }

    /// <summary>
    /// PlaySelected ��ư�� Ŭ�� �Ǿ��� ���� �̺�Ʈ
    /// </summary>
    public void OnClickPlaySelected()
    {
        if (currentSelected != null)
        {
            //���� ���õ� ����� World ������ World�� �ѱ�
            World.worldName = currentSelected.WorldName;
            World.seed = currentSelected.Seed;

            //World�� �񵿱�� �ε��ϰ� �ε�â�� ����.
            loadingControl.LoadingStart(GameManager.Mgr.LoadWorld());
        }
    }

    /// <summary>
    /// DeleteSelected ��ư�� Ŭ�� �Ǿ��� ���� �̺�Ʈ
    /// </summary>
    public void OnDeleteSelected()
    {
        if(currentSelected != null)
        {
            //���� ���õ� ����� WorldName���� ����� ���͸��� ã�´�.
            string worldDirectory = GamePaths.SavePath + currentSelected.WorldName + "/";

            //����� ���͸��� �ִٸ�
            if(Directory.Exists(worldDirectory))
            {
                //���� ���͸��� ���� ������ ��� �����Ѵ�.
                Directory.Delete(worldDirectory, true);
            }

            //���� �������� WorldNode�� ��� �����Ѵ�.
            for(int i = 0; i < scrollRect.content.childCount; i++)
            {
                Destroy(scrollRect.content.GetChild(i).gameObject);
            }

            //���� ����� �ٽ� �ε��Ѵ�.
            LoadWorldList();
        }
    }

    /// <summary>
    /// CreateNew ��ư�� Ŭ�� �Ǿ��� ���� �̺�Ʈ
    /// </summary>
    public void OnCreateNew()
    {
        //CreateNewWorld�� �̵�
        menuControl.MoveToMenu(transform.name, "CreateNewWorld");
    }

    /// <summary>
    /// Cancel ��ư�� Ŭ�� �Ǿ��� ���� �̺�Ʈ
    /// </summary>
    public void OnCancel()
    {
        //MainMenu�� �̵�
        menuControl.MoveToMenu(transform.name, "MainMenu");
    }

    /// <summary>
    /// ���̺� ������ ������� WorldList�� �߰��Ѵ�.
    /// </summary>
    void LoadWorldList()
    {
        string savePath = GamePaths.SavePath;

        //���̺� ������ ���͸� ����� �ҷ��´�.
        string[] worlds = Directory.GetDirectories(savePath);

        foreach (string worldsPath in worlds)
        {
            Debug.Log(worldsPath);

            //world.json ������ �����Ѵٸ�
            if (File.Exists(worldsPath + "/world.json"))
            {

                using(var fStream = new FileStream(worldsPath + "/world.json", FileMode.Open))
                {
                    using (var bStream = new BufferedStream(fStream))
                    {
                        //���Ͽ��� ����Ʈ�� �о��
                        byte[] bytes = new byte[bStream.Length];
                        bStream.Read(bytes, 0, bytes.Length);

                        //byte[]�� json ��Ʈ������ ����
                        string json = Encoding.UTF8.GetString(bytes);

                        //Json String�� JObject�� ���� (Linq ó�� ���� ����)
                        JObject worldJson = JObject.Parse(json);

                        //WorldNode�� Scroll Rect�� Content�� �ڽ����� �ν��Ͻ�ȭ �ϰ�
                        //WorldNodeControl ������Ʈ�� �ҷ��´�.
                        WorldNodeControl worldNodeControl = 
                            Instantiate(worldNode, scrollRect.content.transform)
                            .GetComponent<WorldNodeControl>();

                        //Json String���� worldName�� �õ带 �о WorldNode�� �ؽ�Ʈ�� �����Ѵ�.
                        worldNodeControl.SetWorldText((string)worldJson["worldName"], (int)worldJson["seed"]);

                        //Ŭ�� ���� ���� ����
                        worldNodeControl.SetOnClick(OnClickWorldNode);
                    }
                }
            }
        }
    }

    /// <summary>
    /// WorldNode�� ������ ���� ����
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
