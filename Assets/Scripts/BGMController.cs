using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMController : MonoBehaviour
{
    private AudioSource audioSource = null;

    /// <summary>
    /// bgm용 AudioClip 목록
    /// </summary>
    private AudioClip[] bgms;

    private int currentBgmIndex = 0;
    
    private string currentBgm = "";

    /// <summary>
    /// settings의 bgmVolume 값이 바뀌었는지 감시하기 위한 변수
    /// </summary>
    private float lastBgmVolume = 1.0f;

    /// <summary>
    /// 현재 재생중인 BGM 이름
    /// </summary>
    public string CurrentBgm
    {
        get { return currentBgm; }
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        //AudioClip의 배열을 받아온다.
        bgms = GameManager.Mgr.GetAudioClips("bgm_1", "bgm_2", "bgm_3");

        //초기화
        lastBgmVolume = GameManager.Mgr.settings.bgmVolume;
        audioSource.volume = lastBgmVolume;
    }

    void Start()
    {
        audioSource.loop = false;

        //현재 bgm 선택
        AudioClip temp = GetBgmAudioClip();

        //재생
        audioSource.PlayOneShot(temp);
    }

    

    void Update()
    {
        //음악이 재생 중이 아니라면
        if(!audioSource.isPlaying)
        {
            //재생할 BGM 선택
            AudioClip temp = GetBgmAudioClip();
            
            //재생
            audioSource.PlayOneShot(temp);
        }

        //bgmVolume이 변경되었다면
        if(lastBgmVolume != GameManager.Mgr.settings.bgmVolume)
        {
            //변경된 볼륨으로 변경
            audioSource.volume = GameManager.Mgr.settings.bgmVolume;
        }
    }

    /// <summary>
    /// 재생할 Bgm의 AudioClip을 받아온다.
    /// bgms 배열을 순화한다.
    /// </summary>
    /// <returns></returns>
    AudioClip GetBgmAudioClip()
    {
        //인덱스 순회
        if (currentBgmIndex > bgms.Length - 1)
            currentBgmIndex = 0;

        AudioClip clip = bgms[currentBgmIndex];
        currentBgmIndex++;

        //현재 BGM 이름 설정
        currentBgm = clip.name;

        return clip;
    }

    /// <summary>
    /// bgm의 volume을 설정한다.
    /// </summary>
    public void SetBgmVolume(float volume)
    {
        audioSource.volume = volume;
    }
}
