using System.Collections.Generic;
using RoR2;

namespace PoppyMenu
{
    internal class ItemsModule : PoppyModule
    {
        internal override string Name => "Items";

        internal static bool NoEquipmentCooldown;
        internal static int GiveCount = 1;

        internal override void DrawMenu()
        {
            Widgets.SectionBegin("Give");
            GiveCount = Widgets.IntStepper("Count", GiveCount, 1, 1, 1000);

            Widgets.Button("Give Item...", () =>
                ItemPicker.Open("Items", idx => NetUtil.Do(PoppyOp.GiveItem, (int)idx, GiveCount)));

            var equipRows = new List<ListPicker.Row>(Catalogs.Equipment.Count);
            foreach (var entry in Catalogs.Equipment)
            {
                var e = entry;
                equipRows.Add(new ListPicker.Row(
                    e.Name, e.Color,
                    () => NetUtil.Do(PoppyOp.GiveEquipment, (int)e.Index)));
            }
            Widgets.PickerButton("Give Equipment...", "Equipment", equipRows);
            Widgets.Button($"Give All Items (x{GiveCount})", () => NetUtil.Do(PoppyOp.GiveAllItems, i2: GiveCount));
            Widgets.SectionEnd();

            Widgets.SectionBegin("Inventory");
            Widgets.Button("Stack (Shrine of Order)", () => NetUtil.Do(PoppyOp.StackInventory));
            Widgets.Button("Reroll Items", () => NetUtil.Do(PoppyOp.RollItems));
            Widgets.Button("Undo Last Item Change", () => NetUtil.Do(PoppyOp.UndoInventory));
            NoEquipmentCooldown = Widgets.Toggle("No Equipment Cooldown", NoEquipmentCooldown);
            Widgets.ConfirmButton("items.clearinv", "Clear Inventory", () => NetUtil.Do(PoppyOp.ClearInventory));
            Widgets.SectionEnd();
        }

        internal override void Tick()
        {
            if (!NoEquipmentCooldown || !NetUtil.IsServer || !PlayerContext.HasBody)
                return;

            EquipmentSlot slot = PlayerContext.Body.equipmentSlot;
            if (slot == null || slot.equipmentIndex == EquipmentIndex.None)
                return;

            int max = slot.maxStock > 0 ? slot.maxStock : 1;
            if (slot.stock < max)
                slot.stock = max;
        }
    }
}
