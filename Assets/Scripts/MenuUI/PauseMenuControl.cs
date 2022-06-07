using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuControl : MonoBehaviour
{
    /// <summary>
    /// �ε�ȭ�鿡 ���� ����
    /// </summary>
    public LoadingControl loadingControl;


    private bool _IsPaused = false;

    /// <summary>
    /// �Ͻ� ���� ����
    /// </summary>
    public bool IsPaused
    {
        get { return _IsPaused; }
    }

    /// <summary>
    /// timeScale�� 0f�� ���� �� �Ͻ����� �޴� Ȱ��ȭ
    /// </summary>
    public void SetPause()
    {
        //�ð� ����
        Time.timeScale = 0f;

        //Ŀ�� ���� ����
        Cursor.lockState = CursorLockMode.None;

        _IsPaused = true;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// �Ͻ� ���� �޴��� ��Ȱ��ȭ �ϰ� �ð� �簳
    /// </summary>
    public void Continue()
    {
        gameObject.SetActive(false);

        //Ŀ�� ���� ����
        Cursor.lockState = CursorLockMode.Locked;

        _IsPaused = false;

        //�ð� �簳
        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// �����ϰ� ������ ��ư�� ������ ��
    /// </summary>
    public void OnSaveAndQuitCliked()
    {
        //���ķ� �����ϰ� ���¸� �޾ƿ´�.
        System.IAsyncResult saveResult = 
            SaveManager.SaveWorldAsync(GameManager.Mgr.World.worldData, false);

        //�ε� ȭ���� ����.
        loadingControl.LoadingStart(saveResult);

        //�ڷ�ƾ ����
        StartCoroutine(WaitSaveComplete(saveResult));
    }

    /// <summary>
    /// ������ �Ϸ�ɶ����� ����ϴٰ� 
    /// �Ϸ�Ǹ� ���θ޴��� �̵��ϴ� �ڷ�ƾ
    /// </summary>
    IEnumerator WaitSaveComplete(System.IAsyncResult asyncResult)
    {
        while (!asyncResult.IsCompleted)
        {
            yield return null;
        }

        Continue();

        //���� �޴���
        GameManager.Mgr.LoadMainMenu();
    }
}
