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
	/// UnderBar에 대한 참조
	/// </summary>
	private GameObject underBar;

	/// <summary>
	/// ToolbarControl에 대한 참조
	/// </summary>
	private ToolbarControl toolbar;

	/// <summary>
	/// World에 대한 참조
	/// </summary>
	private World world;

	/// <summary>
	/// GridInv의 아이템 슬롯을 저장
	/// UI_ItemSlot과는 별개로, ItemSlot은 저장해놓을 필요 있음
	/// </summary>
	List<ItemSlot> gridInvSlots = new List<ItemSlot>();

	private bool IsToolbarInit = false;

	void Awake()
	{
		//World에 대한 참조 초기화
		world = GameObject.Find("World").GetComponent<World>();

		//GridInv에 대한 참조 초기화
		gridInv = transform.GetChild(0).gameObject;

		//UnserBar에 대한 참조 초기화
		underBar = transform.GetChild(1).gameObject;

		//툴바에 대한 참조
		toolbar = GameObject.Find("Toolbar").GetComponent<ToolbarControl>();

		gridInvSlots.Capacity = 27;
	}

	void Start()
	{

		#region GridInv 초기화
		//테스트를 위한 아이템 슬롯들을 GridInv의 자식들에 연결한다.
		for (int i = 1; i < world.blockTypes.Length; i++)
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
		#endregion

	}

	void Update()
	{
		
	}

	//DefaultInv가 활성화 되면 툴바 끔
	void OnEnable()
	{
		//코루틴 실행하여 한 프레임 뒤, ItemSlot 링크
		StartCoroutine(ToolbarItemSlotLink());
		toolbar.gameObject.SetActive(false);
	}

	
	//DefaultInv가 비활성화 되면 툴바 켬
	void OnDisable()
	{
		//다시 toolbar의 UI_ItemSlot에 링크해준다.
		for(int i = 0; i < 9; i++)
		{
			ItemSlot temp = toolbar.toolbarItemSlots[i];
			toolbar.toolbarSlots[i].Link(temp);
		}
		toolbar.gameObject.SetActive(true);
	}


	/// <summary>
	/// toolbar의 ItemSlot을 UnderBar로 Link한다.
	/// 활성화 타이밍 조절을 위해 한프레임 지연한다.
	/// </summary>
	/// <returns></returns>
	IEnumerator ToolbarItemSlotLink()
	{
		//한프레임 지연한다.
		yield return null;

		//툴바의 UI_ItemSlot에 연결된 ItemSlot의 링크를 해제하고
		//UnderBar의 자식들로 변경한다.
		for (int i = 0; i < 9; i++)
		{
			ItemSlot temp = toolbar.toolbarItemSlots[i];
			UI_ItemSlot UI_temp = underBar.transform.GetChild(i).GetComponent<UI_ItemSlot>();
			UI_temp.Link(temp);

		}
	}
}
