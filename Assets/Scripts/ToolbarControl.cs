using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ToolbarControl : MonoBehaviour
{
	[Header("Toolbar Slots")]
	public UI_ItemSlot[] toolbarSlots;

	[Header("Delay of Scroll Interval")]
	public float scrollDelay = 0.2f; //스크롤 간격 제한
	
	[Header("Reference of Selected")]
	public RectTransform Selected;

	//입력을 위한
	StarterAssets.StarterAssetsInputs _input;

	[Header("Reference of Player")]
	public StarterAssets.FirstPersonController player;

	private float scrollTimeOut;


	//선택중인 블럭, 디폴트 == 0
	private int slotIndex = 0;


	[Header("Reference of DefaultInv_UnderBar")]
	public Transform underBar;

	/// <summary>
	/// Toolbar의 UI_ItemSlot들과 연결된 ItemSlot들의 리스트
	/// </summary>
	public List<ItemSlot> toolbarItemSlots = new List<ItemSlot>(9);

	void Awake()
	{
		_input = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssets.StarterAssetsInputs>();

	}

	void Start()
	{

		//스크롤 간격 변수 초기화
		scrollTimeOut = scrollDelay;

		//툴바에 초기 아이템 설정
		foreach(UI_ItemSlot uSlot in toolbarSlots)
		{
			ItemStack stack = new ItemStack(2, Random.Range(2, 65));

			ItemSlot slot = new ItemSlot(uSlot, stack);

			//아이템 슬롯 목록에 추가
			toolbarItemSlots.Add(slot);
		}


		//모바일 환경이 아닐때에만
#if !UNITY_ANDROID || !UNITY_IOS
		//키보드의 onTextInput 이벤트에 SelectItemByNumber를 연결
		Keyboard.current.onTextInput += SelectItemByNumber;
#endif

		//블럭 선택 갱신
		RefreshSelected();

	}

	void Update()
	{
		//스크롤 입력을 감지해서
		if (_input.ScrollAxis != 0)
		{
			//스크롤 딜레이 확인
			if (scrollTimeOut <= 0.0f)
			{
				if (_input.ScrollAxis > 0)
				{
					slotIndex--;
				}
				if (_input.ScrollAxis < 0)
				{
					slotIndex++;
				}

				//델타 타임 초기화
				scrollTimeOut = scrollDelay;
			}
		}

		//델타 타임 감소
		if (scrollTimeOut >= 0.0f)
		{
			scrollTimeOut -= Time.deltaTime;
		}

		//범위 체크해서 순환
		if (slotIndex > toolbarSlots.Length - 1)
		{
			slotIndex = 0;
		}

		if (slotIndex < 0)
		{
			slotIndex = toolbarSlots.Length - 1;
		}

		//블럭 선택 갱신
		RefreshSelected();

	}

	//파괴될 때 이벤트를 해제 해주어야 한다.
    void OnDestroy()
    {
		//이벤트 해제
        Keyboard.current.onTextInput -= SelectItemByNumber;
    }

    //모바일 환경이 아닐때에만
#if !UNITY_ANDROID || !UNITY_IOS
    //onTextInput의 이벤트로 연결
    void SelectItemByNumber(char c)
	{
		//char을 숫자로 변환(0의 아스키 코드는 48이므로)
		int index = c - 48;

		//범위 체크
		if (index > 0 && index < 10)
		{
			//인덱스를 맞추어 줌
			slotIndex = index - 1;

			//블럭 선택 갱신
			RefreshSelected();
		}
	}
#endif

	/// <summary>
	/// 플레이어의 HoldingBlockId와 선택 가이드라인 위치 갱신
	/// slotIndex 참조함
	/// </summary>
	public void RefreshSelected()
	{
		//선택한 블럭으로 가이드라인 이동, 해당 UI의 아이콘 위치 참조
		Selected.position = toolbarSlots[slotIndex].sIcon.transform.position;
	}

	/// <summary>
	/// 선택된 슬롯의 ItemSlot 반환
	/// </summary>
	public ItemSlot SelectedItemSlot
	{
		get
		{
			if (toolbarSlots[slotIndex].itemSlot != null)
				return toolbarSlots[slotIndex].itemSlot;
			else
				return null;
		}
	}

	
	
}