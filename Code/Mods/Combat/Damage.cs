#define TARGETING_DEBUG2

namespace Vheos.Mods.Outward;

public class Damage : AMod
{
    #region Settings
    private static readonly Dictionary<Team, DamageSettings> _settingsByTeam = new();
    private class DamageSettings : PerValueSettings<Damage, Team>
    {
        public ModSetting<int> HealthDamageMultiplier, FFHealthDamageMultiplier;
        public ModSetting<int> StabilityDamageMultiplier, FFStabilityDamageMultiplier;
        public DamageSettings(Damage mod, Team team, bool isToggle = false) : base(mod, team, isToggle)
        {
            int ffMultiplier = team == Team.Players ? 0 : 100;
            HealthDamageMultiplier = CreateSetting(nameof(HealthDamageMultiplier), 100, mod.IntRange(0, 200));
            StabilityDamageMultiplier = CreateSetting(nameof(StabilityDamageMultiplier), 100, mod.IntRange(0, 200));
            FFHealthDamageMultiplier = CreateSetting(nameof(FFHealthDamageMultiplier), ffMultiplier, mod.IntRange(0, 200));
            FFStabilityDamageMultiplier = CreateSetting(nameof(FFStabilityDamageMultiplier), ffMultiplier, mod.IntRange(0, 200));
        }
    }
    protected override void Initialize()
    {
        foreach (var team in Utility.GetEnumValues<Team>())
            _settingsByTeam[team] = new(this, team);
    }
    protected internal override void LoadPreset(string presetName)
    {
        switch (presetName)
        {
            case nameof(Preset.Vheos_CoopSurvival):
                ForceApply();
                _settingsByTeam[Team.Players].HealthDamageMultiplier.Value = 50;
                _settingsByTeam[Team.Players].StabilityDamageMultiplier.Value = 50;
                _settingsByTeam[Team.Players].FFHealthDamageMultiplier.Value = 20;
                _settingsByTeam[Team.Players].FFStabilityDamageMultiplier.Value = 40;

                _settingsByTeam[Team.Enemies].HealthDamageMultiplier.Value = 80;
                _settingsByTeam[Team.Enemies].StabilityDamageMultiplier.Value = 120;
                _settingsByTeam[Team.Enemies].FFHealthDamageMultiplier.Value = 20;
                _settingsByTeam[Team.Enemies].FFStabilityDamageMultiplier.Value = 40;
                break;
        }
    }
    #endregion

    #region Formatting
    protected override string SectionOverride
        => ModSections.Combat;
    protected override string Description
        => "• Adjust players' and enemies' damages" +
        "\n• Enable friendly fire between players" +
        "\n• Adjust friendly fire between enemies";
    protected override void SetFormatting()
    {
        foreach (var settings in _settingsByTeam.Values)
        {
            settings.FormatHeader();
            string teamName = settings.Value.ToString().ToLower();

            settings.Header.Description =
                $"Multipliers for damages dealt by {teamName}";
            using (Indent)
            {
                settings.HealthDamageMultiplier.Format("Health");
                settings.HealthDamageMultiplier.Description =
                    $"How much health damage {teamName} deal" +
                    $"\n\nUnit: percent multiplier";
                settings.StabilityDamageMultiplier.Format("Stability");
                settings.StabilityDamageMultiplier.Description =
                    $"How much stability damage {teamName} deal" +
                    $"\n\nUnit: percent multiplier";
                CreateHeader("Friendly Fire").Description =
                    $"Additional multipliers for damages dealt by {teamName} to other {teamName}";
                using (Indent)
                {
                    settings.FFHealthDamageMultiplier.Format("Health");
                    settings.FFHealthDamageMultiplier.Description =
                        $"How much health damage {teamName} deal to other {teamName}" +
                        $"\n\nUnit: percent multiplier";
                    settings.FFStabilityDamageMultiplier.Format("Stability");
                    settings.FFStabilityDamageMultiplier.Description =
                        $"How much stability damage {teamName} deal to other {teamName}" +
                        $"\n\nUnit: percent multiplier";
                }
            }
        }
    }
    #endregion

