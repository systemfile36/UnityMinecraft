using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ItemSlot의 데이터를 UI에 보여주는 클래스 
/// ItemSlot을 실제로 가시화 하는 역할
/// </summary>
public class UI_ItemSlot : MonoBehaviour
{
    //연결 되었는지 여부
    public bool IsLinked = false;

    public ItemSlot itemSlot;

    [Header("Information about Slot's Item")]
    public Image sImage;
    public Image sIcon;
    public Text sAmount;

    //월드에 대한 참조
    World world;

    private void Awake()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }
    /// <summary>
    /// 아이템 존재 여부
    /// </summary>
    public bool IsHasItem
    {
        get
        {
            if (itemSlot == null)
                return false;
            else
            {
                //연결된 아이템 슬롯의 ItemStack null 여부 반환
                return itemSlot.IsHasItem;
            }
        }
    }

    /// <summary>
    /// ItemSlot과 UI 연결하는 메소드
    /// 상호간의 참조 확보
    /// </summary>
    /// <param name="itemSlot"></param>
    public void Link(ItemSlot itemSlot)
    {
        this.itemSlot = itemSlot;
        IsLinked = true;
        itemSlot.Link_UI_Slot(this);

        //연결 될때마다 갱신
        RefreshSlot();
    }

    /// <summary>
    /// ItemSlot과의 연결 끊기
    /// </summary>
    public void UnLink()
    {
        itemSlot.UnLink_UI_Slot();
        itemSlot = null;

        //연결 끊길 때마다 갱신
        RefreshSlot();
    }

    /// <summary>
    /// UI 슬롯을 갱신한다.
    /// 해당 슬롯의 아이템 정보가 수정되었을 때 호출
    /// </summary>
    public void RefreshSlot()
    {
        //Debug.Log(itemSlot.IsHasItem);
        //이 UI 슬롯의 아이템 슬롯에 아이템이 있다면
        if (itemSlot != null && itemSlot.IsHasItem)
        {
        
            //아이템에 대한 정보들 받아서 설정함
            sIcon.sprite = world.blockTypes[itemSlot.itemStack.id].icon;
            sAmount.text = itemSlot.itemStack.amount.ToString();
            //초기엔 비활성화 상태이므로 활성화 시켜줌
            sIcon.enabled = true;
            //텍스트 활성화
            sAmount.enabled = true;
        }
        else
        {
            //아이템이 없다면 비운다.
            ClearSlot();
        }

    }

    /// <summary>
    /// UI 슬롯의 아이템 정보를 비운다.
    /// 해당 슬롯에서 아이템이 사라졌을 때 호출
    /// </summary>
    public void ClearSlot()
	{
        //이미지와 텍스트 모두 비활성화
        sIcon.sprite = null;
        sAmount.text = "";
        sIcon.enabled = false;
        sAmount.enabled = false;
	}

    //UI 슬롯이 사라지면 연결을 해제한다.
	private void OnDestroy()
	{
        //파괴 되었을 때, 연결된 상태라면
		if(IsLinked)
		{
            //연결 해제
            itemSlot.UnLink_UI_Slot();
		}
	}
}

/// <summary>
/// 슬롯의 아이템 데이터를 가지고 있는 클래스
/// 표시되든 아니든 아이템 정보를 Holding함
/// </summary>
public class ItemSlot
{
    public ItemStack itemStack;
    private UI_ItemSlot uItemSlot;

    /// <summary>
    /// 생성 되었을 때 UI 슬롯을 받아서 연결함
    /// </summary>
    /// <param name="uItemSlot"></param>
    public ItemSlot(UI_ItemSlot uItemSlot)
	{
        itemStack = null;

        this.uItemSlot = uItemSlot;

        uItemSlot.Link(this);
	}

    /// <summary>
    /// UI_ItemSlot과 ItemStack을 같이 받는 생성자
    /// </summary>
    /// <param name="uItemSlot"></param>
    /// <param name="itemStack"></param>
    public ItemSlot(UI_ItemSlot uItemSlot, ItemStack itemStack)
	{
        this.uItemSlot = uItemSlot;
        this.itemStack = itemStack;
        uItemSlot.Link(this);
    }


    /// <summary>
    /// UI 슬롯과 연결
    /// 상호간의 참조 확보
    /// </summary>
    /// <param name="uSlot"></param>
    public void Link_UI_Slot(UI_ItemSlot uSlot)
	{
        uItemSlot = uSlot;
	}

    /// <summary>
    /// UI 슬롯과 분리
    /// </summary>
    /// <param name="uSlot"></param>
    public virtual void UnLink_UI_Slot()
	{
        uItemSlot = null;
	}

    /// <summary>
    /// 이 슬롯의 아이템에 감소시킬 개수를 받아서
    /// 슬롯에 반영하고 감소시킨 개수를 반환한다.
    /// </summary>
    /// <param name="amount">감소시킬 개수</param>
    /// <returns>감소시킨 개수</returns>
    public int TakeItem(int amount)
    {
        //감소하는 개수를 0에서 현재의 개수로 Clamp함
        int reduce_value = Mathf.Clamp(amount, 0, itemStack.amount);

        //감소
        itemStack.amount -= reduce_value;

        //만약 감소한 후 남은 개수가 0이라면
        if (itemStack.amount == 0)
        {
            //스택 비움, 이때 itemStack이 null이 되므로 주의
            ClearSlot();
            return reduce_value;
        }
        else
		{
            RefreshUI_Slot();
            return reduce_value;
		}
	}

    /// <summary>
    /// 현재 슬롯의 ItemStack을 반환하고 
    /// 슬롯을 비움
    /// </summary>
    /// <returns></returns>
    public ItemStack TakeItemAll()
	{
        ItemStack temp = new ItemStack(itemStack.id, itemStack.amount);
        ClearSlot();
        return temp;
	}

    /// <summary>
    /// itemStack을 설정한 후 UI 슬롯 갱신하는 메소드
    /// </summary>
    /// <param name="itemStack"></param>
    public void SetItemStack(ItemStack itemStack)
	{
        this.itemStack = itemStack;
        RefreshUI_Slot();
	}

    /// <summary>
    /// 개수와 최대사이즈를 받아서 자신의 ItemSlot의 amount에 더한다.
    /// 최대 사이즈를 넘길 경우 넘은 개수를 반환, 아니면 0을 반환
    /// -1은 예외
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="MaxSize"></param>
    /// <returns></returns>
    public int AddStackAmount(int amount, byte MaxSize)
	{
        if (itemStack == null || amount < 0)
            return -1;

        //itemStack의 amount에 더한다.
        itemStack.amount += amount;

        //넘치는 양을 미리 저장
        int temp = itemStack.amount - MaxSize;

        //이 값이 음수이거나 0이면 넘치지 않았다는 말이므로
        //0으로 세팅한다.
        if (temp <= 0)
            temp = 0;

        //넘친 만큼 빼준다.
        itemStack.amount -= temp;

        RefreshUI_Slot();

        return temp;

	}

    /// <summary>
    /// 아이템 스택의 내용 비움
    /// </summary>
    public void ClearSlot()
	{
        itemStack = null;
        RefreshUI_Slot();
	}

    /// <summary>
    /// ItemSlot과 연결된 UI_ItemSlot을 갱신한다.
    /// </summary>
    public virtual void RefreshUI_Slot()
	{
        if(uItemSlot != null)
		{
            uItemSlot.RefreshSlot();
		}
	}

    /// <summary>
    /// 아이템 정보가 있는지 확인
    /// </summary>
    public bool IsHasItem
    {
        get
        {
            if (itemStack != null)
            {
                
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
