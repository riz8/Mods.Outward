using AlternateStart.StartScenarios;
using SideLoader;

namespace Vheos.Mods.Outward;

public class AlternateStart : AMod
{
    #region Constants
    private static readonly int BEGGAR_ARMOR_A_ID = "BeggarArmor A".ToItemID();
    private static readonly int HATCHET_ID = "Hatchet".ToItemID();
    private static readonly int CIERZO_TOWN_KEY_ID = "Cierzo Town Key".ToItemID();
    private static readonly int WATERSKIN_ID = "Waterskin".ToItemID();
    private static readonly int FLINT_AND_STEEL_ID = "Flint and Steel".ToItemID();

    private static readonly int TALUS_CLEAVER_ID = "Talus Cleaver".ToSkillID();

    private static readonly string SHIV_UID = "7mRc8ivfKkO39OMa9kEpcQ";
    #endregion

    public static ModSetting<bool> _alternateStartEnabled;
    // General
    public static int temporarySkillPriceModifier = 1;
    // Scenario: Giant
    public static bool temporaryAllowedTravel = false;
    // Scenario: Surivor
    public static ModSetting<bool> _scenarioSurvivor;
        public static ModSetting<bool> _survivorShivDagger;
        public static ModSetting<bool> _survivorRobCustom;
    
    protected override void Initialize()
    {
        _alternateStartEnabled      = CreateSetting(nameof(_alternateStartEnabled), false);
        _scenarioSurvivor           = CreateSetting(nameof(_scenarioSurvivor),      false);
        _survivorShivDagger         = CreateSetting(nameof(_survivorShivDagger),    false);
        _survivorRobCustom          = CreateSetting(nameof(_survivorRobCustom),     false);

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

        _scenarioSurvivor.Format("Scenario: Survivor");
        using (Indent)
        {
            _survivorShivDagger.Format("Enable Shiv Dagger Recipe");
            _survivorShivDagger.Description = "If starting Scenario Survivor and limited crafting there will be no way to learn shiv dagger. Enable this to auto learn this receipe";

            _survivorRobCustom.Format("Rob One Choice");
            _survivorRobCustom.Description = "Start with; Hatchet, Beggar Armor, Flint&Steel, Waterskin, Cizero Town Key";
            _survivorRobCustom.IsAdvanced = true;
        }

    }
    protected override string Description
    => "• SkillPrices: AlternateStart choices are free\n" +
       "• Fixed: Giant Risen\n" +
       "• Survivor: Enable shiv dagger receipe if using limited crafting\n";

    protected override string SectionOverride
    => ModSections.Compatibility;
    protected internal override void LoadPreset(string presetName)
    {
        switch (presetName)
        {
            case nameof(Preset.Vheos_CoopSurvival):
                ForceApply();
                _alternateStartEnabled.Value = false;
                _survivorShivDagger.Value = false;
                _survivorRobCustom.Value = false;
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

    [HarmonyPrefix, HarmonyPatch(typeof(SurvivorScenario), nameof(SurvivorScenario.Gear))]
    private static bool SurvivorScenario_Gear_Pre(SurvivorScenario __instance, Character character)
    {
        if (!_alternateStartEnabled.Value)
            return true;

        if (_survivorShivDagger.Value)
        {
            RecipeManager.Instance.m_recipes.TryGetValue(SHIV_UID, out var shivDaggerRecipe);
            character.LearnRecipe(shivDaggerRecipe);
        }

        if(!_survivorRobCustom.Value)
            return true;

        character.Inventory.ReceiveItemReward(BEGGAR_ARMOR_A_ID, 1, true);
        character.Inventory.ReceiveItemReward(HATCHET_ID, 1, true);
        character.Inventory.ReceiveItemReward(CIERZO_TOWN_KEY_ID, 1, false);
        character.Inventory.ReceiveItemReward(WATERSKIN_ID, 1, false);
        character.Inventory.ReceiveItemReward(FLINT_AND_STEEL_ID, 1, false);

        character.Inventory.ReceiveSkillReward(TALUS_CLEAVER_ID);

        return false;
    }
}
