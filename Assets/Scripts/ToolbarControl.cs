using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarControl : MonoBehaviour
{
    //���忡 ���� ����
    World world;

    //�÷��̾ ���� ����
    public StarterAssets.FirstPersonController player;

    //���� ���Կ� ���� ����
    public RectTransform Selected;

    //������ ���� �迭
    public ItemSlot[] itemSlot;

    private int slotIndex = 0;

	void Start()
	{
        world = GameObject.Find("World").GetComponent<World>();

        //itemSlot
        foreach(ItemSlot slot in itemSlot)
		{
            slot.icon.sprite = world.blockTypes[slot.itemID].icon;
            slot.icon.enabled = true;
		}
	}

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