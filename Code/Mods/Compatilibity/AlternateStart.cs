using AlternateStart.StartScenarios;

namespace Vheos.Mods.Outward;

public class AlternateStart : AMod
{
    #region Constants
    #endregion
    #region Enums
    #endregion

    public static ModSetting<bool> _alternateStartEnabled;
    protected override void Initialize()
    {
        _alternateStartEnabled = CreateSetting(nameof(_alternateStartEnabled), false);
    }
    protected override void SetFormatting()
    {
        _alternateStartEnabled.Format("Enable AlternateStart");
    }
    protected override string Description
    => "• Fixed: Giant Risen";
    protected override string SectionOverride
    => ModSections.Compatibility;
    protected internal override void LoadPreset(string presetName)
    {
        switch (presetName)
        {
            case nameof(Preset.Vheos_CoopSurvival):
                ForceApply();
                _alternateStartEnabled.Value = false;
                break;
        }
    }

    public static bool temporaryAllowedTravel = false;

    // Hooks
    [HarmonyPostfix, HarmonyPatch(typeof(GiantRisenScenario), nameof(GiantRisenScenario.UpdateQuestProgress))]
    private static void GiantRisenScenario_UpdateQuestProgress_Post(GiantRisenScenario __instance, Quest quest)
    {
        if (!_alternateStartEnabled.Value)
            return;

        var eventUID = Traverse.Create(__instance).Field("QE_FixedGiantRisenStart").GetValue<QuestEventSignature>().EventUID;

        bool inGiantsVillage = QuestEventManager.Instance.GetEventCurrentStack(eventUID) < 2;

        temporaryAllowedTravel = inGiantsVillage;
    }
}
