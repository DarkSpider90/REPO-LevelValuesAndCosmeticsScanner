using System.Collections.Generic;
using LevelValuesAndCosmeticsScanner.Settings;
using LevelValuesAndCosmeticsScanner.Snapshots;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LevelValuesAndCosmeticsScanner.Hud;

public sealed class HudOverlay
{
    private const float RowHeight = 24f;
    private const float ValueLineHeight = 28f;
    private const float ValueGap = 8f;
    private const float DefaultRightPadding = 10f;
    private const float DefaultBottomPadding = 8f;
    private static readonly Vector2 ValueTextSize = new(210f, ValueLineHeight);
    private static readonly Vector2 RowSize = new(42f, 22f);
    private static HudOverlay _instance;

    private readonly Dictionary<SemiFunc.Rarity, RarityLine> _rarityLines = new();
    private GameObject _root;
    private RectTransform _rootRect;
    private TextMeshProUGUI _valueText;
    private TMP_Text _fontSource;
    private Vector2 _contentOffset;

    public static HudOverlay Instance => _instance ??= new HudOverlay();

    public void Show(
        string valueText,
        BoxRaritySnapshot boxes,
        bool showBoxes,
        HudAnchor anchor
    )
    {
        if (!EnsureCreated())
            return;

        ApplyPosition(anchor);
        RefreshVanillaFont();
        UpdateBoxRows(boxes, showBoxes);
        UpdateValueText(valueText);
        _root.SetActive(true);
    }

    public void Hide()
    {
        if (_root != null)
            _root.SetActive(false);
    }

    private bool EnsureCreated()
    {
        if (_root != null)
            return true;

        var hud = GameObject.Find("Game Hud");
        if (hud == null)
            return false;

        _root = new GameObject("LevelValuesAndCosmeticsScanner HUD");
        _root.transform.SetParent(hud.transform, false);
        _root.SetActive(false);

        _rootRect = _root.AddComponent<RectTransform>();
        StretchToHud(_rootRect);

        var group = _root.AddComponent<CanvasGroup>();
        group.alpha = 0.95f;
        group.blocksRaycasts = false;
        group.interactable = false;

        _fontSource = ResolveFontSource();
        var font = _fontSource?.font ?? TMP_Settings.defaultFontAsset;
        _valueText = CreateValueText(_root.transform, font, _fontSource);

        AddRarityLine(SemiFunc.Rarity.Common, font, _fontSource);
        AddRarityLine(SemiFunc.Rarity.Uncommon, font, _fontSource);
        AddRarityLine(SemiFunc.Rarity.Rare, font, _fontSource);
        AddRarityLine(SemiFunc.Rarity.UltraRare, font, _fontSource);

        return true;
    }

    private void ApplyPosition(HudAnchor anchor)
    {
        StretchToHud(_rootRect);
        _contentOffset = OffsetFor(anchor);
    }

    private static Vector2 OffsetFor(HudAnchor anchor)
    {
        return anchor switch
        {
            HudAnchor.Right => new Vector2(-DefaultRightPadding, 125f),
            _ => new Vector2(-DefaultRightPadding, DefaultBottomPadding)
        };
    }

    private void UpdateValueText(string text)
    {
        var hasText = text.Length > 0;
        _valueText.gameObject.SetActive(hasText);
        if (!hasText)
            return;

        _valueText.rectTransform.anchoredPosition = _contentOffset;
        _valueText.SetText(text);
    }

    private void UpdateBoxRows(BoxRaritySnapshot boxes, bool showBoxes)
    {
        var visibleCount = VisibleRarityCount(boxes, showBoxes);
        var rowIndex = 0;

        rowIndex = PlaceRarityLine(SemiFunc.Rarity.Common, boxes.Common, showBoxes, visibleCount, rowIndex);
        rowIndex = PlaceRarityLine(SemiFunc.Rarity.Uncommon, boxes.Uncommon, showBoxes, visibleCount, rowIndex);
        rowIndex = PlaceRarityLine(SemiFunc.Rarity.Rare, boxes.Rare, showBoxes, visibleCount, rowIndex);
        PlaceRarityLine(SemiFunc.Rarity.UltraRare, boxes.UltraRare, showBoxes, visibleCount, rowIndex);
    }

