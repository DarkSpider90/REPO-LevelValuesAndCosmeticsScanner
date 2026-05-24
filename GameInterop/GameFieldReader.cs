using System.Reflection;

namespace LevelValuesAndCosmeticsScanner.GameInterop;

internal static class GameFieldReader
{
    private static readonly BindingFlags InstanceField =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly FieldInfo ValuableCurrentValue =
        typeof(ValuableObject).GetField("dollarValueCurrent", InstanceField);

    private static readonly FieldInfo ValuableValueReady =
        typeof(ValuableObject).GetField("dollarValueSet", InstanceField);

    private static readonly FieldInfo RoundCompleted =
        typeof(RoundDirector).GetField("allExtractionPointsCompleted", InstanceField);

    private static readonly FieldInfo MapIsActive =
        typeof(MapToolController).GetField("Active", InstanceField);

    public static float CurrentValue(ValuableObject valuable)
    {
        return ReadField(ValuableCurrentValue, valuable, 0f);
    }

    public static bool ValueWasAssigned(ValuableObject valuable)
    {
        return ReadField(ValuableValueReady, valuable, true);
    }

    public static bool ExtractionIsFinished(RoundDirector roundDirector)
    {
        return ReadField(RoundCompleted, roundDirector, false);
    }

    public static bool LocalMapIsOpen(MapToolController mapTool)
    {
        return ReadField(MapIsActive, mapTool, false);
    }

    private static T ReadField<T>(FieldInfo field, object owner, T defaultValue)
    {
        if (field == null || owner == null)
            return defaultValue;

        var value = field.GetValue(owner);
        return value is T typed ? typed : defaultValue;
    }
}
