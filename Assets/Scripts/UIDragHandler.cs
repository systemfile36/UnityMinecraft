using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class UIDragHandler : MonoBehaviour
{
    //[SerializeField]는 인스펙터에서 보기 위함

    //홀딩 중인 아이템이 저장되는 슬롯, 
    [SerializeField] private UI_ItemSlot cursorSlot = null;

    //커서 슬롯과 연결된 아이템 슬롯
    private ItemSlot cursorItemSlot = null;

    //UI 요소에 대한 레이캐스터
    [SerializeField] private GraphicRaycaster raycaster = null;

    //포인터 위치를 위한 변수
    private PointerEventData pointerEventData = null;

    
    //오브젝트의 이벤트 시스템
    [SerializeField] private EventSystem eventSystem = null;

    //world에 대한 참조
    World world;

    //Player의 입출력 스크립트 참조
    StarterAssets.StarterAssetsInputs _input;

	void Awake()
	{
        world = GameObject.Find("World").GetComponent<World>();
        _input = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssets.StarterAssetsInputs>();

        //커서 슬롯과 연결된 아이템 슬롯
        cursorItemSlot = new ItemSlot(cursorSlot);
	}

	void Update()
	{
        //UI 상에 있지 않으면 아무것도 하지 않음
        if (!_input.IsOnUI)
            return;

        //커서 슬롯이 마우스를 따라다니게 만든다.
        cursorSlot.transform.position = Mouse.current.position.ReadValue();

        //마우스가 이 프레임에 입력되었다면
        if(Mouse.current.leftButton.wasPressedThisFrame)
		{
            //클릭 핸들러에 현재 위치의 슬롯을 넘긴다.
            HandleClickSlot(CheckSlot());

            //커서 슬롯에 아이템이 있으면 아이템을 홀딩중이라 표시한다.
            _input.IsHoldingCursor = cursorSlot.IsHasItem;
		}
	}

    /// <summary>
    /// 클릭한 위치의 슬롯을 받아서 
    /// 슬롯 클릭 시의 동작을 핸들링 함
    /// </summary>
    /// <param name="c_Slot">클릭한 곳의 슬롯</param>
    private void HandleClickSlot(UI_ItemSlot c_Slot)
	{
        //클릭한 곳의 슬롯이 null이거나
        //커서 슬롯에 홀딩중인 아이템이 없고, 클릭한 곳의 슬롯도 아이템이 없다면
        //그냥 return;
        if (c_Slot == null
            || (!cursorSlot.IsHasItem && !c_Slot.IsHasItem))
            return;

        //커서 슬롯에 홀딩중인 아이템이 없고, 클릭한 슬롯에는 있으면
        
        if(!cursorSlot.IsHasItem && c_Slot.IsHasItem)
		{
            //커서 슬롯으로 클릭한 슬롯의 ItemStack을 옮긴다.
            cursorItemSlot.SetItemStack(c_Slot.itemSlot.TakeItemAll());

            return;
		}

        //커서 슬롯에 홀딩중인 아이템이 있고, 클릭한 슬롯에는 없으면
        //내려 놓는다.
        if(cursorSlot.IsHasItem && !c_Slot.IsHasItem)
		{
            //클릭한 슬롯으로 홀딩중인 아이템을 옮긴다.
            c_Slot.itemSlot.SetItemStack(cursorItemSlot.TakeItemAll());

            return;
		}

        //커서 슬롯과 클릭한 슬롯, 양쪽에 아이템이 있다면
        //개수를 추가하거나 서로 스왑한다.
        if(cursorSlot.IsHasItem && c_Slot.IsHasItem)
		{
            //서로 다른 아이템이라면 스왑한다.
            if(cursorSlot.itemSlot.itemStack.id != c_Slot.itemSlot.itemStack.id)
			{
                ItemStack cursorTemp = cursorSlot.itemSlot.TakeItemAll();
                ItemStack clickTemp = c_Slot.itemSlot.TakeItemAll();

                cursorSlot.itemSlot.SetItemStack(clickTemp);

                c_Slot.itemSlot.SetItemStack(cursorTemp);

                return;
			}
            //서로 같은 아이템이라면 개수를 추가한다.
            //이때 최대값을 넘지 않게 함에 주의하라
            else
			{
                byte MaxSize = world.blockTypes[cursorSlot.itemSlot.itemStack.id].MaxStackSize;

                //클릭 슬롯에 더하고 최대값에서 넘친 값을 임시 저장한다.
                int cursorAmountTemp = c_Slot.itemSlot.AddStackAmount(cursorSlot.itemSlot.itemStack.amount, MaxSize);

                //예외
                if(cursorAmountTemp < 0)
				{
                    Debug.Log("Invalid amount of cursorSlot");
                    return;
				}
                //넘치지 않았다면 슬롯 비움
                else if(cursorAmountTemp == 0)
				{
                    cursorSlot.itemSlot.ClearSlot();
				}
                //넘쳤다면 커서 슬롯의 아이템 개수를 넘친 만큼의 양으로 설정하고 갱신
                else
				{
                    cursorSlot.itemSlot.itemStack.amount = cursorAmountTemp;
                    cursorSlot.RefreshSlot();
				}
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
