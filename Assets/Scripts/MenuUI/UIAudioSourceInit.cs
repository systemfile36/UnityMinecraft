using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameManager�� �ڽ��� AudioSource�� �߰�
/// </summary>
public class UIAudioSourceInit : MonoBehaviour
{
    void Awake()
    {
        AudioSource source = GetComponent<AudioSource>();

        if (source != null)
        {
            //UIAudioSource�� �߰�
            GameManager.Mgr.UIAudioSource = source;
        }
    }

    //�ı��� ���� ������ �����Ѵ�.
    void OnDestroy()
    {
        if(GameManager.Mgr != null)
            GameManager.Mgr.UIAudioSource = null;
    }
}
