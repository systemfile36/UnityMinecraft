using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class UIDragHandler : MonoBehaviour
{
    //[SerializeField]�� �ν����Ϳ��� ���� ����

    //Ȧ�� ���� �������� ����Ǵ� ����, 
    [SerializeField] private UI_ItemSlot cursorSlot = null;

    //Ŀ�� ���԰� ����� ������ ����
    private ItemSlot cursorItemSlot = null;

    //UI ��ҿ� ���� ����ĳ����
    [SerializeField] private GraphicRaycaster raycaster = null;

    //������ ��ġ�� ���� ����
    private PointerEventData pointerEventData = null;

    
    //������Ʈ�� �̺�Ʈ �ý���
    [SerializeField] private EventSystem eventSystem = null;

    //world�� ���� ����
    World world;

    //Player�� ����� ��ũ��Ʈ ����
    StarterAssets.StarterAssetsInputs _input;

	void Awake()
	{
        world = GameObject.Find("World").GetComponent<World>();
        _input = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssets.StarterAssetsInputs>();

        //Ŀ�� ���԰� ����� ������ ����
        cursorItemSlot = new ItemSlot(cursorSlot);
	}

	void Update()
	{
        //UI �� ���� ������ �ƹ��͵� ���� ����
        if (!_input.IsOnUI)
            return;

        //Ŀ�� ������ ���콺�� ����ٴϰ� �����.
        cursorSlot.transform.position = Mouse.current.position.ReadValue();

        //���콺�� �� �����ӿ� �ԷµǾ��ٸ�
        if(Mouse.current.leftButton.wasPressedThisFrame)
		{
            //Ŭ�� �ڵ鷯�� ���� ��ġ�� ������ �ѱ��.
            HandleClickSlot(CheckSlot());

            //Ŀ�� ���Կ� �������� ������ �������� Ȧ�����̶� ǥ���Ѵ�.
            _input.IsHoldingCursor = cursorSlot.IsHasItem;
		}
	}

    /// <summary>
    /// Ŭ���� ��ġ�� ������ �޾Ƽ� 
    /// ���� Ŭ�� ���� ������ �ڵ鸵 ��
    /// </summary>
    /// <param name="c_Slot">Ŭ���� ���� ����</param>
    private void HandleClickSlot(UI_ItemSlot c_Slot)
	{
        //Ŭ���� ���� ������ null�̰ų�
        //Ŀ�� ���Կ� Ȧ������ �������� ����, Ŭ���� ���� ���Ե� �������� ���ٸ�
        //�׳� return;
        if (c_Slot == null
            || (!cursorSlot.IsHasItem && !c_Slot.IsHasItem))
            return;

        //Ŀ�� ���Կ� Ȧ������ �������� ����, Ŭ���� ���Կ��� ������
        
        if(!cursorSlot.IsHasItem && c_Slot.IsHasItem)
		{
            //Ŀ�� �������� Ŭ���� ������ ItemStack�� �ű��.
            cursorItemSlot.SetItemStack(c_Slot.itemSlot.TakeItemAll());

            return;
		}

        //Ŀ�� ���Կ� Ȧ������ �������� �ְ�, Ŭ���� ���Կ��� ������
        //���� ���´�.
        if(cursorSlot.IsHasItem && !c_Slot.IsHasItem)
		{
            //Ŭ���� �������� Ȧ������ �������� �ű��.
            c_Slot.itemSlot.SetItemStack(cursorItemSlot.TakeItemAll());

            return;
		}

        //Ŀ�� ���԰� Ŭ���� ����, ���ʿ� �������� �ִٸ�
        //������ �߰��ϰų� ���� �����Ѵ�.
        if(cursorSlot.IsHasItem && c_Slot.IsHasItem)
		{
            //���� �ٸ� �������̶�� �����Ѵ�.
            if(cursorSlot.itemSlot.itemStack.id != c_Slot.itemSlot.itemStack.id)
			{
                ItemStack cursorTemp = cursorSlot.itemSlot.TakeItemAll();
                ItemStack clickTemp = c_Slot.itemSlot.TakeItemAll();

                cursorSlot.itemSlot.SetItemStack(clickTemp);

                c_Slot.itemSlot.SetItemStack(cursorTemp);

                return;
			}
            //���� ���� �������̶�� ������ �߰��Ѵ�.
            //�̶� �ִ밪�� ���� �ʰ� �Կ� �����϶�
            else
			{
                byte MaxSize = world.blockTypes[cursorSlot.itemSlot.itemStack.id].MaxStackSize;

                //Ŭ�� ���Կ� ���ϰ� �ִ밪���� ��ģ ���� �ӽ� �����Ѵ�.
                int cursorAmountTemp = c_Slot.itemSlot.AddStackAmount(cursorSlot.itemSlot.itemStack.amount, MaxSize);

                //����
                if(cursorAmountTemp < 0)
				{
                    Debug.Log("Invalid amount of cursorSlot");
                    return;
				}
                //��ġ�� �ʾҴٸ� ���� ���
                else if(cursorAmountTemp == 0)
				{
                    cursorSlot.itemSlot.ClearSlot();
				}
                //���ƴٸ� Ŀ�� ������ ������ ������ ��ģ ��ŭ�� ������ �����ϰ� ����
                else
				{
                    cursorSlot.itemSlot.itemStack.amount = cursorAmountTemp;
                    cursorSlot.RefreshSlot();
				}
			}
		}
	}

    /// <summary>
    /// ���콺 ��ġ�� UI_ItemSlot ������Ʈ�� ������ �´�.
    /// </summary>
    /// <returns></returns>
    private UI_ItemSlot CheckSlot()
	{
        //�̺�Ʈ �ý��ۿ��� ������ ������ �ҷ��´�.
        pointerEventData = new PointerEventData(eventSystem);

        //���� ���콺 ��ġ�� �����Ѵ�.
        pointerEventData.position = Mouse.current.position.ReadValue();

        //���� ĳ������ �Ѵ�.
        //���� ���콺 ��ġ�� �ִ� ��� UI ��Ҹ� �����´�.
        List<RaycastResult> r = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, r);

        //���� ĳ��Ʈ ������� UI_ItemSlot�� �޾ƿ´�.
        foreach(RaycastResult result in r)
		{
            //���� �±װ� UI_ItemSlot�̶��
            if(result.gameObject.tag == "UI_ItemSlot")
			{
                //UI_ItemSlot ������Ʈ�� ��ȯ�Ѵ�.
                return result.gameObject.GetComponent<UI_ItemSlot>();
			}
		}
        //���콺 ��ġ�� UI_ItemSlot�� ������ null ����
        return null;

	}

}
