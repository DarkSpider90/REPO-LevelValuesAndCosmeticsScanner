using LevelValuesAndCosmeticsScanner.GameInterop;
using LevelValuesAndCosmeticsScanner.Hud;
using LevelValuesAndCosmeticsScanner.Scanning;
using LevelValuesAndCosmeticsScanner.Settings;
using LevelValuesAndCosmeticsScanner.Snapshots;
using UnityEngine;

namespace LevelValuesAndCosmeticsScanner.Runtime;

public sealed class LevelScannerRuntime : MonoBehaviour
{
    private readonly ValuableObjectScanner _valuableScanner = new();
    private readonly CosmeticBoxCounter _cosmeticCounter = new();
    private bool _levelReady;

    private void Update()
    {
        if (!IsLevelReady())
        {
            LeaveLevelIfNeeded();
            return;
        }

        EnterLevelIfNeeded();
        RefreshHud();
    }

    private void RefreshHud()
    {
        if (RoundDirector.instance == null)
        {
            HudOverlay.Instance.Hide();
            return;
        }

        var hudRequested = ModSettings.AlwaysOn.Value || IsMapVisible();

        var valuables = _valuableScanner.Read();
        var boxes = ReadCosmeticBoxes();

        if (GameFieldReader.ExtractionIsFinished(RoundDirector.instance) || !hudRequested || NothingToShow(valuables, boxes))
        {
            HudOverlay.Instance.Hide();
            return;
        }

        HudOverlay.Instance.Show(
            BuildValueText(valuables),
            boxes,
            ModSettings.ShowCosmeticBoxes.Value,
            ModSettings.UIPosition.Value
        );
    }

    private void EnterLevelIfNeeded()
    {
        if (_levelReady)
            return;

        _levelReady = true;
        _valuableScanner.Reset();
        _cosmeticCounter.Reset();
        _valuableScanner.Read(force: true);
        if (ModSettings.ShowCosmeticBoxes.Value)
            _cosmeticCounter.Read(force: true);
        ModEntryPoint.Log.LogDebug("Level scan session started.");
    }

    private void LeaveLevelIfNeeded()
    {
        if (!_levelReady)
            return;

        _levelReady = false;
        _valuableScanner.Reset();
        _cosmeticCounter.Reset();
        HudOverlay.Instance.Hide();
        ModEntryPoint.Log.LogDebug("Level scan session cleared.");
    }

    private static bool IsLevelReady()
    {
        return SemiFunc.RunIsLevel()
               && LevelGenerator.Instance != null
               && LevelGenerator.Instance.Generated;
    }

    private static bool IsMapVisible()
    {
        if (SemiFunc.InputHold(InputKey.Map))
            return true;

        var map = MapToolController.instance;
        return map != null && GameFieldReader.LocalMapIsOpen(map);
    }

    private static bool NothingToShow(LevelValuablesSnapshot valuables, BoxRaritySnapshot boxes)
    {
        return !valuables.HasLooseItems && !boxes.HasAny;
    }

    private BoxRaritySnapshot ReadCosmeticBoxes()
    {
        if (ModSettings.ShowCosmeticBoxes.Value)
            return _cosmeticCounter.Read();

        _cosmeticCounter.Reset();
        return BoxRaritySnapshot.Empty;
    }

    private static string BuildValueText(LevelValuablesSnapshot valuables)
    {
        if (!valuables.HasLooseItems)
            return string.Empty;

        var showValue = ModSettings.ShowTotalValue.Value;
        var showCount = ModSettings.ShowItemCount.Value;

        if (showValue && showCount)
            return $"${valuables.LooseItemValue:N0}  x{valuables.LooseItemCount}";

        if (showValue)
            return $"${valuables.LooseItemValue:N0}";

        return showCount ? $"x{valuables.LooseItemCount}" : string.Empty;
    }
}
