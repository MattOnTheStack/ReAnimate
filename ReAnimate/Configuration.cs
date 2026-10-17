using Dalamud.Configuration;

namespace ReAnimate;

public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    // Penumbra mod folder name on disk and its display name in the mod list.
    public string ModDirectory { get; set; } = "ReAnimate";
    public string ModDisplayName { get; set; } = "ReAnimate";

    // Where an animation swap lands: its own "<mod> - ReAnimate swaps" mod (the source mod
    // untouched), a "ReAnimate swaps" option inside the source mod, or the source mod's own
    // paths rewritten in place.
    public SwapPlacement SwapPlacement { get; set; } = SwapPlacement.NewMod;

    // An animation with no start of its own moved onto a /cpose slot: the slot's vanilla
    // start is cut to one frame so the idle begins at once (off = the vanilla transition plays).
    public bool InstantStart { get; set; } = true;

    // High so the bake wins over other animation mods touching the same paths.
    public int ModPriority { get; set; } = 99;

    // Where /reanimate save lands when the current stance isn't bakeable.
    public int LastFamily { get; set; } = -1;
    public byte LastSlot { get; set; }

    // Base the bake on whatever mod currently replaces that idle (top priority in the
    // character's collection, ReAnimate itself excluded) instead of the vanilla file.
    public bool SaveOverModded { get; set; } = true;

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}

public enum SwapPlacement
{
    NewMod,
    OptionInMod,
    RewriteMod,
}
