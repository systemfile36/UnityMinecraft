using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEController : MonoBehaviour
{
    private AudioSource audioSource = null;

    /// <summary>
    /// se가 재생되는 최소 간격
    /// </summary>
    private float seInterval = 0.08f;

    private float seDelta = 0.08f;


    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

    }

    void Update()
    {
        if(seDelta > 0.0f)
            seDelta -= Time.unscaledDeltaTime;
    }

    /// <summary>
    /// AudioClip을 받아서 SE AudioSource에서 재생
    /// </summary>
    /// <param name="audioClip"></param>
    public void PlaySESound(AudioClip audioClip)
    {
        if (CheckSeInterval())
            audioSource.PlayOneShot(audioClip);
    }

    /// <summary>
    /// 이름을 받아서 SE AudioSource에서 재생
    /// </summary>
    /// <param name="seName"></param>
    public void PlaySESound(string seName)
    {
        if(CheckSeInterval())
            audioSource.PlayOneShot(GameManager.Mgr.GetAudioClip(seName));
    }

    /// <summary>
    /// 지정된 딜레이에 도달 했는지 체크
    /// </summary>
    /// <returns></returns>
    private bool CheckSeInterval()
    {
        if (seDelta <= 0.0f)
        {
            //Debug.Log(seDelta);
            seDelta = seInterval;
            return true;
        } 
        else
            return false;
    }
}
