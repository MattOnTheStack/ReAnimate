# ReAnimate (Instant Start)

A fork of [Nyaughty's ReAnimate](https://github.com/Nyotee/ReAnimate) with two additions to
its Animation Swap tab:

- **Instant start.** When you move an idle that has no start animation of its own onto
  another /cpose slot, the slot's vanilla start transition is cut to one frame, so your
  character goes straight into the idle. Pick **Vanilla transition** to keep the game's
  normal transition instead.
- **Default idles can be moved.** Mods that replace the default idle (the one you stand in
  before ever using /cpose), or the default chair-sit, ground-sit or doze pose, can be moved
  onto numbered /cpose slots.

Everything else is ReAnimate as it is.

## Install (once, on the PC you play on)

1. Type `/xlsettings`, open **Experimental**, add this under **Custom Plugin
   Repositories**, tick it and click **Save**:

   ```
   https://raw.githubusercontent.com/MattOnTheStack/ReAnimate/dist/repo.json
   ```

2. Type `/xlplugins`, search for **ReAnimate (Instant Start)** and install it. Penumbra is
   required.
3. If you also have the official **ReAnimate** installed, disable it. Both use `/reanimate`.

Updates arrive like any other plugin's: **Update** in `/xlplugins`, or automatically when
you log in.

## Move an idle to another /cpose slot

1. Type `/reanimate` and open the **Animation Swap** tab.
2. Pick the idle mod on the left.
3. Under **When there's no start of its own**, leave **Instant start** selected, or pick
   **Vanilla transition**. If the mod has its own start, it shows that instead, and the start
   moves along with the idle.
4. In the idle's row, click the button in the **Play it as** column and pick a slot, for
   example **Standing Idle 6**. Sitting, Ground Sit and Doze slots work the same way.
5. Click **Swap**, then use `/cpose` until you reach that slot.

The swap is added to the original mod as an option in a **ReAnimate swaps** group. To undo
it, untick that option in Penumbra.

The idle also keeps playing on its original slot. If you only want it on the new one, untick
the mod's own option for the original slot in Penumbra, if it has one. Otherwise, untick
**Add as a mod option** before swapping: that moves the idle by rewriting the mod itself,
which can't be undone except by reinstalling the mod.

## About ReAnimate

ReAnimate turns your character's current pose into an idle pose (including combat/weapon
poses) with a single click. It keeps natural breathing motion, so the result doesn't look
static, and it works whether you posed in GPose or not.

- `/reanimate` opens the window. Idle2Pose tab: pick race, pose type and slot (they default
  to what you're doing on screen), then save. Animation Swap tab: see above.
- `/reanimate save` saves straight into your current idle, no window.
- `/reanimate idle` / `sitting` / `groundsit` / `doze` / `weapon`, optionally followed by a
  slot number, saves into that pose type.

## Building

Every push to `main` or `instant-start` is built by GitHub Actions, published as a release,
and added to the plugin repository above. To build locally, download
`https://goatcorp.github.io/dalamud-distrib/latest.zip`, extract it into `.dalamud/`, then:

```
dotnet build ReAnimate/ReAnimate.csproj -c Release -p:DalamudLibPath=../.dalamud/
```

Offline checks against real game files: `python tools/Checks/extract_testfiles.py testfiles`,
then `dotnet run --project tools/Checks -- testfiles`.

## Credits

ReAnimate is by Nyaughty. See also [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## License

MIT. See [LICENSE](LICENSE).
