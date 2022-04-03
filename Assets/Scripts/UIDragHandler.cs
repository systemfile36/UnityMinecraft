using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class UIDragHandler : MonoBehaviour
{
    //[SerializeField]�� �ν����Ϳ��� ���� ����

    [SerializeField] private UI_ItemSlot cursorSlot = null;

    private ItemSlot cursorItemSlot = null;

    //UI ��ҿ� ���� ����ĳ����
    [SerializeField] private GraphicRaycaster raycaster = null;

    //������ ��ġ�� ���� ����
    private PointerEventData pointerEventData = null;

    
    //������Ʈ�� �̺�Ʈ �ý���
    [SerializeField] private EventSystem eventSystem = null;

    //world�� ���� ����
    //World world;

    //Player�� ����� ��ũ��Ʈ ����
    StarterAssets.StarterAssetsInputs _input;

	void Start()
	{
        //world = GameObject.Find("World").GetComponent<World>();
        _input = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssets.StarterAssetsInputs>();
        cursorItemSlot = new ItemSlot(cursorSlot);
	}

	void FixedUpdate()
	{
        //UI �� ���� ������ �ƹ��͵� ���� ����
        if (!_input.IsOnUI)
            return;

        //���콺�� �� �����ӿ� �ԷµǾ��ٸ�
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
