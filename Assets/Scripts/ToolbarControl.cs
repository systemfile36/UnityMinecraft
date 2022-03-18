using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarControl : MonoBehaviour
{
    //월드에 대한 참조
    World world;

    //플레이어에 대한 참조
    public StarterAssets.FirstPersonController player;

    //선택 슬롯에 대한 변수
    public RectTransform Selected;

    //아이템 슬롯 배열
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

//인스펙터에서 관리하기 편하기 위해
[System.Serializable]
public class ItemSlot
{
    //아이템 아이디
    public byte itemID;
    //아이콘 이미지
    public Image icon;
}