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
    /// 지정된 title과 text로 MsgBox를 띄운다. 
    /// 인자로 받은 onClickYes을 Yes버튼의 이벤트에 연결한다.
    /// No, Cancel 일때는 단순히 창만 닫는다.
    /// </summary>
    public void MsgBoxAsync(string title, string text, UnityEngine.Events.UnityAction onClickYes)
    {
        //MsgBox에 제목과 내용을 설정한다.
        this.title.text = title;
        this.text.text = text;

        //MsgBox 활성화
        msgBox.SetActive(true);

        //기존의 리스너 전부 제거
        btnYes.onClick.RemoveAllListeners();

        //Yes 버튼에 콜백 연결
        btnYes.onClick.AddListener(onClickYes);


    }

    /// <summary>
    /// MsgBox를 닫는다.
    /// </summary>
    public void CloseMsgBox()
    {
        //Yes 버튼의 리스너 모두 제거
        btnYes.onClick.RemoveAllListeners();

        //메시지 박스 비활성화
        msgBox.SetActive(false);
    }

}

