using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ToolbarControl : MonoBehaviour
{
    //���忡 ���� ����
    World world;

    StarterAssets.StarterAssetsInputs _input;

    //�÷��̾ ���� ����
    public StarterAssets.FirstPersonController player;

    //���� ���Կ� ���� ����
    public RectTransform Selected;

    [Tooltip("Delay of Scroll Interval")]
    public float scrollDelay = 0.2f;

    //������ ���� �迭
    public ItemSlot[] itemSlot;

    private int slotIndex = 0;

    private float scrollTimeOut;
    
    

	void Start()
	{
        world = GameObject.Find("World").GetComponent<World>();
        _input = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssets.StarterAssetsInputs>();

        //��ũ�� ���� �ʱ�ȭ
        scrollTimeOut = scrollDelay;

        //itemSlot
        //������ ���Կ� ���Ͽ� �ݺ��Ͽ� ��������Ʈ�� �־��ش�.
        //��������Ʈ�� World.cs�� blockTypes�� ����Ǿ��ִ�.
        foreach(ItemSlot slot in itemSlot)
		{
            slot.icon.sprite = world.blockTypes[slot.itemID].icon;
            slot.icon.enabled = true;
		}

        //���� Ŀ���� ��ġ�� ���� �ε����� icon ��ġ�� �ű�
        Selected.position = itemSlot[slotIndex].icon.transform.position;
        player.HoldingBlockId = itemSlot[slotIndex].itemID;

#if !UNITY_ANDROID || !UNITY_IOS
        //Ű������ onTextInput �̺�Ʈ�� SelectItemByNumber�� ����
        Keyboard.current.onTextInput += SelectItemByNumber;
#endif
    }


    void Update()
	{


        //��ũ�� �Է��� �����ؼ�
        if(_input.ScrollAxis != 0)
		{
            //��ũ�� ������ Ȯ��
            if (scrollTimeOut <= 0.0f)
			{
                if(_input.ScrollAxis > 0)
				{
                    slotIndex--;
				}
                if(_input.ScrollAxis < 0)
				{
                    slotIndex++;
				}

                //��Ÿ Ÿ�� �ʱ�ȭ
                scrollTimeOut = scrollDelay;
            } 
            //��Ÿ Ÿ�� ����
            if(scrollTimeOut >= 0.0f)
			{
                scrollTimeOut -= Time.deltaTime;
			}

            //���� üũ�ؼ� ��ȯ
            if(slotIndex > itemSlot.Length - 1)
			{
                slotIndex = 0;
			}

            if(slotIndex < 0)
			{
                slotIndex = itemSlot.Length - 1;
			}

            //���� Ŀ���� ��ġ�� ���� �ε����� icon ��ġ�� �ű�
            Selected.position = itemSlot[slotIndex].icon.transform.position;
            player.HoldingBlockId = itemSlot[slotIndex].itemID;
		}
      
    }

#if !UNITY_ANDROID || !UNITY_IOS
    //onTextInput�� �̺�Ʈ�� ����
    void SelectItemByNumber(char c)
    {
        //char�� ���ڷ� ��ȯ(0�� �ƽ�Ű �ڵ�� 48�̹Ƿ�)
        int index = c - 48;

        //���� üũ
        if (index > 0 && index < 10)
        {
            //�ε����� ���߾� ��
            slotIndex = index - 1;

            //Ŀ���� �÷��̾��� Ȧ�� ���̵� �ٲ�
            Selected.position = itemSlot[slotIndex].icon.transform.position;
            player.HoldingBlockId = itemSlot[slotIndex].itemID;
        }
    }
#endif

}

//�ν����Ϳ��� �����ϱ� ���ϱ� ����
[System.Serializable]
public class ItemSlot
{
    //������ ���̵�
    public byte itemID;
    //������ �̹���
    public Image icon;
}