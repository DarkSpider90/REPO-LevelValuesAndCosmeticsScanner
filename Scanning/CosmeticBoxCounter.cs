using LevelValuesAndCosmeticsScanner.Snapshots;
using UnityEngine;

namespace LevelValuesAndCosmeticsScanner.Scanning;

public sealed class CosmeticBoxCounter
{
    private const float RefreshDelay = 0.5f;
    private float _nextRefreshTime;
    private BoxRaritySnapshot _latest = BoxRaritySnapshot.Empty;

    public void Reset()
    {
        _latest = BoxRaritySnapshot.Empty;
        _nextRefreshTime = 0f;
    }

    public BoxRaritySnapshot Read(bool force = false)
    {
        if (!force && Time.unscaledTime < _nextRefreshTime)
            return _latest;

        _nextRefreshTime = Time.unscaledTime + RefreshDelay;
        _latest = BuildSnapshot();
        return _latest;
    }

    private static BoxRaritySnapshot BuildSnapshot()
    {
        var roundDirector = RoundDirector.instance;
        if (roundDirector == null)
            return BoxRaritySnapshot.Empty;

        var snapshot = new BoxRaritySnapshot();
        foreach (var box in roundDirector.cosmeticWorldObjects)
        {
            if (box == null || box.gameObject == null)
                continue;

            if (!box.isActiveAndEnabled || !box.gameObject.activeInHierarchy)
                continue;

            snapshot.Include(box.rarity);
        }

        return snapshot;
    }
}
