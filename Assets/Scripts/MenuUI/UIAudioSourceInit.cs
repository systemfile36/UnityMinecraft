using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameManager에 자신의 AudioSource를 추가
/// </summary>
public class UIAudioSourceInit : MonoBehaviour
{
    void Awake()
    {
        AudioSource source = GetComponent<AudioSource>();

        if (source != null)
        {
            //UIAudioSource를 추가
            GameManager.Mgr.UIAudioSource = source;
        }
    }

    //파괴될 때는 참조를 해제한다.
    void OnDestroy()
    {
        if(GameManager.Mgr != null)
            GameManager.Mgr.UIAudioSource = null;
    }
}
