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

        //기본 인벤토리 변수에 할당
        defaultInv = gameObject.transform.GetChild(0).gameObject;
    }

	void Start()
    {
        //크리에티브 모드가 아니라면 기본 인벤토리
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
