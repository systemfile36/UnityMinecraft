using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryControl : MonoBehaviour
{

    private StarterAssets.StarterAssetsInputs _input;

    private GameObject defaultInv = null;
    private GameObject creativeInv = null;
    

    void Awake()
	{
        _input = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssets.StarterAssetsInputs>();

        //�⺻ �κ��丮 ������ �Ҵ�
        defaultInv = gameObject.transform.GetChild(0).gameObject;
    }

	void Start()
    {
        //ũ����Ƽ�� ��尡 �ƴ϶�� �⺻ �κ��丮
        if(GameManager.Mgr.settings.gameMode != GameMode.Creative)
		{
            defaultInv.SetActive(_input.IsOnUI);
		}
    }

    void Update()
    {
        if(GameManager.Mgr.settings.gameMode != GameMode.Creative)
		{
            defaultInv.SetActive(_input.IsOnUI);
        }
    }
}
