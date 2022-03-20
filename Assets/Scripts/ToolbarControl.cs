using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ToolbarControl : MonoBehaviour
{
    //월드에 대한 참조
    World world;

    StarterAssets.StarterAssetsInputs _input;

    //플레이어에 대한 참조
    public StarterAssets.FirstPersonController player;

    //선택 슬롯에 대한 변수
    public RectTransform Selected;

    [Tooltip("Delay of Scroll Interval")]
    public float scrollDelay = 0.2f;

    //아이템 슬롯 배열
    public ItemSlot[] itemSlot;

    private int slotIndex = 0;

    private float scrollTimeOut;
    
    

	void Start()
	{
        world = GameObject.Find("World").GetComponent<World>();
        _input = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssets.StarterAssetsInputs>();

        //스크롤 간격 초기화
        scrollTimeOut = scrollDelay;

        //itemSlot
        //아이템 슬롯에 대하여 반복하여 스프라이트를 넣어준다.
        //스프라이트는 World.cs의 blockTypes에 저장되어있다.
        foreach(ItemSlot slot in itemSlot)
		{
            slot.icon.sprite = world.blockTypes[slot.itemID].icon;
            slot.icon.enabled = true;
		}

        //선택 커서의 위치를 현재 인덱스의 icon 위치로 옮김
        Selected.position = itemSlot[slotIndex].icon.transform.position;
        player.HoldingBlockId = itemSlot[slotIndex].itemID;

#if !UNITY_ANDROID || !UNITY_IOS
        //키보드의 onTextInput 이벤트에 SelectItemByNumber를 연결
        Keyboard.current.onTextInput += SelectItemByNumber;
#endif
    }


    void Update()
	{


        //스크롤 입력을 감지해서
        if(_input.ScrollAxis != 0)
		{
            //스크롤 딜레이 확인
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

                //델타 타임 초기화
                scrollTimeOut = scrollDelay;
            } 
            //델타 타임 감소
            if(scrollTimeOut >= 0.0f)
			{
                scrollTimeOut -= Time.deltaTime;
			}

            //범위 체크해서 순환
            if(slotIndex > itemSlot.Length - 1)
			{
                slotIndex = 0;
			}

            if(slotIndex < 0)
			{
                slotIndex = itemSlot.Length - 1;
			}

            //선택 커서의 위치를 현재 인덱스의 icon 위치로 옮김
            Selected.position = itemSlot[slotIndex].icon.transform.position;
            player.HoldingBlockId = itemSlot[slotIndex].itemID;
		}
      
    }

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

            //커서와 플레이어의 홀딩 아이디를 바꿈
            Selected.position = itemSlot[slotIndex].icon.transform.position;
            player.HoldingBlockId = itemSlot[slotIndex].itemID;
        }
    }
#endif

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