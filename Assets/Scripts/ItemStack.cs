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
    /// 복사를 위한 복사 생성자
    /// </summary>
    /// <param name="o"></param>
    public ItemStack(ItemStack o)
	{
        id = o.id;
        amount = o.amount;
	}
}
