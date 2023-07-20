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

    private static readonly int ZHORN_DEMON_SHIELD_ID = "Zhorns Demon Shield".ToItemID();
    private static readonly int ZHORN_HUNTING_BACKPACK_ID = "Zhorns Hunting Backpack".ToItemID();
    private static readonly int ZHORN_GLOWSTONE_DAGGER_ID = "Zhorns Glowstone Dagger".ToItemID();
    private static readonly int ASH_ARMOR_ID = "Ash Armor".ToItemID();
    private static readonly int ASH_BOOTS_ID = "Ash Boots".ToItemID();
    private static readonly int ELITE_HOOD_ID = "Elite Hood".ToItemID();
    private static readonly int FANG_CLUB_ID = "Fang Club".ToItemID();

    private static readonly string SHIV_SKILL_UID = "7mRc8ivfKkO39OMa9kEpcQ";
    #endregion

    public static ModSetting<bool> _alternateStartEnabled;
    // General
    public static int temporarySkillPriceModifier = 1;
    // Scenario: Giant
    public static ModSetting<bool> _scenarioGiant;
        public static bool temporaryAllowedTravel = false;
        public static ModSetting<bool> _giantRobCustom;
    // Scenario: Surivor
    public static ModSetting<bool> _scenarioSurvivor;
        public static ModSetting<bool> _survivorShivDagger;
        public static ModSetting<bool> _survivorRobCustom;
    
    protected override void Initialize()
    {
        _alternateStartEnabled      = CreateSetting(nameof(_alternateStartEnabled), false);

        _scenarioGiant              = CreateSetting(nameof(_scenarioGiant),         false);
        _giantRobCustom             = CreateSetting(nameof(_giantRobCustom),        false);

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

        _scenarioGiant.Format("Scenario: Giant");
        using (Indent)
        {
            _giantRobCustom.Format("Rob One Choice");
            _giantRobCustom.Description = "Start with; Fang Mace, Ash Armor, Ash Boots, Elite Hood, Zhorn Backpack, Zhorn Shield, Zhorn Dagger";
            _giantRobCustom.IsAdvanced = true;
        }

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

                _scenarioGiant.Value = false;
                _giantRobCustom.Value = false;

                _scenarioSurvivor.Value = false;
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

        if (!_scenarioGiant.Value)
            return;

        var eventUID = Traverse.Create(__instance).Field("QE_FixedGiantRisenStart").GetValue<QuestEventSignature>().EventUID;

        bool inGiantsVillage = QuestEventManager.Instance.GetEventCurrentStack(eventUID) < 2;

        temporaryAllowedTravel = inGiantsVillage;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GiantRisenScenario), nameof(GiantRisenScenario.Gear))]
    private static bool GiantRisenScenario_Gear_Pre(GiantRisenScenario __instance, Character character)
    {
        if (!_alternateStartEnabled.Value)
            return true;

        if (!_scenarioGiant.Value)
            return true;

        if (!_giantRobCustom.Value)
            return true;

        character.Inventory.ReceiveItemReward(ZHORN_HUNTING_BACKPACK_ID, 1, true);
        character.Inventory.ReceiveItemReward(ASH_ARMOR_ID, 1, true);
        character.Inventory.ReceiveItemReward(ASH_BOOTS_ID, 1, true);
        character.Inventory.ReceiveItemReward(ELITE_HOOD_ID, 1, true);
        character.Inventory.ReceiveItemReward(FANG_CLUB_ID, 1, true);
        character.Inventory.ReceiveItemReward(ZHORN_DEMON_SHIELD_ID, 1, true);
        //character.Inventory.ReceiveItemReward(ZHORN_GLOWSTONE_DAGGER_ID, 1, true);

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SurvivorScenario), nameof(SurvivorScenario.Gear))]
    private static bool SurvivorScenario_Gear_Pre(SurvivorScenario __instance, Character character)
    {
        if (!_alternateStartEnabled.Value)
            return true;

        if (_survivorShivDagger.Value)
        {
            RecipeManager.Instance.m_recipes.TryGetValue(SHIV_SKILL_UID, out var shivDaggerRecipe);
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
