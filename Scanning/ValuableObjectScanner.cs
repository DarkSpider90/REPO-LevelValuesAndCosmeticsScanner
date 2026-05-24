using System.Collections.Generic;
using LevelValuesAndCosmeticsScanner.GameInterop;
using LevelValuesAndCosmeticsScanner.Snapshots;
using UnityEngine;

namespace LevelValuesAndCosmeticsScanner.Scanning;

public sealed class ValuableObjectScanner
{
    private const float RefreshDelay = 0.25f;
    private float _nextRefreshTime;
    private LevelValuablesSnapshot _latest = LevelValuablesSnapshot.Empty;

    public void Reset()
    {
        _latest = LevelValuablesSnapshot.Empty;
        _nextRefreshTime = 0f;
    }

    public LevelValuablesSnapshot Read(bool force = false)
    {
        if (!force && Time.unscaledTime < _nextRefreshTime)
            return _latest;

        _nextRefreshTime = Time.unscaledTime + RefreshDelay;
        _latest = BuildSnapshot();
        return _latest;
    }

    private static LevelValuablesSnapshot BuildSnapshot()
    {
        var valuableDirector = ValuableDirector.instance;
        var roundDirector = RoundDirector.instance;

        if (valuableDirector == null || roundDirector == null)
            return LevelValuablesSnapshot.Empty;

        var extractedObjects = roundDirector.dollarHaulList;
        var looseCount = 0;
        var looseValue = 0f;

        foreach (var valuable in valuableDirector.valuableList)
        {
            if (!ShouldCount(valuable, extractedObjects))
                continue;

            looseCount++;
            looseValue += Mathf.Max(0f, GameFieldReader.CurrentValue(valuable));
        }

        return new LevelValuablesSnapshot(looseCount, looseValue);
    }

    private static bool ShouldCount(ValuableObject valuable, List<GameObject> extractedObjects)
    {
        if (valuable == null || valuable.gameObject == null)
            return false;

        if (!valuable.isActiveAndEnabled || !valuable.gameObject.activeInHierarchy)
            return false;

        if (!GameFieldReader.ValueWasAssigned(valuable))
            return false;

        return extractedObjects == null || !extractedObjects.Contains(valuable.gameObject);
    }
}
