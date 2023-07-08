namespace Vheos.Mods.Outward;

public class TrueHardcore : AMod
{
    #region Constants
    private static readonly int[] ITEMS =
    {
        ((int)HardcoreRebalance.HardcoreItems.ElattPiece)
    };
    #endregion
    #region Enums
    #endregion

    public static ModSetting<bool> _limitedManualCraftingTrueHardcore;
    protected override void Initialize()
    {
        _limitedManualCraftingTrueHardcore = CreateSetting(nameof(_limitedManualCraftingTrueHardcore), false);
    }
    protected override void SetFormatting()
    {
        _limitedManualCraftingTrueHardcore.Format("Enable TrueHardcore craft");
        _limitedManualCraftingTrueHardcore.Description = "With limited manual crafting, you need to enable this to be able to craft TrueHardcore items.";
    }
    protected override string Description
    => "• Fixes some incompability between this mod and TrueHardcore";
    protected override string SectionOverride
    => ModSections.Compatibility;
    protected internal override void LoadPreset(string presetName)
    {
        switch (presetName)
        {
            case nameof(Preset.Vheos_CoopSurvival):
                ForceApply();
                _limitedManualCraftingTrueHardcore.Value = false;
                break;
        }
    }
}
