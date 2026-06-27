using System.Collections.Generic;

namespace PoppyMenu
{
    internal class TabGroup
    {
        internal readonly string Name;
        internal readonly List<PoppyModule> Pages;
        internal int Page;

        internal TabGroup(string name, params PoppyModule[] pages)
        {
            Name = name;
            Pages = new List<PoppyModule>(pages);
        }
    }
}
