using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SE�� ����ϴ� ������Ʈ
/// </summary>
public class SEController : MonoBehaviour
{
    private AudioSource audioSource = null;

    /// <summary>
    /// se�� ����Ǵ� �ּ� ����
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
    /// AudioClip�� �޾Ƽ� SE AudioSource���� ���
    /// </summary>
    /// <param name="audioClip"></param>
    public void PlaySESound(AudioClip audioClip)
    {
        //if (CheckSeInterval())
            audioSource.PlayOneShot(audioClip);
    }

    /// <summary>
    /// �̸��� �޾Ƽ� SE AudioSource���� ���
    /// </summary>
    /// <param name="seName"></param>
    public void PlaySESound(string seName)
    {
        //if(CheckSeInterval())
            audioSource.PlayOneShot(GameManager.Mgr.GetAudioClip(seName));
    }

    /// <summary>
    /// ȿ���� AudioClip�� �� ���� ������ �޾� �Ҹ� ���
    /// ���� ���� ���� ������ ��ġ ������ ��
    /// </summary>
    public void PlayBlockSESound(AudioClip audioClip, BlockSESub seSub)
    {
        //������ ����
        float subVolume = 0.0f;

        //�� �ൿ�� ���� pitch�� volume�� �����Ѵ�.
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

        //������ ������ seVolume�� ���Ѵ�.
        audioSource.PlayOneShot(audioClip, subVolume * GameManager.Mgr.settings.seVolume);
    }

    /// <summary>
    /// ȿ���� �̸��� �� ���� ������ �޾� �Ҹ� ���
    /// ���� ���� ���� ������ ��ġ ������ ��
    /// </summary>
    public void PlayBlockSESound(string seName, BlockSESub seSub)
    {
        AudioClip audioClip = GameManager.Mgr.GetAudioClip(seName);

        //����ó��
        if(audioClip != null)
            PlayBlockSESound(audioClip, seSub);
    }

    /*
    /// <summary>
    /// ������ �����̿� ���� �ߴ��� üũ
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
