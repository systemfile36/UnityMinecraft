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
            btn.onClick.AddListener(PlayClickSE);
    }

    /// <summary>
    /// GameManager에 설정된 UIAudioSource에서 클릭 효과음을 
    /// settings의 seVolume만큼으로 재생
    /// </summary>
    void PlayClickSE()
    {
        GameManager.Mgr.UIAudioSource.PlayOneShot(GameManager.Mgr.GetAudioClip("click"),
            GameManager.Mgr.settings.seVolume);
    }
}
