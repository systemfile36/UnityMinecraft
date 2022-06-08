using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MenuControl : MonoBehaviour
{

    /// <summary>
    /// �̸��� Transform���� �޴��� �����ϴ� ��ųʸ�
    /// </summary>
    private Dictionary<string, Transform> menus = new Dictionary<string, Transform>();

    /// <summary>
    /// MsgBoxControl�� ���� ����
    /// </summary>
    public MsgBoxControl msgBoxControl;

    void Awake()
    {
        //�ڽ��� Transform�� �̸��� �������� ��ųʸ����߰�
        for(int i = 0; i < transform.childCount; i++)
        {
            menus.Add(transform.GetChild(i).name, transform.GetChild(i)); 
        }

        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// �̸��� ���� ���� �޴��� ��Ȱ��ȭ �ϰ� �̵��� �޴� Ȱ��ȭ
    /// </summary>
    public void MoveToMenu(string current, string toMove)
    {
        menus[current].gameObject.SetActive(false);
        menus[toMove].gameObject.SetActive(true);    
    }

}
