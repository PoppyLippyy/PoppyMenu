using RoR2;

namespace PoppyMenu
{
    internal class TeleporterModule : PoppyModule
    {
        internal override string Name => "Teleport";

        internal override void DrawMenu()
        {
            if (!PlayerContext.InGame) { Widgets.Label("Start a run first."); return; }

            Widgets.SectionBegin("Teleporter");
            TeleporterInteraction tp = TeleporterInteraction.instance;
            Widgets.Label(tp != null ? "Charge: " + tp.chargePercent + "%" : "No teleporter on this stage.");
            Widgets.Button("Instant Charge", () => NetUtil.Do(PoppyOp.ChargeTeleporter));
            Widgets.Button("Skip Stage", () => NetUtil.Do(PoppyOp.SkipStage));
            Widgets.Button("Add Mountain Shrine Stack", () => NetUtil.Do(PoppyOp.AddMountainShrine));
            Widgets.SectionEnd();

            Widgets.SectionBegin("Portals");
            Widgets.Button("Blue (Shop) Portal", () => NetUtil.Do(PoppyOp.SpawnShopPortal));
            Widgets.Button("Gold Portal", () => NetUtil.Do(PoppyOp.SpawnGoldshoresPortal));
            Widgets.Button("Celestial Portal", () => NetUtil.Do(PoppyOp.SpawnMSPortal));
            Widgets.PrimaryButton("Spawn All Portals", () =>
            {
                NetUtil.Do(PoppyOp.SpawnShopPortal);
                NetUtil.Do(PoppyOp.SpawnGoldshoresPortal);
                NetUtil.Do(PoppyOp.SpawnMSPortal);
            });
            Widgets.SectionEnd();
        }
    }
}
