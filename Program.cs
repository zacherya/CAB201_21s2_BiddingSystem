using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using EBuy.InterfaceManagement;
using EBuy.InterfaceManagement.Interfaces;
using EBuy.Models;
using EBuy.Helpers;
using EBuy.DataProviders;

namespace EBuy
{
    public class Program
    {
        // The interface object currently being displayed
        private static Interface UserInterface { get; set; }

        // Flag whether or not the program is running or not to break loop
        public static bool programRunning = false;

        // Defining the data provider context objects to reference in application instances
        private static ClientDataProvider clientContext { get; set; }
        private static ProductDataProvider productContext { get; set; }

        /// <summary>
        /// The main execution point of the program
        /// </summary>
        /// <param name="args">Arugments passed through from before runtime</param>
        public static void Main(string[] args)
        {
            // Create new instances of the data providers for this current runtime
            clientContext = new ClientDataProvider();
            productContext = new ProductDataProvider();

            // Provide the client context to the Identity Controller for authentication
            IdentityController.ProvideContext(clientContext);

            // Run The Console Application Loop
            RunApplication(args);
        }

        /// <summary>
        /// THe console application loop that runs the program methods indefinetely until the user specifies other wise.
        /// A constant loop is run to achieve this. This is the main method of the program.
        /// Behavihour of the program is modified here
        /// </summary>
        /// <param name="args">Arugments passed through from before runtime</param>
        private static void RunApplication(string[] args)
        {

            // Flag that the program should stay running indefinitetly
            programRunning = true;

            // Try the following code and catch any exceptions caused by run time to avoid the application crashing
            try
            {
                // Loop through each arugment pass through at runtime and determine program functionality changes
                for (int i = 0; i < args.Length; i++)
                {
                    if ((args[i][0] == '/') || (args[i][0] == '-'))
                    {
                        string arg = args[i].Substring(1);
                        string larg = arg.ToLower();

                        // If argument d or demo is found, populate the data providers with relative demo data
                        if (larg.Equals("d") || larg.Equals("demo"))
                        {
                            // Populate demo users
                            clientContext.SetupDemoClients(); // Must be done first to establish users to assign to products
                            // Populate demo products
                            productContext.SetupDemoProducts(clientContext);
                        }

                    }
                }

                // Run a loop indefinitely dependant on the programRunning flag 
                while (programRunning)
                {
                    // Determine based on current authentication status provided by the identity controller
                    if (IdentityController.IsAuthenticated)
                    {
                        // User is logged in

                        // Assign the main application interface to the program interface context
                        // and provide the data source contexts it will use
                        UserInterface = new Application(clientContext, productContext);
                    }
                    else
                    {
                        // User is not logged in

                        // If the user interface is null (First run time)
                        // or the current user interface is of the type main application show the inital greeting.
                        // The typeof is included because the console history is cleared to protect the previous client session data
                        if (UserInterface == null || UserInterface.GetType() == typeof(Application))
                        {
                            // Tell the generic interface class to display a greeting (Not dependant on a application type)
                            // An interface is also not defined so it must use the base class non dependant on an objects presence
                            Interface.DisplayGreeting();
                        }
                        // Assign the authentication interface to the program interface context
                        // and provide the data source context it will use for authentication.
                        // This Interface does not require access to the product context as a user is not authenticated
                        UserInterface = new Authentication(clientContext);
                    }
                    // Tell the interface object to Display the menu of the interface.
                    // An override method from the base Interface class is used to keep the display menu function generic
                    UserInterface.DisplayMenu();
                }
            } catch (Exception ex)
            {
                // Catch all exceptions that occure and display the error caused by it for diagnositcs
                // Logging would usually occur here using the ILogger method
                Interface.DisplayCriticalError("An unexpected error has occured and caused the program to fail executing.");
                Interface.DisplayCriticalError($"Error: {ex.Message}");
                Interface.DisplayCriticalError("You'll need to restart the program to continue using it!", true);
            } finally
            {
                // This is where we finally wrap up the current programs data.
                // If the program made files that should be deleted after run time, it should be done here.
                // We are able to handle code even after an exception has occured and still cleanly exit the program (e.g stopping services)
                Interface.DisplayFarewell();
            }


            
        }
    }
}