    #region Utility
    private static bool IsPlayersFriendlyFireEnabled
        => _settingsByTeam[Team.Players].FFHealthDamageMultiplier > 0
        || _settingsByTeam[Team.Players].FFStabilityDamageMultiplier > 0;
    private static void TryOverrideElligibleFaction(ref bool result, Character defender, Character attacker)
    {
        if (result
        || defender == null
        || defender == attacker
        || !defender.IsAlly()
        || !IsPlayersFriendlyFireEnabled)
            return;

        result = true;
    }
    #endregion

    #region Hooks
    [HarmonyPostfix, HarmonyPatch(typeof(Weapon), nameof(Weapon.ElligibleFaction), new[] { typeof(Character) })]
    private static void Weapon_ElligibleFaction_Post(Weapon __instance, ref bool __result, Character _character)
        => TryOverrideElligibleFaction(ref __result, _character, __instance.OwnerCharacter);

    [HarmonyPostfix, HarmonyPatch(typeof(MeleeHitDetector), nameof(MeleeHitDetector.ElligibleFaction), new[] { typeof(Character) })]
    private static void MeleeHitDetector_ElligibleFaction_Post(MeleeHitDetector __instance, ref bool __result, Character _character)
        => TryOverrideElligibleFaction(ref __result, _character, __instance.OwnerCharacter);

    [HarmonyPrefix, HarmonyPatch(typeof(Character), nameof(Character.OnReceiveHitCombatEngaged))]
    private static bool Character_OnReceiveHitCombatEngaged_Pre(Character __instance, Character _dealerChar)
        => _dealerChar == null
        || !_dealerChar.IsAlly()
        || !IsPlayersFriendlyFireEnabled;

