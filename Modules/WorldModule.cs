using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal class WorldModule : PoppyModule
    {
        internal override string Name => "Time";

        internal static bool FreezeMatch;
        internal static bool FreezeTimer;
        internal static float TimeScale = 1f;

        private static bool _controllingTime;
        private static bool _timerFrozen;

        internal override void Tick()
        {
            if (NetUtil.IsServer && Run.instance != null)
            {
                if (FreezeTimer) Run.instance.SetRunStopwatchPaused(true);
                else if (_timerFrozen) Run.instance.SetRunStopwatchPaused(false);
            }
            _timerFrozen = FreezeTimer;

            ApplyTimeScale();
        }

        private static void ApplyTimeScale()
        {
            bool want = FreezeMatch || Mathf.Abs(TimeScale - 1f) > 0.001f;
            if (want)
            {
                Time.timeScale = FreezeMatch ? 0f : TimeScale;
                _controllingTime = true;
            }
            else if (_controllingTime)
            {
                if (!PauseManager.isPaused) Time.timeScale = 1f;
                _controllingTime = false;
            }
        }

        internal static void RestoreTime()
        {
            FreezeMatch = false;
            if (_controllingTime)
            {
                if (!PauseManager.isPaused) Time.timeScale = 1f;
                _controllingTime = false;
            }
            if (_timerFrozen)
            {
                if (NetUtil.IsServer && Run.instance != null) Run.instance.SetRunStopwatchPaused(false);
                _timerFrozen = false;
            }
            FreezeTimer = false;
        }

        internal override void OnUnload() => RestoreTime();

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("Time");
            FreezeMatch = Widgets.Toggle("Freeze Match (everything stops)", FreezeMatch);
            Widgets.Hint("Stops everything, including you. Toggle off to move. Host or solo.");

            FreezeTimer = Widgets.Toggle("Freeze Timer (difficulty)", FreezeTimer);
            Widgets.Hint("Stops the run clock so difficulty quits climbing. Enemies still move.");

            GUILayout.Space(4);
            TimeScale = Widgets.Slider("Time Scale", TimeScale, 0.1f, 3f);
            GUILayout.BeginHorizontal();
            Widgets.Button("Slow-mo (0.25x)", () => TimeScale = 0.25f);
            Widgets.Button("Normal (1x)", () => TimeScale = 1f);
            Widgets.Button("Fast (2x)", () => TimeScale = 2f);
            GUILayout.EndHorizontal();
            Widgets.SectionEnd();

            Widgets.SectionBegin("Safety");
            Safety.NoEnemies = Widgets.Toggle("No Enemies (kill on spawn)", Safety.NoEnemies);
            Safety.LockExp = Widgets.Toggle("Lock Experience", Safety.LockExp);
            Safety.PreventProfileWriting = Widgets.Toggle("Prevent Profile Saving", Safety.PreventProfileWriting);
            Widgets.Hint("Host or solo. Prevent Profile Saving keeps test runs out of your save file.");
            Widgets.SectionEnd();
        }
    }
}
