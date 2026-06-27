# Changelog

## 2.2.2
- Presets can be set to "Load on startup". Mark one as your default and it applies automatically the first time you load into a game each session, so reopening the game restores your setup. Shown with a STARTUP badge.

## 2.2.1
- Homing projectiles are now part of the aimbot itself. Whenever the aimbot has a target, your projectiles curve onto it, no separate toggle. Magic Bullet stays a toggle.

## 2.2.0
- Macros are now fully custom. A step can be give a specific item x N, give a specific buff for N seconds, money/XP/lunar +N, spawn N of something, become a body, heal, run a console command, run a feature, or even run another macro.
- New "My Shortcuts" on Home: pin features and items from search, or turn on Customize to build your own buttons that do any of the above. Reorder and remove them. It's your dashboard.
- Same builder powers macros and Home, so anything you can put in a macro you can also pin to Home and bind to a key.

## 2.1.0
- Home search: type to find any feature or item and run it right from the box.
- Favorites: pin any feature to Home, and every feature shows its bound key inline.
- Custom macros: build a named sequence of features and bind it to one key (new Macros page under Settings).
- Spawn options: pick how many to spawn and whether they're your ally or an enemy.
- Preset sharing: Export copies a preset to a code on your clipboard, Import adds one from a pasted code.
- Undo: restore your inventory to before the last Give All, Clear, Stack, or Reroll.

## 2.0.3
- Give All Items now grants the highest-quality version of each item instead of the base. Items with quality variants (the duplicate-per-quality kind some mods add) get their best one, so if a Legendary exists, that's what you get.
- It gives one item per name now (the best), so it's lighter than dumping every quality.

## 2.0.2
- Give All Items now only hands out normal beneficial items. It skips Lunar downside items (Shaped Glass and the like), untiered/hidden entries, and world-unique items, so it stops killing you, and it heals you to full afterward just in case.
- That also cuts the lag a lot, since it grants far fewer items, though giving hundreds of modded items at once will still hitch.

## 2.0.1
- Item list cleanup: stripped raw markup from item, equipment, and body names (no more "<sprite name=...>" showing).
- Quality variants of the same item now group into one row that opens a sub-list of qualities, with a back option.
- Fixed the picker popup resize so it tracks the mouse like the main window instead of dropping on a fast drag.

## 2.0.0
The big one: merged in the feature sets of two great open mods (DebugToolkit by harbingerofme, Aerolt by Lodington), reimplemented from scratch and credited. Now one build with everything, and the project is open source under MIT.

- New Run tab: set stages cleared, set run time, set team level, toggle every artifact mid-run.
- New Players tab: per-player heal, revive, hurt, kill, teleport, set team, give items, and change body.
- New Console tab: short commands (give, spawn, kill_all, no_enemies, and more) with a searchable reference, plus a fallback to the game's own console.
- New safety toggles in World: no enemies, lock experience, prevent profile saving, and buddha mode in Player.
- Combat tools: heal and hurt by amount, timed buffs, and inflict any damage-over-time effect.
- ESP customization: custom colors per category, a max-distance filter, marker size, and name/distance/health toggles.
- Macros on Home: mid-game, end-game, and a movement-item zoom set.
- Collapsed the dev/full split into a single build with everything included.
- Universal keybinds: bind any feature to any key or mouse button from a new Keybinds tab, saved to disk.
- Customizable menu accent color in Settings, and the tab sidebar now scrolls.
- Reorganized 17 flat tabs into 9 grouped tabs with sub-navigation (Player holds Combat/Aimbot/Stats/Move, World holds Time/Spawn/Run/Teleport, Settings holds Settings/Keybinds/Presets).
- All DebugToolkit command names work in the normal console (give_item, spawn_ai, no_enemies, and the rest).
- Smoother window resize: grabbing the corner now follows the mouse even on a fast drag.
- Aimbot is silent-aim only now; removed the camera-move mode and its smoothing.
- Tidied the in-menu help text so it reads like a person wrote it.

## 1.0.0 Poppy Menu (first release)
A ground-up rewrite and rebrand of the old Umbra Menu for current Risk of Rain 2.

- Aimbot tab: silent aim for any survivor, homing projectiles, magic-bullet wall-pierce, FOV/range/line-of-sight/priority, and a hold key (mouse buttons included).
- No-clip, world time controls (freeze match, freeze timer, time scale), and presets with auto-grant on spawn.
- Host-permission gate so clients can't use the mod in your game unless you allow it.
- A clickable window: draggable, resizable, with a sidebar, a Home dashboard, switch-style toggles, searchable tier-colored item pickers, a status pill, and an active-effects readout while it's closed.
- On-screen popups, an in-game Settings tab with live key rebinding, confirm guards on destructive actions, and a disable-all panic button.
- Cursor and input are handled cleanly while the menu is open, so no flicker and no camera spin.
- Now a BepInEx 5 plugin instead of an injected DLL: no injector, no admin, auto-loads, and it coexists with other mods.
- Reads everything live from the game catalogs, so modded items, equipment, buffs, and bodies show up on their own.
- Works as a non-host client too, routing server actions over R2API networking.

### Features
- Player: God Mode, Infinite Skills, Aimbot, Give Money / XP / Lunar Coins, Respawn, give/remove buffs.
- Stats: per-stat multipliers (damage, attack speed, move speed, armor, crit, max health) + live readout.
- Movement: Flight, Always Sprint, Jump Pack.
- Items: give any item/equipment (searchable, tier-colored), give-all, clear, stack (Shrine of Order), reroll, no equipment cooldown.
- Spawn: spawn any monster/interactable at your crosshair, kill all enemies.
- Teleporter: instant charge, skip stage, mountain shrine stack, spawn shop/gold/celestial portals.
- Character: change into any body (survivors and monsters).
- ESP: enemies, interactables, and the teleporter through walls.
