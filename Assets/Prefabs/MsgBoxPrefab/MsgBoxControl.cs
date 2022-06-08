using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class MsgBoxControl : MonoBehaviour
{
    public GameObject msgBox;

    public TMP_Text title;

    public TMP_Text text;

    public Button btnYes;

    /// <summary>
    /// ������ title�� text�� MsgBox�� ����. 
    /// ���ڷ� ���� onClickYes�� Yes��ư�� �̺�Ʈ�� �����Ѵ�.
    /// No, Cancel �϶��� �ܼ��� â�� �ݴ´�.
    /// </summary>
    public void MsgBoxAsync(string title, string text, UnityEngine.Events.UnityAction onClickYes)
    {
        //MsgBox�� ����� ������ �����Ѵ�.
        this.title.text = title;
        this.text.text = text;

        //MsgBox Ȱ��ȭ
        msgBox.SetActive(true);

        //������ ������ ���� ����
        btnYes.onClick.RemoveAllListeners();

        //Yes ��ư�� �ݹ� ����
        btnYes.onClick.AddListener(onClickYes);


    }

    /// <summary>
    /// MsgBox�� �ݴ´�.
    /// </summary>
    public void CloseMsgBox()
    {
        //Yes ��ư�� ������ ��� ����
        btnYes.onClick.RemoveAllListeners();

        //�޽��� �ڽ� ��Ȱ��ȭ
        msgBox.SetActive(false);
    }

}

