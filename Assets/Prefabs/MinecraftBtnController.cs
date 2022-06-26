using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinecraftBtnController : MonoBehaviour
{

    //Start���� Button�� Ŭ�� �Ҹ���� �̺�Ʈ �߰�
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(() => GameManager.Mgr.PlayAudio(GameManager.Mgr.UIAudioSource, "click"));
    }


}
