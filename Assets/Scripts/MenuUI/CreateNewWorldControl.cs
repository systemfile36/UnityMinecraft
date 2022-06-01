using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateNewWorldControl : MonoBehaviour
{
    public MenuControl menuControl;

    public TMP_InputField tbName;
    public TMP_InputField tbSeed;

    private GameMode gameMode = GameMode.Debug;
    public Button btnGameMode;

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

    public void OnClickCreate()
    {
        //������ �̸��� �ݿ�
        World.worldName = tbName.text;

        //���� �õ� ���� int �ִ밪���� ũ�ٸ�
        //tbSeed�� �ؽ�Ʈ�� ���� ��Ŀ�� �̵�
        long seed = long.Parse(tbSeed.text);
        if(seed > int.MaxValue)
        {
            tbSeed.text = "";
            tbSeed.Select();
            return;
        }

        //�õ� �� ����
        World.seed = (int)seed;

        //���� �������� �Ѿ��.
        GameManager.Mgr.LoadWorld();
    }

    public void OnClickCancel()
    {
        menuControl.MoveToMenu(transform.name, "MainMenu");
    }
}
