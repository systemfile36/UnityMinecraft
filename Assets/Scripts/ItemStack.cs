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
    

}
