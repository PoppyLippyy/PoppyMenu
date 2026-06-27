using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal class PlayersModule : PoppyModule
    {
        internal override string Name => "Players";

        private static int _hurt = 100;
        private static int _giveCount = 1;

        internal override void DrawMenu()
        {
            if (!PlayerContext.InGame) { Widgets.Label("Start a run first."); return; }

            var users = NetworkUser.readOnlyInstancesList;
            if (users == null || users.Count == 0) { Widgets.Label("No players in the lobby."); return; }

            if (!NetUtil.IsServer)
                Widgets.Hint("You're a client, so these go to the host and only apply if the host turned on \"Allow Others To Use This\".");

            _hurt = Widgets.IntStepper("Hurt amount", _hurt, 50, 1, 1000000000);
            _giveCount = Widgets.IntStepper("Give item count", _giveCount, 1, 1, 100);

            foreach (NetworkUser user in users)
                if (user != null) DrawPlayer(user);
        }

        private static void DrawPlayer(NetworkUser user)
        {
            CharacterMaster master = user.master;
            bool isLocal = NetworkUser.readOnlyLocalPlayersList.Contains(user);

            Widgets.SectionBegin(user.userName + (isLocal ? "  (you)" : ""));

            CharacterBody body = master != null ? master.GetBody() : null;
            if (body != null && body.healthComponent != null)
                Widgets.Label($"{body.GetDisplayName()}   ·   {Mathf.CeilToInt(body.healthComponent.health)} / {Mathf.CeilToInt(body.healthComponent.fullCombinedHealth)} HP");
            else
                Widgets.Label("No body (dead or not spawned)");

            if (!isLocal && NetUtil.IsServer)
            {
                GUILayout.BeginHorizontal();
                Widgets.Button("Kick", () => NetUtil.KickUser(user));
                Widgets.ConfirmButton("players.ban." + user.userName, "Ban", () => NetUtil.BanUser(user));
                GUILayout.EndHorizontal();
            }

            if (master == null) { Widgets.SectionEnd(); return; }

            GUILayout.BeginHorizontal();
            Widgets.Button("Heal", () => NetUtil.DoFor(master, PoppyOp.HealFull));
            Widgets.Button("Revive", () => NetUtil.DoFor(master, PoppyOp.Respawn));
            Widgets.Button("Hurt " + _hurt, () => NetUtil.DoFor(master, PoppyOp.HurtBody, f1: _hurt));
            Widgets.Button("Kill", () => NetUtil.DoFor(master, PoppyOp.TrueKillTarget));
            GUILayout.EndHorizontal();

            if (!isLocal)
            {
                GUILayout.BeginHorizontal();
                Widgets.Button("Bring to me", () =>
                {
                    Vector3 p = MyPos();
                    NetUtil.DoFor(master, PoppyOp.TeleportBody, f1: p.x, f2: p.y, f3: p.z);
                });
                Widgets.Button("Go to them", () =>
                {
                    CharacterBody b = master.GetBody();
                    Vector3 p = b != null ? b.footPosition : MyPos();
                    NetUtil.DoFor(PlayerContext.Master, PoppyOp.TeleportBody, f1: p.x, f2: p.y, f3: p.z);
                });
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            Widgets.Button("Team: Players", () => NetUtil.DoFor(master, PoppyOp.SetTeam, i1: (int)TeamIndex.Player));
            Widgets.Button("Monsters", () => NetUtil.DoFor(master, PoppyOp.SetTeam, i1: (int)TeamIndex.Monster));
            Widgets.Button("Neutral", () => NetUtil.DoFor(master, PoppyOp.SetTeam, i1: (int)TeamIndex.Neutral));
            GUILayout.EndHorizontal();

            CharacterMaster target = master;
            Widgets.Button("Give item", () =>
                ItemPicker.Open("Give item to " + user.userName, idx => NetUtil.DoFor(target, PoppyOp.GiveItem, i1: (int)idx, i2: _giveCount)));
            Widgets.PickerButton("Set body", "Set " + user.userName + "'s body", SetBodyRows(master));

            Widgets.SectionEnd();
        }

        private static List<ListPicker.Row> SetBodyRows(CharacterMaster master)
        {
            var rows = new List<ListPicker.Row>(Catalogs.Bodies.Count);
            foreach (Catalogs.BodyEntry entry in Catalogs.Bodies)
            {
                if (entry.Prefab == null) continue;
                Catalogs.BodyEntry e = entry;
                rows.Add(new ListPicker.Row(e.Name, Color.white, () =>
                {
                    CharacterBody b = master.GetBody();
                    Vector3 p = b != null ? b.footPosition : Vector3.zero;
                    NetUtil.DoFor(master, PoppyOp.ChangeBody, f1: p.x, f2: p.y, f3: p.z, s1: e.Prefab.name);
                }));
            }
            return rows;
        }

        private static Vector3 MyPos()
        {
            CharacterBody b = PlayerContext.Body;
            return b != null ? b.footPosition : Vector3.zero;
        }
    }
}
