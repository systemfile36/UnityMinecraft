using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ToolbarControl : MonoBehaviour
{
	[Header("Toolbar Slots")]
	public UI_ItemSlot[] toolbarSlots;

	void Start()
	{
		ItemStack stack = new ItemStack(3, 14);
		ItemSlot slot = new ItemSlot(toolbarSlots[0], stack);
	}
}