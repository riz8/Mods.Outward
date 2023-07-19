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

    public static ModSetting<bool> _trueHardcoreEnabled;
    public static readonly UID mortalityStatusUID = "4BN0PSfq2kaTEQTzvOy3Nw";
    protected override void Initialize()
    {
        _trueHardcoreEnabled = CreateSetting(nameof(_trueHardcoreEnabled), false);
    }
    protected override void SetFormatting()
    {
        _trueHardcoreEnabled.Format("Enable TrueHardcore");
    }
    protected override string Description
    => "• Fixed: Manual Crafting";
    protected override string SectionOverride
    => ModSections.Compatibility;
    protected internal override void LoadPreset(string presetName)
    {
        switch (presetName)
        {
            case nameof(Preset.Vheos_CoopSurvival):
                ForceApply();
                _trueHardcoreEnabled.Value = false;
                break;
        }
    }
}
