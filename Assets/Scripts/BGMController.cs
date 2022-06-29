using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMController : MonoBehaviour
{
    private AudioSource audioSource = null;

    /// <summary>
    /// bgm�� AudioClip ���
    /// </summary>
    private AudioClip[] bgms;

    private int currentBgmIndex = 0;
    
    private string currentBgm = "";

    /// <summary>
    /// settings�� bgmVolume ���� �ٲ������ �����ϱ� ���� ����
    /// </summary>
    private float lastBgmVolume = 1.0f;

    /// <summary>
    /// ���� ������� BGM �̸�
    /// </summary>
    public string CurrentBgm
    {
        get { return currentBgm; }
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        //AudioClip�� �迭�� �޾ƿ´�.
        bgms = GameManager.Mgr.GetAudioClips("bgm_1", "bgm_2", "bgm_3");

        //�ʱ�ȭ
        lastBgmVolume = GameManager.Mgr.settings.bgmVolume;
        audioSource.volume = lastBgmVolume;
    }

    void Start()
    {
        audioSource.loop = false;

        //���� bgm ����
        AudioClip temp = GetBgmAudioClip();

        //���
        audioSource.PlayOneShot(temp);
    }

    

    void Update()
    {
        //������ ��� ���� �ƴ϶��
        if(!audioSource.isPlaying)
        {
            //����� BGM ����
            AudioClip temp = GetBgmAudioClip();
            
            //���
            audioSource.PlayOneShot(temp);
        }

        //bgmVolume�� ����Ǿ��ٸ�
        if(lastBgmVolume != GameManager.Mgr.settings.bgmVolume)
        {
            //����� �������� ����
            audioSource.volume = GameManager.Mgr.settings.bgmVolume;
        }
    }

    /// <summary>
    /// ����� Bgm�� AudioClip�� �޾ƿ´�.
    /// bgms �迭�� ��ȭ�Ѵ�.
    /// </summary>
    /// <returns></returns>
    AudioClip GetBgmAudioClip()
    {
        //�ε��� ��ȸ
        if (currentBgmIndex > bgms.Length - 1)
            currentBgmIndex = 0;

        AudioClip clip = bgms[currentBgmIndex];
        currentBgmIndex++;

        //���� BGM �̸� ����
        currentBgm = clip.name;

        return clip;
    }

    /// <summary>
    /// bgm�� volume�� �����Ѵ�.
    /// </summary>
    public void SetBgmVolume(float volume)
    {
        audioSource.volume = volume;
    }
}
