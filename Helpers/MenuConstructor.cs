using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EBuy.Models;

namespace EBuy.Helpers
{
    public class MenuConstructor
    {
        private static ILookup<int, MenuItem> itemsInMenu;

        /// <summary>
        /// Create an instance of the Menu Constructor with a predefined list of menu items
        /// </summary>
        /// <param name="menuItems">List of menu items to pass to the constructor </param>
        public MenuConstructor(List<MenuItem> menuItems)
        {
            SetMenuItems(menuItems, true);
        }

        /// <summary>
        /// Create an instance of the Menu Constructor with a predefined singlar menu item
        /// </summary>
        /// <param name="menuItem">Single menu item to pass to the constructor</param>
        public MenuConstructor(MenuItem menuItem)
        {
            // Convert singlar item into a list of menu items
            List<MenuItem> menuItems = new List<MenuItem>();
            menuItems.Add(menuItem); // add the item to the temp list

            SetMenuItems(menuItems); // pass the list to the set menu items to construct into a lookup
        }

        /// <summary>
        /// Get the number of items currently in the menu
        /// </summary>
        /// <returns>The current number of menu items</returns>
        public int ItemCount()
        {
            return itemsInMenu.Count();
        }

        /// <summary>
        /// Indent a string by a certain padding
        /// </summary>
        /// <param name="count">The left padding for the text to be indented by</param>
        /// <returns>The a string with indented white spaces of the total specified length</returns>
        private static string Indent(int count)
        {
            return "".PadLeft(count);
        }

        /// <summary>
        /// Print the menu items to the console
        /// </summary>
        public void PrintToConsole()
        {
            foreach (MenuItem item in itemsInMenu.OrderBy(obj => obj.Key).Select(obj => obj.First()))
            {
                ConsoleColor prevConsoleFg = Console.ForegroundColor;
                ConsoleColor prevConsoleBg = Console.BackgroundColor;

                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.DarkGray;

                Console.Write(Indent(2) + item.Position + ")");

                Console.ForegroundColor = prevConsoleFg;
                Console.BackgroundColor = prevConsoleBg;

                Console.WriteLine(" " + item.Name);
            }
        }

        /// <summary>
        /// Primary function to set/replace the master menu item lookup, accepts a list of MenuItems and maps to the position
        /// </summary>
        /// <param name="lookupDictonary">A list of menu items to set into the Constructor class lookup list</param>
        /// <param name="reOrder">false by default to ensure reordering is only called if needed</param>
        private void SetMenuItems(List<MenuItem> lookupDictonary, bool reOrder = false)
        {
            itemsInMenu = lookupDictonary.ToLookup(p => p.Position, p => p);
            if (reOrder) ReStructureOrder();
        }

        /// <summary>
        /// The ReStructureOrder method simply ensures all elements take on a acesending numeric form with no outliers.
        /// For example, if there are 5 menu items and 4/5 were give an appropriate position of 1-4 but one outlyer 
        ///     existed with a postion of 999 to ensure it was the last element, it will be reorganised to have an
        ///     appropriate position number.
        /// </summary>
        private void ReStructureOrder()
        {
            List<MenuItem> tmpReorder = new List<MenuItem>();
            int index = 1;
            foreach (MenuItem item in itemsInMenu.OrderBy(obj => obj.Key).Select(obj=>obj.First()))
            {
                MenuItem tmpItem = new MenuItem(item,index);
                tmpReorder.Add(tmpItem);
                index++;
            }
            SetMenuItems(tmpReorder);
        }
        /// <summary>
        /// Invoke the Action method defined in the menu item.
        /// </summary>
        /// <param name="number">The lookup number (Position) of the menu item to execute</param>
        public void ExecuteActionFor(int number)
        {
            try
            {
                MenuItem itemBeingCalled = itemsInMenu[number].First();
                itemBeingCalled.OnSelection.Invoke();
            }
            catch (OperationCanceledException) {
                Console.WriteLine();

                ConsoleColor prevBg = Console.BackgroundColor;
                ConsoleColor prevTxt = Console.ForegroundColor;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;

                Console.Write($"!! Operation Cancelled !!");

                Console.BackgroundColor = prevBg;
                Console.ForegroundColor = prevTxt;

                Console.WriteLine();
                Console.WriteLine();
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException("The menu item that was selected or initally displayed doesn't exist inside the application context. Check all constructor references point to the same lookup method.");
            } catch (Exception ex)
            {
                throw new Exception($"An error has occured whilst invoking the menu action ({ex.Message})");
            }
            
        }
    }
}
