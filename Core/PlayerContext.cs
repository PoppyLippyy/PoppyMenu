using RoR2;
using UnityEngine;

namespace PoppyMenu
{
    internal static class PlayerContext
    {
        internal static NetworkUser User;
        internal static CharacterMaster Master;
        internal static CharacterBody Body;
        internal static Inventory Inventory;
        internal static HealthComponent Health;
        internal static SkillLocator Skills;
        internal static CharacterMotor Motor;
        internal static InputBankTest InputBank;

        internal static bool InGame => Run.instance != null;

        internal static bool HasBody => Master != null && Body != null;

        internal static void Refresh()
        {
            User = null;
            Master = null;
            Body = null;
            Inventory = null;
            Health = null;
            Skills = null;
            Motor = null;
            InputBank = null;

            if (!InGame)
                return;

            foreach (NetworkUser nu in NetworkUser.readOnlyLocalPlayersList)
            {
                if (nu == null)
                    continue;

                User = nu;
                Master = nu.master;
                if (Master == null)
                    continue;

                Body = Master.GetBody();
                if (Body == null)
                    continue;

                Inventory = Master.inventory;
                Health = Body.healthComponent;
                Skills = Body.skillLocator;
                Motor = Body.GetComponent<CharacterMotor>();
                InputBank = Body.GetComponent<InputBankTest>();
                break;
            }
        }

        internal static Ray AimRay()
        {
            if (InputBank != null)
                return new Ray(InputBank.aimOrigin, InputBank.aimDirection);
            Camera cam = Camera.main;
            return cam != null ? cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f)) : default;
        }
    }
}
