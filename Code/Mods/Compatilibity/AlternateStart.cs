using AlternateStart.StartScenarios;

namespace Vheos.Mods.Outward;

public class AlternateStart : AMod
{
    #region Constants
    #endregion
    #region Enums
    #endregion

    public static ModSetting<bool> _disallowedInCombatSwitchAreaGiantScenario;
    protected override void Initialize()
    {
        _disallowedInCombatSwitchAreaGiantScenario = CreateSetting(nameof(_disallowedInCombatSwitchAreaGiantScenario), false);
    }
    protected override void SetFormatting()
    {
        _disallowedInCombatSwitchAreaGiantScenario.Format("Enable AlternateStart Giant Scenario");
        _disallowedInCombatSwitchAreaGiantScenario.Description = "If using 'Interaction - Disallowed in Combat - Travel' and play Giant Scenario, you would not be able to leave starting zone. Enable this for a fix.";
    }
    protected override string Description
    => "• Fixes some incompability between this mod and AlternateStart";
    protected override string SectionOverride
    => ModSections.Compatibility;
    protected internal override void LoadPreset(string presetName)
    {
        switch (presetName)
        {
            case nameof(Preset.Vheos_CoopSurvival):
                ForceApply();
                _disallowedInCombatSwitchAreaGiantScenario.Value = false;
                break;
        }
    }

    public static bool temporaryAllowedTravel = false;

    // Hooks
    [HarmonyPostfix, HarmonyPatch(typeof(GiantRisenScenario), nameof(GiantRisenScenario.UpdateQuestProgress))]
    private static void GiantRisenScenario_UpdateQuestProgress_Post(GiantRisenScenario __instance, Quest quest)
    {
        if (!_disallowedInCombatSwitchAreaGiantScenario.Value)
            return;

        var eventUID = Traverse.Create(__instance).Field("QE_FixedGiantRisenStart").GetValue<QuestEventSignature>().EventUID;

        bool inGiantsVillage = QuestEventManager.Instance.GetEventCurrentStack(eventUID) < 2;

        temporaryAllowedTravel = inGiantsVillage;
    }
}
