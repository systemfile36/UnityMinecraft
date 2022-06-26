using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinecraftBtnController : MonoBehaviour
{

    //Start에서 Button의 클릭 소리재생 이벤트 추가
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(() => GameManager.Mgr.PlayAudio(GameManager.Mgr.UIAudioSource, "click"));
    }


}
