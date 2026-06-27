using System.Collections.Generic;

namespace PoppyMenu
{
    internal class Preset
    {
        public string Name = "New Preset";
        public bool AutoApplyOnSpawn;
        public bool LoadOnStartup;

        public bool God, Skills, Flight, Sprint, JumpPack, NoEquipCd;
        public bool SilentAim;
        public bool EspMobs, EspInteractables, EspTeleporter;

        public bool DmgOn, AtkOn, MoveOn, ArmorOn, CritOn, HpOn;
        public float DmgMul = 1f, AtkMul = 1f, MoveMul = 1f, ArmorMul = 1f, CritMul = 1f, HpMul = 1f;

        public List<GrantItem> Items = new List<GrantItem>();
        public List<string> Equipment = new List<string>();
        public bool GiveAllItems;
        public int Money, Xp, Coins;
    }

    internal class GrantItem
    {
        public string Name;
        public string Display;
        public int Count = 1;
    }
}
