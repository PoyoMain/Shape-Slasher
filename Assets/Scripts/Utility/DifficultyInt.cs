using System;

[Serializable]
public struct DifficultyInt
{
    public int EasyValue;
    public int MediumValue;
    public int HardValue;

    public readonly int ReturnValue(Difficulty value)
    {
        return value switch
        {
            Difficulty.Easy => EasyValue,
            Difficulty.Medium => MediumValue,
            Difficulty.Hard => HardValue,
            _ => throw new System.NotImplementedException(),
        };
    }
}