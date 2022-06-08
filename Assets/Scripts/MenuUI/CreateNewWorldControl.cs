using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class CreateNewWorldControl : MonoBehaviour
{
    public MenuControl menuControl;

    public TMP_InputField tbName;
    public TMP_InputField tbSeed;

    private GameMode gameMode = GameMode.Debug;
    public Button btnGameMode;

    public LoadingControl loadingControl;

    void Awake()
    {
        //Seed â�� �⺻������ ������ �õ带 ����
        tbSeed.text = Random.Range(int.MinValue, int.MaxValue).ToString();
    }

    /// <summary>
    /// ���� ��� ���� ��ư ��ȯ, ���� �ۼ�
    /// </summary>
    public void OnClickBtnGameMode()
    {
        
    }

    /// <summary>
    /// Create ��ư�� ������ ��
    /// </summary>
    public void OnClickCreate()
    {
        string worldPath = GamePaths.SavePath + "/" + tbName.text + "/";
        //���� World �̸��� ���͸��� �̹� �ִٸ�
        if (Directory.Exists(worldPath))
        {
            menuControl.msgBoxControl.MsgBoxAsync("Warning", $"{tbName.text} is already exists!\n" +
                $"Do you want to Overwrite ?", () => 
                {
                    //�����ϴ� ���͸� ��� ����
                    Directory.Delete(worldPath, true);

                    //���� �� �ε�
                    CreateNewWorld();
                });
        }
        else
        {
            CreateNewWorld();
        }

        
        
    }

    /// <summary>
    /// �̸��� �õ带 �����ϰ� World ���� �ε��Ѵ�.
    /// </summary>
    void CreateNewWorld()
    {
        //������ �̸��� �ݿ�
        World.worldName = tbName.text;

        //���� �õ� ���� int �ִ밪���� ũ�ٸ�
        //tbSeed�� �ؽ�Ʈ�� ���� ��Ŀ�� �̵�
        long seed = long.Parse(tbSeed.text);
        if (seed > int.MaxValue)
        {
            tbSeed.text = "";
            tbSeed.Select();
            return;
        }

        //�õ� �� ����
        World.seed = (int)seed;

        //World ���� �񵿱������� �ε��ϰ� �ε�â�� ����.
        loadingControl.LoadingStart(GameManager.Mgr.LoadWorld());
    }

    public void OnClickCancel()
    {
        menuControl.MoveToMenu(transform.name, "MainMenu");
    }
}
