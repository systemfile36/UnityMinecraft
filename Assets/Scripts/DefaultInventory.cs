using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//이 스크립트는 DefaultInv 아래의 UI들을 관리한다.
public class DefaultInventory : MonoBehaviour
{
    /// <summary>
    /// GridInv에 대한 참조
    /// </summary>
    private GameObject gridInv;

	/// <summary>
	/// World에 대한 참조
	/// </summary>
	private World world;

	/// <summary>
	/// GridInv의 아이템 슬롯을 저장
	/// UI_ItemSlot과는 별개로, ItemSlot은 저장해놓을 필요 있음
	/// </summary>
	List<ItemSlot> gridInvSlots = new List<ItemSlot>();

	void Awake()
	{
		//World에 대한 참조 초기화
		world = GameObject.Find("World").GetComponent<World>();

		//GridInv에 대한 참조 초기화
		gridInv = transform.GetChild(0).gameObject;

		gridInvSlots.Capacity = 27;
	}

	void Start()
	{
		//테스트를 위한 아이템 슬롯들을 GridInv의 자식들에 연결한다.
		for(int i = 1; i < world.blockTypes.Length; i++)
		{
			ItemStack temp = new ItemStack((byte)i, 64);

			//아이템 슬롯을 만들어서 GridInv의 UI_ItemSlot들과 연결한다.
			ItemSlot slotTemp = 
				new ItemSlot(gridInv.transform.GetChild(i - 1).GetComponent<UI_ItemSlot>(), temp);

			//리스트에 추가
			gridInvSlots.Add(slotTemp);

		}

		//리스트에 추가된 아이템 슬롯 개수가 전체 슬롯 수보다 모자라면
		if (gridInvSlots.Count < gridInv.transform.childCount)
		{
			//빈 슬롯을 추가 해준다.
			for (int i = gridInvSlots.Count - 1; i < gridInv.transform.childCount; i++)
			{
				ItemSlot slotTemp = new ItemSlot(gridInv.transform.GetChild(i).GetComponent<UI_ItemSlot>());

				gridInvSlots.Add(slotTemp);
			}
		}

	}
}
