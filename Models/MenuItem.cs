using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBuy.Models
{
    public class MenuItem
    {
        public int Position { get; private set; }
        public string Name { get; private set; }
        public Action OnSelection { get; private set; }

        public MenuItem(int position, string name, Action eventHandler)
        {
            Position = position;
            Name = name;
            OnSelection = eventHandler;
        }
        public MenuItem(MenuItem item, int newPosition)
        {
            Position = newPosition;
            Name = item.Name;
            OnSelection = item.OnSelection;
        }
    }
}
