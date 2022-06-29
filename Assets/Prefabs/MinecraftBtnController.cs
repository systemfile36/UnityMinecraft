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
            btn.onClick.AddListener(PlayClickSE);
    }

    /// <summary>
    /// GameManager�� ������ UIAudioSource���� Ŭ�� ȿ������ 
    /// settings�� seVolume��ŭ���� ���
    /// </summary>
    void PlayClickSE()
    {
        GameManager.Mgr.UIAudioSource.PlayOneShot(GameManager.Mgr.GetAudioClip("click"),
            GameManager.Mgr.settings.seVolume);
    }
}
