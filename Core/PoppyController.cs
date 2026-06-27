using System.Collections.Generic;
using UnityEngine;

namespace PoppyMenu
{
    internal class PoppyController : MonoBehaviour
    {
        private readonly List<PoppyModule> _modules = new List<PoppyModule>();
        private readonly List<TabGroup> _groups = new List<TabGroup>();
        private bool _catalogsTried;
        private string _lastScene;
        private bool _pendingAutoApply;

        private static string ActiveScene() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        private void Awake()
        {
            _groups.Add(new TabGroup("Home", new HomeModule()));
            _groups.Add(new TabGroup("Player", new PlayerModule(), new AimbotModule(), new StatsModule(), new MovementModule()));
            _groups.Add(new TabGroup("Items", new ItemsModule()));
            _groups.Add(new TabGroup("Character", new CharacterModule()));
            _groups.Add(new TabGroup("World", new WorldModule(), new SpawnModule(), new RunModule(), new TeleporterModule()));
            _groups.Add(new TabGroup("Players", new PlayersModule()));
            _groups.Add(new TabGroup("Visuals", new RenderModule()));
            _groups.Add(new TabGroup("Console", new ConsoleModule()));
            _groups.Add(new TabGroup("Settings", new SettingsModule(), new KeybindsModule(), new PresetsModule(), new MacrosModule()));

            foreach (TabGroup g in _groups) _modules.AddRange(g.Pages);

            RoR2.CharacterBody.onBodyStartGlobal += OnBodyStart;

            gameObject.AddComponent<CursorOverlay>();
        }

        private void OnBodyStart(RoR2.CharacterBody body)
        {
            if (body == null) return;
            RoR2.NetworkUser nu = RoR2.Util.LookUpBodyNetworkUser(body);
            if (nu != null && RoR2.NetworkUser.readOnlyLocalPlayersList.Contains(nu))
                _pendingAutoApply = true;
        }

        private void Update()
        {
            PlayerContext.Refresh();

            if (PlayerContext.InGame)
            {
                if (!Catalogs.Ready && !_catalogsTried)
                {
                    _catalogsTried = true;
                    try { Catalogs.Refresh(); } catch (System.Exception e) { Log.Error(e); }
                    _lastScene = ActiveScene();
                }
                else if (Catalogs.Ready)
                {
                    string scene = ActiveScene();
                    if (scene != _lastScene)
                    {
                        _lastScene = scene;
                        try { Catalogs.RefreshSpawnCards(); } catch (System.Exception e) { Log.Error(e); }
                    }
                }
            }

            NetUtil.TickGuards();

            Rebind.Poll();
            if (Rebind.IsActive && !MenuRoot.Visible) Rebind.Cancel();

            if (!PlayerContext.InGame) WorldModule.RestoreTime();

            if (_pendingAutoApply && PlayerContext.HasBody)
            {
                _pendingAutoApply = false;
                try { PresetStore.ApplyStartupPresets(); PresetStore.ApplyAutoPresets(); } catch (System.Exception e) { Log.Error(e); }
            }

            HandleHotkeys();

            InputCapture.Sync(MenuRoot.Visible || ListPicker.IsOpen);

            if (PlayerContext.InGame)
            {
                foreach (PoppyModule m in _modules)
                {
                    try { m.Tick(); }
                    catch (System.Exception e) { Log.Error($"{m.Name}.Tick: {e}"); }
                }
            }
        }

        private void HandleHotkeys()
        {
            if (Rebind.IsActive) return;

            if (Input.GetKeyDown(ModConfig.ToggleMenuKey.Value))
            {
                MenuRoot.Visible = !MenuRoot.Visible;
                if (!MenuRoot.Visible) { ListPicker.Close(); MenuRoot.SaveLayout(); }
            }

            BindStore.Poll();
        }

        private void OnGUI()
        {
            Theme.EnsureInit();

            if (PlayerContext.InGame)
            {
                foreach (PoppyModule m in _modules)
                {
                    try { m.DrawOverlay(); }
                    catch {  }
                }
            }

            Notify.Draw();

            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * ModConfig.UiScale.Value);
            MenuRoot.Draw(_groups);
        }

        private void LateUpdate()
        {
            if (MenuRoot.Visible || ListPicker.IsOpen)
                Cursor.lockState = CursorLockMode.None;
        }

        private void OnDestroy()
        {
            RoR2.CharacterBody.onBodyStartGlobal -= OnBodyStart;

            MenuRoot.Visible = false;
            ListPicker.Close();
            try { InputCapture.Shutdown(); } catch { }
            try { Aim.Shutdown(); } catch { }
            try { Safety.Shutdown(); } catch { }
            try { ConsoleCommands.Shutdown(); } catch { }
            foreach (PoppyModule m in _modules)
            {
                try { m.OnUnload(); } catch { }
            }
        }
    }
}
