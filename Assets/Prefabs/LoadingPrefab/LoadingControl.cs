using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LoadingControl : MonoBehaviour
{
    private GameObject loading;

    /// <summary>
    /// 로딩 중인지 여부
    /// </summary>
    private bool isLoading = false;

    /// <summary>
    /// 비동기 작업이 완료되었는지 여부를 알기 위함
    /// </summary>
    private System.IAsyncResult result = null;

    void Awake()
    {
        //자식의 Loading을 받아온다.
        loading = transform.GetChild(0).gameObject;
    }

    /// <summary>
    /// IAsyncResult를 받아서 멤버에 세팅하고 로딩화면 활성화
    /// </summary>
    public void LoadingStart(System.IAsyncResult result)
    {
        this.result = result;
        isLoading = true;

        loading.SetActive(true);
    }

    void Update()
    {
        //로딩 중이고, result가 완료되었다면
        //로딩화면을 끄고 result를 초기화한다.
        if(isLoading && result != null
            && result.IsCompleted)
        {
            loading.SetActive(false);
            result = null;
        }
    }
}
