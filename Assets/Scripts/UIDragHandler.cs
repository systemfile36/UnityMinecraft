using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class UIDragHandler : MonoBehaviour
{
    //[SerializeField]는 인스펙터에서 보기 위함

    [SerializeField] private UI_ItemSlot cursorSlot = null;

    private ItemSlot cursorItemSlot = null;

    //UI 요소에 대한 레이캐스터
    [SerializeField] private GraphicRaycaster raycaster = null;

    //포인터 위치를 위한 변수
    private PointerEventData pointerEventData = null;

    
    //오브젝트의 이벤트 시스템
    [SerializeField] private EventSystem eventSystem = null;

    //world에 대한 참조
    //World world;

    //Player의 입출력 스크립트 참조
    StarterAssets.StarterAssetsInputs _input;

	void Start()
	{
        //world = GameObject.Find("World").GetComponent<World>();
        _input = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssets.StarterAssetsInputs>();
        cursorItemSlot = new ItemSlot(cursorSlot);
	}

	void FixedUpdate()
	{
        //UI 상에 있지 않으면 아무것도 하지 않음
        if (!_input.IsOnUI)
            return;

        //마우스가 이 프레임에 입력되었다면
        if(Mouse.current.leftButton.wasPressedThisFrame)
		{
            if(CheckSlot() != null)
			{
                Debug.Log("ItemSlot Cliked");
			}
            else
			{
                Debug.Log("No ItemSlot on There!");
			}
		}
	}

    /// <summary>
    /// 마우스 위치의 UI_ItemSlot 컴포넌트를 가지고 온다.
    /// </summary>
    /// <returns></returns>
    private UI_ItemSlot CheckSlot()
	{
        //이벤트 시스템에서 포인터 정보를 불러온다.
        pointerEventData = new PointerEventData(eventSystem);

        //현재 마우스 위치로 설정한다.
        pointerEventData.position = Mouse.current.position.ReadValue();

        //레이 캐스팅을 한다.
        //현재 마우스 위치에 있는 모든 UI 요소를 가져온다.
        List<RaycastResult> r = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, r);

        //레이 캐스트 결과에서 UI_ItemSlot만 받아온다.
        foreach(RaycastResult result in r)
		{
            //만약 태그가 UI_ItemSlot이라면
            if(result.gameObject.tag == "UI_ItemSlot")
			{
                //UI_ItemSlot 컴포넌트를 반환한다.
                return result.gameObject.GetComponent<UI_ItemSlot>();
			}
		}

        //마우스 위치에 UI_ItemSlot이 없으면 null 리턴
        return null;

	}

}
