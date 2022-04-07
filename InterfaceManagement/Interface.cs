using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EBuy.Helpers;
using EBuy.InterfaceManagement;
using EBuy.InterfaceManagement.Interfaces;
using EBuy.Models;

namespace EBuy.InterfaceManagement
{
    public class Interface
    {
        protected MenuConstructor InterfaceConstructor;
        protected MenuItem BasicExitItem;
        public Interface() {
            // Define the basic exit item that may be used accross multiple future interfaces
            BasicExitItem = new MenuItem(999, "Exit", ExitProgram);
        }

        // Method definition for Interface Modules
        /// <summary>
        /// Arugment to be overriden by all sub class of this parent class. 
        /// If a new interface with no compose menu method defined an expception will be thrown to the developer
        /// </summary>
        public virtual void ComposeMenu() { throw new NotSupportedException(); }

        /// <summary>
        /// Display the menu that has been constructed with the interface constructor from a sub class.
        /// </summary>
        public void DisplayMenu()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("Please select one of the following: ");
                InterfaceConstructor.PrintToConsole();

                //wait for user input
                int choice;
                if (AskForChoice(InterfaceConstructor.ItemCount(), out choice))
                {
                    // Retrieve the corresponding menu item
                    InterfaceConstructor.ExecuteActionFor(choice);
                }
            } catch (Exception ex)
            {
                DisplayError($"An unexpected internal error has occured: {ex.Message}");
            }
        }

        /// <summary>
        /// Generic interface function to manipulate the console application to ask for a non escapable choice with an index.
        /// </summary>
        /// <param name="maxChoices">Max amount options in the list (Limits valid choices from 1 to this number)</param>
        /// <param name="result">Pass an argument to the method by reference rather than by value</param>
        /// <param name="prompt">The prompt that the choice will use, by default will use generic word "Choice"</param>
        /// <returns></returns>
        public static bool AskForChoice(int maxChoices, out int result, string prompt = "Choice")
        {
            // Store previous console display configuration
            ConsoleColor prevConsoleFg = Console.ForegroundColor;
            ConsoleColor prevConsoleBg = Console.BackgroundColor;

            // Set new configuration for choice word
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;

            // Print the choice prompt
            Console.Write("{0}:", prompt);

            // Restore previous configuration for user entry
            Console.ForegroundColor = prevConsoleFg;
            Console.BackgroundColor = prevConsoleBg;
            // Write a gap between the prompt and the user input
            Console.Write(" ");

            // Read the user input
            string choice = Console.ReadLine();
            // Declaire the reference point for the choice
            int choiceIndex;

            // Attempt to validate the choice as an integer and determine if choice is within the boundaries defined
            if (!int.TryParse(choice, out choiceIndex) || choiceIndex < 1 || choiceIndex > maxChoices)
            {
                // Write a space between choice and error
                Console.WriteLine();

                // Display error message with trailing try again
                DisplayError("Invalid selection made",true);
                // Set result to 0 and return an unsuccessful flag
                result = 0;
                return false;
            }
            else
            {
                // Write a space between choice and next method content
                Console.WriteLine();
                // Set the user choice and return a successful flag
                result = choiceIndex;
                return true;
            }
        }

        /// <summary>
		/// Gets a single line of text from user. 
		/// </summary>
		/// <param name="prompt">Text used to ask the user for input.</param>
		/// <param name="nextLine">Used when the function needs to display an error on the same line from a parent function</param>
		/// <returns>Returns a single line of text from user. Will not return until input is made.</returns>
        /// <reference>Copied from the provided User Interface class but modified to fit application/method purpose</reference>
		public static string GetInput(string prompt, bool nextLine = true, bool allowNoInput = false)
        {
            Console.Write("{0}: ", prompt);
            StringBuilder input = new System.Text.StringBuilder();

            while (true)
            {
                var keyInfo = Console.ReadKey(intercept: true);
                var key = keyInfo.Key;

                if (key == ConsoleKey.Enter)
                    if (string.IsNullOrWhiteSpace(input.ToString()) && !allowNoInput)
                    {
                        //Console.WriteLine("".PadLeft(3) + $" | You must enter valid input for {prompt}");
                        DisplayInlineError($"You must enter valid input for {prompt}",true);
                        Console.Write("{0}: ", prompt);
                    }
                    else
                    {
                        break;
                    }
                else if (key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        Console.Write("\b \b");
                        input.Remove(input.Length - 1, 1);
                    }
                }
                else if (key == ConsoleKey.Escape)
                {
                    // Allow the user to cancel the input operation
                    throw new OperationCanceledException($"Operation Cancelled!");
                }
                else
                {
                    Console.Write(keyInfo.KeyChar);
                    input.Append(keyInfo.KeyChar);
                }
            }

            if(nextLine) Console.WriteLine();
            return input.ToString();
        }

        /// <summary>
        /// Get a single yes or no prompt from the user
        /// </summary>
        /// <param name="prompt">Text used to ask the user for input.</param>
        /// <returns>The yes or no prompt as a boolean value</returns>
        /// <reference>Copied from the provided User Interface class but modified to fit application/method purpose</reference>
        public static bool GetBoolean(string prompt)
        {
            Console.Write("{0} (Y/N): ", prompt);
            bool responseValue = false;

            while (true)
            {
                var keyInfo = Console.ReadKey(intercept: true);
                var key = keyInfo.Key;

                if (key == ConsoleKey.Escape)
                {
                    // Allow the user to cancel the input operation
                    throw new OperationCanceledException($"Operation Cancelled!");
                }
                else
                {
                    if(keyInfo.KeyChar.ToString().ToLower() == "y")
                    {
                        Console.Write("YES");
                        responseValue = true;
                        break;
                    }else if (keyInfo.KeyChar.ToString().ToLower() == "n")
                    {
                        Console.Write("NO");
                        responseValue = false;
                        break;
                    }
                }
            }

            Console.WriteLine();
            return responseValue;
        }

        /// <summary>
        /// Modifies the UI to display input as hidden values (Disguise input)
        /// </summary>
        /// <param name="prompt">Text used to ask the user for input.</param>
        /// <returns>A built string of the hidden input</returns>
        /// <reference>Copied from the provided User Interface class but modified to fit application/method purpose</reference>
        public static string GetPassword(string prompt)
        {
            Console.Write("{0}: ", prompt);
            StringBuilder password = new System.Text.StringBuilder();

            while (true)
            {
                var keyInfo = Console.ReadKey(intercept: true);
                var key = keyInfo.Key;

                if (key == ConsoleKey.Enter)
                    if(string.IsNullOrWhiteSpace(password.ToString()))
                    {
                        //Console.WriteLine("".PadLeft(3) + $" | Invalid password entered, try again.");
                        DisplayInlineError("Invalid password entered", true);
                        Console.Write("{0}: ", prompt);
                    } else
                    {
                        break;
                    }
                else if (key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        Console.Write("\b \b");
                        password.Remove(password.Length - 1, 1);
                    }
                }
                else if (key == ConsoleKey.Escape)
                {
                    // Allow the user to cancel the input operation
                    throw new OperationCanceledException($"Operation Cancelled!");
                }
                else
                {
                    Console.Write("*");
                    password.Append(keyInfo.KeyChar);
                }
            }

            Console.WriteLine();
            return password.ToString();
        }

        /// <summary>
		/// Gets a validated integer.
		/// </summary>
		/// <param name="prompt">Text used to ask the user for input.</param>
		/// <returns>An integer supplied by the user.</returns>
        /// <reference>Copied from the provided User Interface class but modified to fit application/method purpose</reference>
		public static int GetInt(string prompt)
        {
            while (true)
            {
                string response = GetInput(prompt,false);

                int result;

                if (int.TryParse(response, out result))
                {
                    Console.WriteLine();
                    return result;
                }
                else
                {
                    //Console.WriteLine("".PadLeft(3) + " | Supplied value is not an integer, try again.");
                    DisplayInlineError("Supplied value is not an integer", true);
                }
            }
            
        }

        /// <summary>
        /// Displays a generic success message to the console in green with padding below
        /// </summary>
        /// <param name="msg">The message to display to the console</param>
        public static void DisplaySuccess(string msg)
        {
            ConsoleColor previousConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{msg}");
            Console.ForegroundColor = previousConsoleColor;
            Console.WriteLine();
        }

        /// <summary>
        /// Displays a generic warning message to the console in yellow/orange with padding below
        /// </summary>
        /// <param name="msg">The message to display to the console</param>
        /// <param name="noNextLine">Indicate whether or not there will be direct content below the warning</param>
        public static void DisplayWarning(string msg, bool nextLine = true)
        {
            ConsoleColor previousConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{msg}");
            Console.ForegroundColor = previousConsoleColor;
            if(nextLine) Console.WriteLine();
        }

        /// <summary>
        /// Displays a generic error message to the console in red with padding below
        /// </summary>
        /// <param name="msg">The message to display to the console</param>
        /// <param name="trailingTryAgain">To determine if the error a plausable cause and can be retried</param>
        public static void DisplayError(string msg, bool trailingTryAgain = false)
        {
            ConsoleColor previousConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{msg}" + (trailingTryAgain ? ", try again." : "."));
            Console.ForegroundColor = previousConsoleColor;
            Console.WriteLine();
        }

        /// <summary>
        /// Displays a generic error message to the console in white with red backrgound + padding above and below to convery severity
        /// </summary>
        /// <param name="msg">The message to display to the console</param>
        /// <param name="breakBefore">Add padding before to separate critical message from previous content</param>
        public static void DisplayCriticalError(string msg, bool breakBefore = false)
        {
            if(breakBefore) Console.WriteLine();
            ConsoleColor prevConsoleFg = Console.ForegroundColor;
            ConsoleColor prevConsoleBg = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;

            Console.WriteLine($"{msg}");

            Console.ForegroundColor = prevConsoleFg;
            Console.BackgroundColor = prevConsoleBg;
        }

        /// <summary>
        /// Displays a generic error message to the console in red on the same line the error has occured in
        /// Used primarily in asking for input
        /// </summary>
        /// <param name="msg">The message to display to the console</param>
        /// <param name="trailingTryAgain">To determine if the error a plausable cause and can be retried</param>
        public static void DisplayInlineError(string msg, bool trailingTryAgain=false)
        {
            ConsoleColor previousConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("".PadLeft(3) + $" | {msg}"+(trailingTryAgain ? ", try again.":"."));
            Console.ForegroundColor = previousConsoleColor;
        }

        /// <summary>
        /// Display a welcome greeting of the application to the user (Technical interlude)
        /// </summary>
        public static void DisplayGreeting()
        {
            ConsoleColor prevConsoleFg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("====================================");
            Console.WriteLine("Welcome to EBuy!");
            Console.WriteLine($"Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            Console.WriteLine("====================================");
            Console.WriteLine($"By: Zachery Adams (n10772693)");
            Console.WriteLine("====================================");
            Console.ForegroundColor = prevConsoleFg;
        }

        /// <summary>
        /// Displays the final fairwell message to the user before the applications finally exits
        /// </summary>
        public static void DisplayFarewell()
        {
            ConsoleColor prevConsoleFg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("====================================");
            Console.WriteLine("Goodbye :)");
            Console.WriteLine("====================================");
            Console.ForegroundColor = prevConsoleFg;
        }

        /// <summary>
        /// Convert a numeric value to a currency string like $5.00.
        /// Overload to allow int and double to be used
        /// </summary>
        /// <param name="value">The numeric value in question of type int or double</param>
        /// <returns>The value passed through as a currency value string </returns>
        public static string ToCurrency(int value)
        {
            return ToCurrency((double)value);

        }
        public static string ToCurrency(double value)
        {
            return (value).ToString("C");

        }
        /// <summary>
        /// General interface exit program method, safe exit procedures would be places here to ensure the program has finished.
        /// </summary>
        protected static void ExitProgram()
        {
            //if(harshExit) System.Environment.Exit(1);
            Program.programRunning = false; //Exit Gracefully
        }

    }
}
