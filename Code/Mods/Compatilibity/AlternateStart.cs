using AlternateStart.StartScenarios;
using SideLoader;

namespace Vheos.Mods.Outward;

public class AlternateStart : AMod
{
    #region Constants
    #endregion
    #region Enums
    #endregion

    public static ModSetting<bool> _alternateStartEnabled;

    public static bool temporaryAllowedTravel = false;
    public static int temporarySkillPriceModifier = 1;
    protected override void Initialize()
    {
        _alternateStartEnabled = CreateSetting(nameof(_alternateStartEnabled), false);

        SL.OnSceneLoaded += () =>
        {
            temporarySkillPriceModifier = 1;

            if (!_alternateStartEnabled.Value)
                return;

            // There is no way to retrieve these strings.
            if (SceneManagerHelper.ActiveSceneName == "DreamWorld" &&
                !QuestEventManager.Instance.HasQuestEvent("iggythemad.altstart.destinyChosen"))
            {
                temporarySkillPriceModifier = 0;
            }
        };
    }
    protected override void SetFormatting()
    {
        _alternateStartEnabled.Format("Enable AlternateStart");
    }
    protected override string Description
    =>  "• SkillPrices: AlternateStart choices are free\n"+
        "• Fixed: Giant Risen";
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

    // Hooks
    [HarmonyPostfix, HarmonyPatch(typeof(GiantRisenScenario), nameof(GiantRisenScenario.UpdateQuestProgress))]
    private static void GiantRisenScenario_UpdateQuestProgress_Post(GiantRisenScenario __instance, Quest quest)
    {
        temporaryAllowedTravel = false;
        if (!_alternateStartEnabled.Value)
            return;

        var eventUID = Traverse.Create(__instance).Field("QE_FixedGiantRisenStart").GetValue<QuestEventSignature>().EventUID;

        bool inGiantsVillage = QuestEventManager.Instance.GetEventCurrentStack(eventUID) < 2;

        temporaryAllowedTravel = inGiantsVillage;
    }

}
