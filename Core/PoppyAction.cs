using System;

namespace PoppyMenu
{
    internal class PoppyAction
    {
        internal readonly string Id;
        internal readonly string Category;
        internal readonly string Name;
        internal readonly Action Invoke;

        internal PoppyAction(string id, string category, string name, Action invoke)
        {
            Id = id; Category = category; Name = name; Invoke = invoke;
        }
    }
}
