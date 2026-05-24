namespace LevelValuesAndCosmeticsScanner.Snapshots;

public sealed class BoxRaritySnapshot
{
    public static readonly BoxRaritySnapshot Empty = new();

    public int Common { get; set; }
    public int Uncommon { get; set; }
    public int Rare { get; set; }
    public int UltraRare { get; set; }

    public int Total => Common + Uncommon + Rare + UltraRare;
    public bool HasAny => Total > 0;

    public void Include(SemiFunc.Rarity rarity)
    {
        switch (rarity)
        {
            case SemiFunc.Rarity.Common:
                Common++;
                break;
            case SemiFunc.Rarity.Uncommon:
                Uncommon++;
                break;
            case SemiFunc.Rarity.Rare:
                Rare++;
                break;
            case SemiFunc.Rarity.UltraRare:
                UltraRare++;
                break;
        }
    }
}
