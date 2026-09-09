using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace ReAnimate.Game;

public sealed record IdleTarget(string GamePath, ushort FileRace, string Label);

// Maps (pose family, cpose slot) to the LOOP pap the game plays, never _start transitions.
// Slot tables and race fallbacks are sqpack-verified; details in NOTES.md.
public static class IdleResolver
{
    // battleFolder ("bt_2sw_emp") only matters for WeaponDrawn; unarmed stances live in bt_common.
    public static IdleTarget? Resolve(EmoteController.PoseType family, byte slot, ushort raceSexId, byte animVariant, string? battleFolder, out string? error)
    {
        if (LoopFile(family, slot) is { } file)
            return Probe(file, family, slot, raceSexId, animVariant, battleFolder, "", out error);
        error = $"'{GameState.FamilyName(family)}' is not supported";
        return null;
    }

    // "Miqo'te Female Standing Idle 3" / "Miqo'te Female Weapon Drawn (Greatsword) 1".
    public static string Label(EmoteController.PoseType family, byte slot, ushort raceSexId, string? battleFolder, string suffix = "")
        => $"{GameState.RaceName(raceSexId)} {GameState.FamilyLabel(family, battleFolder)} {slot}{suffix}";

    // The _start transition played when switching into a cpose slot. Base slots (0) enter
    // through the game's own stance transitions and have none.
    public static IdleTarget? ResolveStart(EmoteController.PoseType family, byte slot, ushort raceSexId, byte animVariant, string? battleFolder)
        => StartFile(family, slot) is { } file ? Probe(file, family, slot, raceSexId, animVariant, battleFolder, " Start", out _) : null;

    // Slot tables (sqpack-verified): slot 0 is the stance's base file, numbered slots are
    // emote/<prefix>poseNN_loop with a _start twin.
    private static string? BaseFile(EmoteController.PoseType family) => family switch
    {
        EmoteController.PoseType.Idle or EmoteController.PoseType.WeaponDrawn => "resident/idle",
        EmoteController.PoseType.Sit => "emote/sit",
        EmoteController.PoseType.GroundSit => "emote/jmn",
        EmoteController.PoseType.Doze => "emote/doze",
        _ => null,
    };

    private static string? Prefix(EmoteController.PoseType family) => family switch
    {
        EmoteController.PoseType.Idle => "",
        EmoteController.PoseType.Sit => "s_",
        EmoteController.PoseType.GroundSit => "j_",
        EmoteController.PoseType.Doze => "l_",
        EmoteController.PoseType.WeaponDrawn => "b_",
        _ => null,
    };

    private static string? LoopFile(EmoteController.PoseType family, byte slot)
        => slot == 0 ? BaseFile(family) : Prefix(family) is { } p ? $"emote/{p}pose{slot:D2}_loop" : null;

    private static string? StartFile(EmoteController.PoseType family, byte slot)
        => slot == 0 ? null : Prefix(family) is { } p ? $"emote/{p}pose{slot:D2}_start" : null;

    // The slot a playing timeline belongs to. Timeline keys are the pap names ("emote/pose03_loop"
    // = Standing 3, start keys included) except the two base idles, which play under their own
    // names; the standing idle's fidget variants count as slot 0 too.
    public static (EmoteController.PoseType Family, byte Slot)? FromKey(string key)
        => Slots.Value.TryGetValue(key, out var slot) ? slot : null;

    private static readonly Lazy<Dictionary<string, (EmoteController.PoseType, byte)>> Slots = new(() =>
    {
        var map = new Dictionary<string, (EmoteController.PoseType, byte)>(StringComparer.Ordinal)
        {
            ["normal/idle"] = (EmoteController.PoseType.Idle, 0),
            ["normal/idle_inactive1"] = (EmoteController.PoseType.Idle, 0),
            ["normal/idle_inactive2"] = (EmoteController.PoseType.Idle, 0),
            ["normal/idle_inactive3"] = (EmoteController.PoseType.Idle, 0),
            ["battle/idle"] = (EmoteController.PoseType.WeaponDrawn, 0),
        };
        foreach (var family in GameState.BakeableFamilies)
        {
            for (byte slot = 0; slot < GameState.MaxSlots; slot++)
            {
                foreach (var (file, _) in FileKeys(family, slot))
                    map.TryAdd(file, (family, slot));
            }
        }

        return map;
    });

    // The loop (and start) file keys of a slot, folder-less, for the animation catalog.
    public static IEnumerable<(string File, string Kind)> FileKeys(EmoteController.PoseType family, byte slot)
    {
        if (LoopFile(family, slot) is { } loop)
            yield return (loop, "loop");
        if (StartFile(family, slot) is { } start)
            yield return (start, "start");
    }

    private static IdleTarget? Probe(string file, EmoteController.PoseType family, byte slot, ushort raceSexId, byte animVariant, string? battleFolder, string labelSuffix, out string? error)
    {
        error = null;
        if (family == EmoteController.PoseType.WeaponDrawn && battleFolder is null)
        {
            error = "this class has no weapon-drawn idles";
            return null;
        }

        var folder = family == EmoteController.PoseType.WeaponDrawn ? battleFolder! : "bt_common";
        foreach (var race in FallbackChain(raceSexId))
        {
            var path = $"chara/human/c{race:D4}/animation/a{animVariant:D4}/{folder}/{file}.pap";
            if (Plugin.DataManager.FileExists(path))
                return new IdleTarget(path, race, Label(family, slot, raceSexId, battleFolder, labelSuffix));
        }

        error = $"no animation file exists for {GameState.FamilyName(family)} {slot}";
        return null;
    }

    public static string SklbPath(ushort race) =>
        $"chara/human/c{race:D4}/skeleton/base/b0001/skl_c{race:D4}b0001.sklb";

    // The game's race inheritance tree: females inherit Midlander F (201), 201 and every
    // male inherit Midlander M (101), Lalafell F inherits Lalafell M (shares its body).
    // A race without its own file requests its parent's path, up to the root.
    public static IEnumerable<ushort> FallbackChain(ushort race)
    {
        yield return race;
        var r = race;
        while (r != 101)
        {
            r = r switch
            {
                1201 => 1101,
                201 => 101,
                _ when r / 100 % 2 == 0 => 201,
                _ => 101,
            };
            yield return r;
        }
    }
}
