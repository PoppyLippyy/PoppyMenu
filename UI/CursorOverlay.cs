using UnityEngine;

namespace PoppyMenu
{
    internal class CursorOverlay : MonoBehaviour
    {
        private void OnGUI()
        {
            GUI.depth = -1000;
            if (MenuRoot.Visible || ListPicker.IsOpen)
                SoftCursor.Draw();
        }

        private void LateUpdate()
        {
            if (MenuRoot.Visible || ListPicker.IsOpen)
                Cursor.lockState = CursorLockMode.None;
        }
    }
}
