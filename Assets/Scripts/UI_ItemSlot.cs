using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ItemSlot�� �����͸� UI�� �����ִ� Ŭ���� 
/// ItemSlot�� ������ ����ȭ �ϴ� ����
/// </summary>
public class UI_ItemSlot : MonoBehaviour
{
    //���� �Ǿ����� ����
    public bool IsLinked = false;

    public ItemSlot itemSlot;

    [Header("Information about Slot's Item")]
    public Image sImage;
    public Image sIcon;
    public Text sAmount;

    //���忡 ���� ����
    World world;

    private void Awake()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }
    /// <summary>
    /// ������ ���� ����
    /// </summary>
    public bool IsHasItem
    {
        get
        {
            if (itemSlot == null)
                return false;
            else
            {
                //����� ������ ������ ItemStack null ���� ��ȯ
                return itemSlot.IsHasItem;
            }
        }
    }

    /// <summary>
    /// ItemSlot�� UI �����ϴ� �޼ҵ�
    /// ��ȣ���� ���� Ȯ��
    /// </summary>
    /// <param name="itemSlot"></param>
    public void Link(ItemSlot itemSlot)
    {
        this.itemSlot = itemSlot;
        IsLinked = true;
        itemSlot.Link_UI_Slot(this);

        //���� �ɶ����� ����
        RefreshSlot();
    }

    /// <summary>
    /// ItemSlot���� ���� ����
    /// </summary>
    public void UnLink()
    {
        itemSlot.UnLink_UI_Slot();
        itemSlot = null;

        //���� ���� ������ ����
        RefreshSlot();
    }

    /// <summary>
    /// UI ������ �����Ѵ�.
    /// �ش� ������ ������ ������ �����Ǿ��� �� ȣ��
    /// </summary>
    public void RefreshSlot()
    {
        //Debug.Log(itemSlot.IsHasItem);
        //�� UI ������ ������ ���Կ� �������� �ִٸ�
        if (itemSlot != null && itemSlot.IsHasItem)
        {
        
            //�����ۿ� ���� ������ �޾Ƽ� ������
            sIcon.sprite = world.blockTypes[itemSlot.itemStack.id].icon;
            sAmount.text = itemSlot.itemStack.amount.ToString();
            //�ʱ⿣ ��Ȱ��ȭ �����̹Ƿ� Ȱ��ȭ ������
            sIcon.enabled = true;
            //�ؽ�Ʈ Ȱ��ȭ
            sAmount.enabled = true;
        }
        else
        {
            //�������� ���ٸ� ����.
            ClearSlot();
        }

    }

    /// <summary>
    /// UI ������ ������ ������ ����.
    /// �ش� ���Կ��� �������� ������� �� ȣ��
    /// </summary>
    public void ClearSlot()
	{
        //�̹����� �ؽ�Ʈ ��� ��Ȱ��ȭ
        sIcon.sprite = null;
        sAmount.text = "";
        sIcon.enabled = false;
        sAmount.enabled = false;
	}

    //UI ������ ������� ������ �����Ѵ�.
	private void OnDestroy()
	{
        //�ı� �Ǿ��� ��, ����� ���¶��
		if(IsLinked)
		{
            //���� ����
            itemSlot.UnLink_UI_Slot();
		}
	}
}

/// <summary>
/// ������ ������ �����͸� ������ �ִ� Ŭ����
/// ǥ�õǵ� �ƴϵ� ������ ������ Holding��
/// </summary>
public class ItemSlot
{
    public ItemStack itemStack;
    private UI_ItemSlot uItemSlot;

    /// <summary>
    /// ���� �Ǿ��� �� UI ������ �޾Ƽ� ������
    /// </summary>
    /// <param name="uItemSlot"></param>
    public ItemSlot(UI_ItemSlot uItemSlot)
	{
        itemStack = null;

        this.uItemSlot = uItemSlot;

        uItemSlot.Link(this);
	}

    /// <summary>
    /// UI_ItemSlot�� ItemStack�� ���� �޴� ������
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
    /// UI ���԰� ����
    /// ��ȣ���� ���� Ȯ��
    /// </summary>
    /// <param name="uSlot"></param>
    public void Link_UI_Slot(UI_ItemSlot uSlot)
	{
        uItemSlot = uSlot;
	}

    /// <summary>
    /// UI ���԰� �и�
    /// </summary>
    /// <param name="uSlot"></param>
    public virtual void UnLink_UI_Slot()
	{
        uItemSlot = null;
	}

    /// <summary>
    /// �� ������ �����ۿ� ���ҽ�ų ������ �޾Ƽ�
    /// ���Կ� �ݿ��ϰ� ���ҽ�Ų ������ ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="amount">���ҽ�ų ����</param>
    /// <returns>���ҽ�Ų ����</returns>
    public int TakeItem(int amount)
    {
        //�����ϴ� ������ 0���� ������ ������ Clamp��
        int reduce_value = Mathf.Clamp(amount, 0, itemStack.amount);

        //����
        itemStack.amount -= reduce_value;

        //���� ������ �� ���� ������ 0�̶��
        if (itemStack.amount == 0)
        {
            //���� ���, �̶� itemStack�� null�� �ǹǷ� ����
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
    /// ���� ������ ItemStack�� ��ȯ�ϰ� 
    /// ������ ���
    /// </summary>
    /// <returns></returns>
    public ItemStack TakeItemAll()
	{
        ItemStack temp = new ItemStack(itemStack.id, itemStack.amount);
        ClearSlot();
        return temp;
	}

    /// <summary>
    /// itemStack�� ������ �� UI ���� �����ϴ� �޼ҵ�
    /// </summary>
    /// <param name="itemStack"></param>
    public void SetItemStack(ItemStack itemStack)
	{
        this.itemStack = itemStack;
        RefreshUI_Slot();
	}

    /// <summary>
    /// ������ �ִ����� �޾Ƽ� �ڽ��� ItemSlot�� amount�� ���Ѵ�.
    /// �ִ� ����� �ѱ� ��� ���� ������ ��ȯ, �ƴϸ� 0�� ��ȯ
    /// -1�� ����
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="MaxSize"></param>
    /// <returns></returns>
    public int AddStackAmount(int amount, byte MaxSize)
	{
        if (itemStack == null || amount < 0)
            return -1;

        //itemStack�� amount�� ���Ѵ�.
        itemStack.amount += amount;

        //��ġ�� ���� �̸� ����
        int temp = itemStack.amount - MaxSize;

        //�� ���� �����̰ų� 0�̸� ��ġ�� �ʾҴٴ� ���̹Ƿ�
        //0���� �����Ѵ�.
        if (temp <= 0)
            temp = 0;

        //��ģ ��ŭ ���ش�.
        itemStack.amount -= temp;

        RefreshUI_Slot();

        return temp;

	}

    /// <summary>
    /// ������ ������ ���� ���
    /// </summary>
    public void ClearSlot()
	{
        itemStack = null;
        RefreshUI_Slot();
	}

    /// <summary>
    /// ItemSlot�� ����� UI_ItemSlot�� �����Ѵ�.
    /// </summary>
    public virtual void RefreshUI_Slot()
	{
        if(uItemSlot != null)
		{
            uItemSlot.RefreshSlot();
		}
	}

    /// <summary>
    /// ������ ������ �ִ��� Ȯ��
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
