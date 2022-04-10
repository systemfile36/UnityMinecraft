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
	/// UnderBar�� ���� ����
	/// </summary>
	private GameObject underBar;

	/// <summary>
	/// ToolbarControl�� ���� ����
	/// </summary>
	private ToolbarControl toolbar;

	/// <summary>
	/// World�� ���� ����
	/// </summary>
	private World world;

	/// <summary>
	/// GridInv�� ������ ������ ����
	/// UI_ItemSlot���� ������, ItemSlot�� �����س��� �ʿ� ����
	/// </summary>
	List<ItemSlot> gridInvSlots = new List<ItemSlot>();

	private bool IsToolbarInit = false;

	void Awake()
	{
		//World�� ���� ���� �ʱ�ȭ
		world = GameObject.Find("World").GetComponent<World>();

		//GridInv�� ���� ���� �ʱ�ȭ
		gridInv = transform.GetChild(0).gameObject;

		//UnserBar�� ���� ���� �ʱ�ȭ
		underBar = transform.GetChild(1).gameObject;

		//���ٿ� ���� ����
		toolbar = GameObject.Find("Toolbar").GetComponent<ToolbarControl>();

		gridInvSlots.Capacity = 27;
	}

	void Start()
	{

		#region GridInv �ʱ�ȭ
		//�׽�Ʈ�� ���� ������ ���Ե��� GridInv�� �ڽĵ鿡 �����Ѵ�.
		for (int i = 1; i < world.blockTypes.Length; i++)
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
		#endregion

	}

	void Update()
	{
		
	}

	//DefaultInv�� Ȱ��ȭ �Ǹ� ���� ��
	void OnEnable()
	{
		//�ڷ�ƾ �����Ͽ� �� ������ ��, ItemSlot ��ũ
		StartCoroutine(ToolbarItemSlotLink());
		toolbar.gameObject.SetActive(false);
	}

	
	//DefaultInv�� ��Ȱ��ȭ �Ǹ� ���� ��
	void OnDisable()
	{
		//�ٽ� toolbar�� UI_ItemSlot�� ��ũ���ش�.
		for(int i = 0; i < 9; i++)
		{
			ItemSlot temp = toolbar.toolbarItemSlots[i];
			toolbar.toolbarSlots[i].Link(temp);
		}
		toolbar.gameObject.SetActive(true);
	}


	/// <summary>
	/// toolbar�� ItemSlot�� UnderBar�� Link�Ѵ�.
	/// Ȱ��ȭ Ÿ�̹� ������ ���� �������� �����Ѵ�.
	/// </summary>
	/// <returns></returns>
	IEnumerator ToolbarItemSlotLink()
	{
		//�������� �����Ѵ�.
		yield return null;

		//������ UI_ItemSlot�� ����� ItemSlot�� ��ũ�� �����ϰ�
		//UnderBar�� �ڽĵ�� �����Ѵ�.
		for (int i = 0; i < 9; i++)
		{
			ItemSlot temp = toolbar.toolbarItemSlots[i];
			UI_ItemSlot UI_temp = underBar.transform.GetChild(i).GetComponent<UI_ItemSlot>();
			UI_temp.Link(temp);

		}
	}
}
