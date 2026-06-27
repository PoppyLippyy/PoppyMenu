using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal class RunModule : PoppyModule
    {
        internal override string Name => "Run";

        private static int _stages;
        private static int _minutes;
        private static int _teamLevel = 1;

        internal override void DrawMenu()
        {
            if (!PlayerContext.InGame || Run.instance == null) { Widgets.Label("Start a run first."); return; }

            Run run = Run.instance;

            Widgets.SectionBegin("Run State");
            Widgets.Label($"Stage {run.stageClearCount + 1}   ·   {FormatClock(run.GetRunStopwatch())}   ·   difficulty {run.difficultyCoefficient:0.0}");

            _stages = Widgets.IntStepper("Stages cleared", _stages, 1, 0, 200);
            Widgets.Button("Set stages cleared", () => NetUtil.Do(PoppyOp.SetStagesCleared, i1: _stages));

            _minutes = Widgets.IntStepper("Run time (minutes)", _minutes, 1, 0, 600);
            Widgets.Button("Set run time", () => NetUtil.Do(PoppyOp.SetRunTime, f1: _minutes * 60f));
            Widgets.Hint("Run time drives difficulty. Freeze it under World > Time to lock it.");
            Widgets.SectionEnd();

            Widgets.SectionBegin("Team Level");
            _teamLevel = Widgets.IntStepper("Player team level", _teamLevel, 1, 1, 99999);
            Widgets.Button("Set team level", () => NetUtil.Do(PoppyOp.SetTeamLevel, i1: (int)TeamIndex.Player, i2: _teamLevel));
            Widgets.SectionEnd();

            Widgets.SectionBegin("Change Stage");
            Widgets.PickerButton("Go to scene...", "Scenes", SceneRows());
            Widgets.SectionEnd();

            DrawArtifacts();
        }

        private static List<ListPicker.Row> SceneRows()
        {
            var rows = new List<ListPicker.Row>();
            var defs = SceneCatalog.allSceneDefs;
            foreach (SceneDef def in defs)
            {
                if (def == null) continue;
                SceneDef d = def;
                string name = Language.GetString(d.nameToken);
                if (string.IsNullOrEmpty(name) || name == d.nameToken) name = d.cachedName;
                rows.Add(new ListPicker.Row(name, Color.white, () => NetUtil.Do(PoppyOp.ChangeScene, i1: (int)d.sceneDefIndex)));
            }
            return rows;
        }

        private static void DrawArtifacts()
        {
            Widgets.SectionBegin("Artifacts");
            ArtifactDef[] defs = ArtifactCatalog.artifactDefs;
            RunArtifactManager mgr = RunArtifactManager.instance;
            if (defs == null || mgr == null) { Widgets.Hint("Artifacts aren't available right now."); Widgets.SectionEnd(); return; }

            foreach (ArtifactDef def in defs)
            {
                if (def == null) continue;
                bool on = mgr.IsArtifactEnabled(def);
                bool now = Widgets.Toggle(ArtifactName(def), on);
                if (now != on) NetUtil.Do(PoppyOp.SetArtifact, i1: (int)def.artifactIndex, b1: now);
            }
            Widgets.SectionEnd();
        }

        private static string ArtifactName(ArtifactDef def)
        {
            string name = Language.GetString(def.nameToken);
            if (string.IsNullOrEmpty(name) || name == def.nameToken) name = def.cachedName;
            return name;
        }

        private static string FormatClock(float seconds)
        {
            int s = Mathf.Max(0, (int)seconds);
            return (s / 60) + ":" + (s % 60).ToString("00");
        }
    }
}
