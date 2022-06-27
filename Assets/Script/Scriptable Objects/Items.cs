using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType{PlayerItem, EnemyItem}

public enum Item{HPpotion,SPpotion, Elexir, PhoenixFeather, Bombs, Poison  }
public class Items : ScriptableObject
{
    [Header("Item description")] [SerializeField]
    private string itemName;
    private string description;
    private ItemType type;
    private Item item;

    [Header("Item Effects")] [SerializeField]
    private int effectAmount;
    private int roundTimer;

    public ItemType GetType()
    {
        return type;
    }

    public string GetName()
    {
        return itemName;
    }

    public void UseItem(CharacterInformation target)
    {
        switch (item)
        {
            case Item.HPpotion:
                target.HealHP(effectAmount);
                break;
            case Item.SPpotion:
                target.RegenSP(effectAmount);
                break;
            case Item.Elexir:
                target.HealHP(effectAmount*3);
                target.RegenSP(effectAmount);
                break;
            case Item.PhoenixFeather:
                target.Revive();
                break;
            case Item.Bombs:
                //bombing the shit out of ya
                break;
            case Item.Poison:
                // I aint no bitchpussy
                break;
        }
    }
}
