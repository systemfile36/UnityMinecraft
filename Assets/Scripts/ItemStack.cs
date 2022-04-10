using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
    public byte id;
    public int amount;

    public ItemStack(byte id, int amount)
	{
        this.id = id;
        this.amount = amount;
	}
    
    /// <summary>
    /// ���縦 ���� ���� ������
    /// </summary>
    /// <param name="o"></param>
    public ItemStack(ItemStack o)
	{
        id = o.id;
        amount = o.amount;
	}
}
