using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

    /// <summary>
    /// AsyncOperation을 받아서 IAsyncResult 멤버에 세팅하고
    /// 로딩화면 활성화
    /// </summary>
    /// <param name="result"></param>
    public void LoadingStart(AsyncOperation result)
    {
        //AsyncOperation을 IAsyncResult 구현 객체로 만들어서 넘김
        LoadingStart(new AsyncResult(result));
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
            isLoading = false;
        }
    }

    /// <summary>
    /// AsyncOperation을 IAsyncResult로 사용하기 위한 클래스
    /// </summary>
    private class AsyncResult : System.IAsyncResult
    {
        private AsyncOperation operation;

        public AsyncResult(AsyncOperation ao)
        {
            operation = ao;
        }

        public object AsyncState { get { return null; } }

        public WaitHandle AsyncWaitHandle { get { return null; } }
        public bool CompletedSynchronously { get { return false; } }

        public bool IsCompleted
        {
            get { return operation.isDone; }
        }
    }
}
