namespace LevelValuesAndCosmeticsScanner.Snapshots;

public readonly struct LevelValuablesSnapshot
{
    public static readonly LevelValuablesSnapshot Empty = new(0, 0f);

    public readonly int LooseItemCount;
    public readonly float LooseItemValue;

    public bool HasLooseItems => LooseItemCount > 0;

    public LevelValuablesSnapshot(int looseItemCount, float looseItemValue)
    {
        LooseItemCount = looseItemCount;
        LooseItemValue = looseItemValue;
    }
}
