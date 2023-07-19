using HardcoreRebalance;

namespace Vheos.Mods.Outward;

public class TrueHardcore : AMod
{
    #region Constants
    public static readonly UID mortalityStatusUID = "4BN0PSfq2kaTEQTzvOy3Nw";
    #endregion
    #region Enums
    #endregion

    public static ModSetting<bool> _trueHardcoreEnabled;
    public static ModSetting<bool> _disableBreakFlint;
    protected override void Initialize()
    {
        _trueHardcoreEnabled    = CreateSetting(nameof(_trueHardcoreEnabled), false);
        _disableBreakFlint      = CreateSetting(nameof(_disableBreakFlint), false);
    }
    protected override void SetFormatting()
    {
        _trueHardcoreEnabled.Format("Enable TrueHardcore");
        _disableBreakFlint.Format("Disable TrueHardcore flint break.");
        _disableBreakFlint.Description = "This module already has support for Flint break with better control.";
    }
    protected override string Description
    => "• Fixed: Manual Crafting\n" +
       "• Fixed: Disable flint breaking";
    protected override string SectionOverride
    => ModSections.Compatibility;
    protected internal override void LoadPreset(string presetName)
    {
        switch (presetName)
        {
            case nameof(Preset.Vheos_CoopSurvival):
                ForceApply();
                _trueHardcoreEnabled.Value = false;
                _disableBreakFlint.Value = false;
                break;
        }
    }

    // Hooks
    [HarmonyPrefix, HarmonyPatch(typeof(DeathManager.Item_OnUse), nameof(DeathManager.Item_OnUse.Postfix))]
    private static bool DeathManager_Item_OnUse_Pre()
        => _trueHardcoreEnabled.Value && !_disableBreakFlint.Value;
}
