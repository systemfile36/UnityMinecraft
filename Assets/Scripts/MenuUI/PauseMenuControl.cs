using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuControl : MonoBehaviour
{
    /// <summary>
    /// 로딩화면에 대한 참조
    /// </summary>
    public LoadingControl loadingControl;


    private bool _IsPaused = false;

    /// <summary>
    /// 일시 정지 여부
    /// </summary>
    public bool IsPaused
    {
        get { return _IsPaused; }
    }

    /// <summary>
    /// timeScale을 0f로 설정 후 일시정지 메뉴 활성화
    /// </summary>
    public void SetPause()
    {
        //시간 정지
        Time.timeScale = 0f;

        //커서 상태 변경
        Cursor.lockState = CursorLockMode.None;

        _IsPaused = true;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 일시 정지 메뉴를 비활성화 하고 시간 재개
    /// </summary>
    public void Continue()
    {
        gameObject.SetActive(false);

        //커서 상태 변경
        Cursor.lockState = CursorLockMode.Locked;

        _IsPaused = false;

        //시간 재개
        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// 저장하고 나가기 버튼이 눌렸을 때
    /// </summary>
    public void OnSaveAndQuitCliked()
    {
        //병렬로 저장하고 상태를 받아온다.
        System.IAsyncResult saveResult = 
            SaveManager.SaveWorldAsync(GameManager.Mgr.World.worldData, false);

        //로딩 화면을 띄운다.
        loadingControl.LoadingStart(saveResult);

        //코루틴 시작
        StartCoroutine(WaitSaveComplete(saveResult));
    }

    /// <summary>
    /// 저장이 완료될때까지 대기하다가 
    /// 완료되면 메인메뉴로 이동하는 코루틴
    /// </summary>
    IEnumerator WaitSaveComplete(System.IAsyncResult asyncResult)
    {
        while (!asyncResult.IsCompleted)
        {
            yield return null;
        }

        Continue();

        //메인 메뉴로
        GameManager.Mgr.LoadMainMenu();
    }
}
