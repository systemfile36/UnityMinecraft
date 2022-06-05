using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class LoadingControl : MonoBehaviour
{
    private GameObject loading;

    /// <summary>
    /// �ε� ������ ����
    /// </summary>
    private bool isLoading = false;

    /// <summary>
    /// �񵿱� �۾��� �Ϸ�Ǿ����� ���θ� �˱� ����
    /// </summary>
    private System.IAsyncResult result = null;

    void Awake()
    {
        //�ڽ��� Loading�� �޾ƿ´�.
        loading = transform.GetChild(0).gameObject;
    }

    /// <summary>
    /// IAsyncResult�� �޾Ƽ� ����� �����ϰ� �ε�ȭ�� Ȱ��ȭ
    /// </summary>
    public void LoadingStart(System.IAsyncResult result)
    {
        this.result = result;
        isLoading = true;

        loading.SetActive(true);
    }

    /// <summary>
    /// AsyncOperation�� �޾Ƽ� IAsyncResult ����� �����ϰ�
    /// �ε�ȭ�� Ȱ��ȭ
    /// </summary>
    /// <param name="result"></param>
    public void LoadingStart(AsyncOperation result)
    {
        //AsyncOperation�� IAsyncResult ���� ��ü�� ���� �ѱ�
        LoadingStart(new AsyncResult(result));
    }

    void Update()
    {
        //�ε� ���̰�, result�� �Ϸ�Ǿ��ٸ�
        //�ε�ȭ���� ���� result�� �ʱ�ȭ�Ѵ�.
        if(isLoading && result != null
            && result.IsCompleted)
        {
            loading.SetActive(false);
            result = null;
            isLoading = false;
        }
    }

    /// <summary>
    /// AsyncOperation�� IAsyncResult�� ����ϱ� ���� Ŭ����
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
