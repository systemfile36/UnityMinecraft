using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�� ��ũ��Ʈ�� DefaultInv �Ʒ��� UI���� �����Ѵ�.
public class DefaultInventory : MonoBehaviour
{
    /// <summary>
    /// GridInv�� ���� ����
    /// </summary>
    private GameObject gridInv;

	/// <summary>
	/// World�� ���� ����
	/// </summary>
	private World world;

	/// <summary>
	/// GridInv�� ������ ������ ����
	/// UI_ItemSlot���� ������, ItemSlot�� �����س��� �ʿ� ����
	/// </summary>
	List<ItemSlot> gridInvSlots = new List<ItemSlot>();

	void Awake()
	{
		//World�� ���� ���� �ʱ�ȭ
		world = GameObject.Find("World").GetComponent<World>();

		//GridInv�� ���� ���� �ʱ�ȭ
		gridInv = transform.GetChild(0).gameObject;

		gridInvSlots.Capacity = 27;
	}

	void Start()
	{
		//�׽�Ʈ�� ���� ������ ���Ե��� GridInv�� �ڽĵ鿡 �����Ѵ�.
		for(int i = 1; i < world.blockTypes.Length; i++)
		{
			ItemStack temp = new ItemStack((byte)i, 64);

			//������ ������ ���� GridInv�� UI_ItemSlot��� �����Ѵ�.
			ItemSlot slotTemp = 
				new ItemSlot(gridInv.transform.GetChild(i - 1).GetComponent<UI_ItemSlot>(), temp);

			//����Ʈ�� �߰�
			gridInvSlots.Add(slotTemp);

		}

		//����Ʈ�� �߰��� ������ ���� ������ ��ü ���� ������ ���ڶ��
		if (gridInvSlots.Count < gridInv.transform.childCount)
		{
			//�� ������ �߰� ���ش�.
			for (int i = gridInvSlots.Count - 1; i < gridInv.transform.childCount; i++)
			{
				ItemSlot slotTemp = new ItemSlot(gridInv.transform.GetChild(i).GetComponent<UI_ItemSlot>());

				gridInvSlots.Add(slotTemp);
			}
		}

	}
}
