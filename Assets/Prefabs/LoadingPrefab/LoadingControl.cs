using System.Collections;
using System.Collections.Generic;
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

    void Update()
    {
        //�ε� ���̰�, result�� �Ϸ�Ǿ��ٸ�
        //�ε�ȭ���� ���� result�� �ʱ�ȭ�Ѵ�.
        if(isLoading && result != null
            && result.IsCompleted)
        {
            loading.SetActive(false);
            result = null;
        }
    }
}