    private int PlaceRarityLine(SemiFunc.Rarity rarity, int count, bool showBoxes, int visibleCount, int rowIndex)
    {
        var line = _rarityLines[rarity];
        var visible = showBoxes && count > 0;

        line.Root.SetActive(visible);
        if (!visible)
            return rowIndex;

        line.Count.SetText(count.ToString());
        var rowFromBottom = visibleCount - rowIndex - 1;
        var y = ValueLineHeight + ValueGap + rowFromBottom * RowHeight + RowSize.y * 0.5f;
        line.Rect.anchoredPosition = new Vector2(_contentOffset.x, _contentOffset.y + y);
        return rowIndex + 1;
    }

    private static int VisibleRarityCount(BoxRaritySnapshot boxes, bool showBoxes)
    {
        if (!showBoxes)
            return 0;

        var count = 0;
        if (boxes.Common > 0)
            count++;
        if (boxes.Uncommon > 0)
            count++;
        if (boxes.Rare > 0)
            count++;
        if (boxes.UltraRare > 0)
            count++;

        return count;
    }

    private void AddRarityLine(SemiFunc.Rarity rarity, TMP_FontAsset font, TMP_Text fontSource)
    {
        var root = new GameObject($"{rarity} Box Counter");
        root.transform.SetParent(_root.transform, false);

        var rect = root.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.sizeDelta = RowSize;

        var icon = new GameObject("Icon");
        icon.transform.SetParent(root.transform, false);

        var iconRect = icon.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(0f, 0f);
        iconRect.sizeDelta = new Vector2(20f, 20f);

        var image = icon.AddComponent<RawImage>();
        image.texture = RarityIconPainter.GetIconTexture(rarity);
        image.raycastTarget = false;

        var count = new GameObject("Count");
        count.transform.SetParent(root.transform, false);

        var text = count.AddComponent<TextMeshProUGUI>();
        text.font = font;
        ApplySharedFontMaterial(text, fontSource);
        text.fontSize = 15f;
        text.color = new Color(0.82f, 0.96f, 0.93f, 0.95f);
        text.alignment = TextAlignmentOptions.Right;
        text.enableWordWrapping = false;
        text.raycastTarget = false;

        var textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, -2f);
        textRect.offsetMax = Vector2.zero;

        _rarityLines[rarity] = new RarityLine(root, rect, text);
    }

    private static TextMeshProUGUI CreateValueText(Transform parent, TMP_FontAsset font, TMP_Text fontSource)
    {
        var valueObject = new GameObject("Loose Loot Summary");
        valueObject.transform.SetParent(parent, false);

        var text = valueObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        ApplySharedFontMaterial(text, fontSource);
        text.fontSize = 20f;
        text.color = new Color(0.82f, 0.96f, 0.93f, 1f);
        text.alignment = TextAlignmentOptions.Right;
        text.horizontalAlignment = HorizontalAlignmentOptions.Right;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        text.enableWordWrapping = false;
        text.raycastTarget = false;

        var rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = ValueTextSize;

        return text;
    }

    private static TMP_Text ResolveFontSource()
    {
        var haulText = GameObject.Find("Tax Haul")?.GetComponent<TMP_Text>();
        if (haulText != null && haulText.font != null)
            return haulText;

        if (HaulUI.instance != null)
        {
            haulText = HaulUI.instance.GetComponent<TMP_Text>();
            if (haulText != null && haulText.font != null)
                return haulText;
        }

        return null;
    }

    private void RefreshVanillaFont()
    {
        if (_fontSource != null && _fontSource.font != null)
            return;

        _fontSource = ResolveFontSource();
        if (_fontSource == null || _fontSource.font == null)
            return;

        ApplyFont(_valueText, _fontSource);
        foreach (var line in _rarityLines.Values)
            ApplyFont(line.Count, _fontSource);
    }

    private static void ApplyFont(TextMeshProUGUI text, TMP_Text fontSource)
    {
        text.font = fontSource.font;
        ApplySharedFontMaterial(text, fontSource);
    }

    private static void ApplySharedFontMaterial(TextMeshProUGUI text, TMP_Text fontSource)
    {
        if (fontSource?.fontSharedMaterial != null)
            text.fontSharedMaterial = fontSource.fontSharedMaterial;
    }

    private static void StretchToHud(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private readonly struct RarityLine
    {
        public readonly GameObject Root;
        public readonly RectTransform Rect;
        public readonly TextMeshProUGUI Count;

        public RarityLine(GameObject root, RectTransform rect, TextMeshProUGUI count)
        {
            Root = root;
            Rect = rect;
            Count = count;
        }
    }
}
