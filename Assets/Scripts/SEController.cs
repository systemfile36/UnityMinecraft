using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SE를 재생하는 오브젝트
/// </summary>
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
        //if (CheckSeInterval())
            audioSource.PlayOneShot(audioClip);
    }

    /// <summary>
    /// 이름을 받아서 SE AudioSource에서 재생
    /// </summary>
    /// <param name="seName"></param>
    public void PlaySESound(string seName)
    {
        //if(CheckSeInterval())
            audioSource.PlayOneShot(GameManager.Mgr.GetAudioClip(seName));
    }

    /// <summary>
    /// 효과음 AudioClip과 블럭 동작 정보를 받아 소리 재생
    /// 설정 값에 따른 볼륨과 피치 조정이 들어감
    /// </summary>
    public void PlayBlockSESound(AudioClip audioClip, BlockSESub seSub)
    {
        //적용할 볼륨
        float subVolume = 0.0f;

        //블럭 행동에 따라 pitch와 volume을 선택한다.
        switch(seSub)
        {
            case BlockSESub.Step:
                audioSource.pitch = GameManager.Mgr.settings.stepPitch;
                subVolume = GameManager.Mgr.settings.stepVolume;
                break;
            case BlockSESub.Placed:
                audioSource.pitch = GameManager.Mgr.settings.placedPitch;
                subVolume = GameManager.Mgr.settings.placedVolume;
                break;
            case BlockSESub.Breaked:
                audioSource.pitch = GameManager.Mgr.settings.breakedPitch;
                subVolume = GameManager.Mgr.settings.breakedVolume;
                break;
            case BlockSESub.Falled:
                audioSource.pitch = GameManager.Mgr.settings.falledPitch;
                subVolume = GameManager.Mgr.settings.falledVolume;
                break;
        }

        //적용할 볼륨에 seVolume을 곱한다.
        audioSource.PlayOneShot(audioClip, subVolume * GameManager.Mgr.settings.seVolume);
    }

    /// <summary>
    /// 효과음 이름과 블럭 동작 정보를 받아 소리 재생
    /// 설정 값에 따른 볼륨과 피치 조정이 들어감
    /// </summary>
    public void PlayBlockSESound(string seName, BlockSESub seSub)
    {
        AudioClip audioClip = GameManager.Mgr.GetAudioClip(seName);

        //예외처리
        if(audioClip != null)
            PlayBlockSESound(audioClip, seSub);
    }

    /*
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
    */
}
