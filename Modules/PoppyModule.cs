namespace PoppyMenu
{
    internal abstract class PoppyModule
    {
        internal abstract string Name { get; }

        internal virtual void Tick() { }

        internal virtual void DrawMenu() { }

        internal virtual void DrawOverlay() { }

        internal virtual void OnUnload() { }
    }
}
