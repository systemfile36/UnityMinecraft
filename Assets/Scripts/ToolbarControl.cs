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
	public float scrollDelay = 0.2f; //��ũ�� ���� ����
	
	[Header("Reference of Selected")]
	public RectTransform Selected;

	//�Է��� ����
	StarterAssets.StarterAssetsInputs _input;

	[Header("Reference of Player")]
	public StarterAssets.FirstPersonController player;

	private float scrollTimeOut;


	//�������� ��, ����Ʈ == 0
	private int slotIndex = 0;


	[Header("Reference of DefaultInv_UnderBar")]
	public Transform underBar;

	/// <summary>
	/// Toolbar�� UI_ItemSlot��� ����� ItemSlot���� ����Ʈ
	/// </summary>
	public List<ItemSlot> toolbarItemSlots = new List<ItemSlot>(9);

	void Awake()
	{
		_input = GameObject.FindGameObjectWithTag("Player").GetComponent<StarterAssets.StarterAssetsInputs>();

	}

	void Start()
	{

		//��ũ�� ���� ���� �ʱ�ȭ
		scrollTimeOut = scrollDelay;

		//���ٿ� �ʱ� ������ ����
		foreach(UI_ItemSlot uSlot in toolbarSlots)
		{
			ItemStack stack = new ItemStack(2, Random.Range(2, 65));

			ItemSlot slot = new ItemSlot(uSlot, stack);

			//������ ���� ��Ͽ� �߰�
			toolbarItemSlots.Add(slot);
		}


		//����� ȯ���� �ƴҶ�����
#if !UNITY_ANDROID || !UNITY_IOS
		//Ű������ onTextInput �̺�Ʈ�� SelectItemByNumber�� ����
		Keyboard.current.onTextInput += SelectItemByNumber;
#endif

		//�� ���� ����
		RefreshSelected();

	}

	void Update()
	{
		//��ũ�� �Է��� �����ؼ�
		if (_input.ScrollAxis != 0)
		{
			//��ũ�� ������ Ȯ��
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

				//��Ÿ Ÿ�� �ʱ�ȭ
				scrollTimeOut = scrollDelay;
			}
		}

		//��Ÿ Ÿ�� ����
		if (scrollTimeOut >= 0.0f)
		{
			scrollTimeOut -= Time.deltaTime;
		}

		//���� üũ�ؼ� ��ȯ
		if (slotIndex > toolbarSlots.Length - 1)
		{
			slotIndex = 0;
		}

		if (slotIndex < 0)
		{
			slotIndex = toolbarSlots.Length - 1;
		}

		//�� ���� ����
		RefreshSelected();

	}

	//�ı��� �� �̺�Ʈ�� ���� ���־�� �Ѵ�.
    void OnDestroy()
    {
		//�̺�Ʈ ����
        Keyboard.current.onTextInput -= SelectItemByNumber;
    }

    //����� ȯ���� �ƴҶ�����
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

			//�� ���� ����
			RefreshSelected();
		}
	}
#endif

	/// <summary>
	/// �÷��̾��� HoldingBlockId�� ���� ���̵���� ��ġ ����
	/// slotIndex ������
	/// </summary>
	public void RefreshSelected()
	{
		//������ ������ ���̵���� �̵�, �ش� UI�� ������ ��ġ ����
		Selected.position = toolbarSlots[slotIndex].sIcon.transform.position;
	}

	/// <summary>
	/// ���õ� ������ ItemSlot ��ȯ
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