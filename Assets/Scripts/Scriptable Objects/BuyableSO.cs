using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuyableItem;

[CreateAssetMenu(fileName = "Buyable", menuName = "Buyable")]
public class BuyableSO : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private DifficultyInt cost;
    [SerializeField] private int testCost;
    [SerializeField] private Buyable type;
    [SerializeField] private Sprite icon;
    [TextArea(1, 3), SerializeField] private string description;

    public int GetCost(bool useDifficulty, Difficulty difficulty = Difficulty.Easy) 
    {
        if (useDifficulty) 
        {
            return difficulty switch 
            {
                Difficulty.Easy => cost.EasyValue,
                Difficulty.Medium => cost.MediumValue,
                Difficulty.Hard => cost.HardValue,
                _ => throw new NotImplementedException(),
            };
        } 
        else return testCost;
    }

    public Buyable Type => type;
    public Sprite Icon => icon;
    public string Description => description;
}