    [HarmonyPrefix, HarmonyPatch(typeof(Character), nameof(Character.VitalityHit))]
    private static void Character_VitalityHit_Pre(Character __instance, Character _dealerChar, ref float _damage)
    {
        if (_dealerChar != null && _dealerChar.IsEnemy()
        || _dealerChar == null && __instance.IsAlly())
        {
            _damage *= _settingsByTeam[Team.Enemies].HealthDamageMultiplier / 100f;
            if (__instance.IsEnemy())
                _damage *= _settingsByTeam[Team.Enemies].FFHealthDamageMultiplier / 100f;
        }
        else
        {
            _damage *= _settingsByTeam[Team.Players].HealthDamageMultiplier / 100f;
            if (__instance.IsAlly())
                _damage *= _settingsByTeam[Team.Players].FFHealthDamageMultiplier / 100f;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Character), nameof(Character.StabilityHit))]
    private static void Character_StabilityHit_Pre(Character __instance, Character _dealerChar, ref float _knockValue)
    {
        if (_dealerChar != null && _dealerChar.IsEnemy()
        || _dealerChar == null && __instance.IsAlly())
        {
            _knockValue *= _settingsByTeam[Team.Enemies].StabilityDamageMultiplier / 100f;
            if (__instance.IsEnemy())
                _knockValue *= _settingsByTeam[Team.Enemies].FFStabilityDamageMultiplier / 100f;
        }
        else
        {
            _knockValue *= _settingsByTeam[Team.Players].StabilityDamageMultiplier / 100f;
            if (__instance.IsAlly())
                _knockValue *= _settingsByTeam[Team.Players].FFStabilityDamageMultiplier / 100f;
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(TargetingSystem), nameof(TargetingSystem.InitTargetableFaction))]
    static void TargetingSystem_InitTargetableFaction_Post(TargetingSystem __instance)
    {
#if TARGETING_DEBUG
        Log.Debug("TARGET SYSTEM INIT");
        Log.Debug("ID: " + __instance.GetInstanceID());
        Log.Debug("CHARACTER: " + __instance.m_character);
        Log.Debug("CHARACTER ID: " + __instance.m_character.GetInstanceID());
        Log.Debug("CHARACTER IS PLAYER: " + __instance.m_character.IsLocalPlayer);
#endif
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayerSystem), nameof(PlayerSystem.SetCharacter))]
    static void PlayerSystem_SetCharacter_Post(PlayerSystem __instance, Character _character)
    {
#if TARGETING_DEBUG
        Log.Debug("PLAYER SYSTEM SET CHARACTER");
        Log.Debug("ID: " + __instance.GetInstanceID());
        Log.Debug("CHARACTER: " + _character);
        Log.Debug("CHARACTER ID: " + _character.GetInstanceID());
        Log.Debug("CHARACTER IS PLAYER: " + _character.IsLocalPlayer);
        Log.Debug("CHARACTER TARGET SYSTEM: " + _character.m_targetingSystem);
        Log.Debug("CHARACTER TARGET SYSTEM ID: " + _character.m_targetingSystem.GetInstanceID());
#endif

        List<Character.Factions> list = new List<Character.Factions>();
        for (int i = 1; i < 10; i++)
        {
            Character.Factions factions = (Character.Factions)i;
            list.Add(factions);
        }
        _character.m_targetingSystem.TargetableFactions = list.ToArray();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(TargetingSystem), nameof(TargetingSystem.SetCharacter))]
    static void TargetingSystem_SetCharacter_Post(TargetingSystem __instance, Character _character)
    {
#if TARGETING_DEBUG
        Log.Debug("TARGET SYSTEM SET CHARACTER");
        Log.Debug("ID: " + __instance.GetInstanceID());
        Log.Debug("CHARACTER: " + _character);
        Log.Debug("CHARACTER ID: " + _character.GetInstanceID());
        Log.Debug("CHARACTER IS PLAYER: " + _character.IsLocalPlayer);
#endif
    }

    [HarmonyPostfix, HarmonyPatch(typeof(Character), nameof(Character.Awake))]
    static void Character_Awake_Post(Character __instance)
    {
#if TARGETING_DEBUG
        Log.Debug("CHARACTER AWAKE");
        Log.Debug("ID: " + __instance.GetInstanceID());
        Log.Debug("IS PLAYER: " + __instance.IsLocalPlayer);
        Log.Debug("TARGET SYSTEM: " + __instance.m_targetingSystem);
        Log.Debug("TARGET SYSTEM ID: " + __instance.m_targetingSystem.GetInstanceID());
#endif
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ShootItem), nameof(ShootItem.Activate))]
    static void ShootItem_Activate_Postfix(ShootItem __instance, Character _affectedCharacter)
    {
#if TARGETING_DEBUG
        Log.Debug("SHOOT ITEM");
        Log.Debug("ID: " + __instance.GetInstanceID());
        Log.Debug("CHARACTER: " + _affectedCharacter);
        Log.Debug("CHARACTER ID: " + _affectedCharacter.GetInstanceID());
        Log.Debug("CHARACTER IS PLAYER: " + _affectedCharacter.IsLocalPlayer);
        Log.Debug("CHARACTER TARGET SYSTEM: " + _affectedCharacter.m_targetingSystem);
        Log.Debug("CHARACTER TARGET SYSTEM ID: " + _affectedCharacter.m_targetingSystem.GetInstanceID());
#endif
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ShootProjectile), nameof(ShootProjectile.Setup))]
    static void ShootProjectile_Setup_Postfix(ShootProjectile __instance, Character.Factions[] _targetFactions)
    {
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Projectile), nameof(Projectile.Setup))]
    static void Projectile_Setup_Postfix(Projectile __instance, Character.Factions[] _targetFactions)
    {
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Projectile), nameof(Projectile.Explode), new[] { typeof(Collider), typeof(Vector3), typeof(Vector3), typeof(bool) })]
    static void Prefix(Projectile __instance, UnityEngine.Collider _collider, UnityEngine.Vector3 __1, UnityEngine.Vector3 __2, bool __3)
    {
#if TARGETING_DEBUG
        foreach (Character.Factions faction in __instance.m_targetableFactions)
        {
            Log.Debug("FACTIONS: " + faction.ToString());
        }
#endif
    }


    //  Log.Debug("AAAA");
    #endregion
}