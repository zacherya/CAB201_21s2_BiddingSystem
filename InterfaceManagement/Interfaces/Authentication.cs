using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EBuy.Models;
using EBuy.Helpers;
using EBuy.DataProviders;
using System.Threading;

namespace EBuy.InterfaceManagement.Interfaces
{
    public class Authentication : Interface
    {
        private ClientDataProvider clientContext { get; set; }

        /// <summary>
        /// Initalise the interface object with context parameters to access
        /// Compose the menu to the menu constructor
        /// </summary>
        /// <param name="cdp">The client data provider context the interface should interact with</param>
        public Authentication(ClientDataProvider cdp)
        {
            Console.Title = "EBuy - Welcome";
            clientContext = cdp;
            ComposeMenu();
        }

        /// <summary>
        /// Compose the menu items required for this interface and assign to the base class interface constructor
        /// </summary>
        public override void ComposeMenu()
        {
            // Construct the Menu Constructor with the basic exit application function
            List<MenuItem> itemsList = new List<MenuItem>();
            itemsList.Add(BasicExitItem);
            itemsList.Add(new MenuItem(1, "Register as a new Client", RegisterClient));
            itemsList.Add(new MenuItem(2, "Login as existing Client", LoginClient));
            InterfaceConstructor = new MenuConstructor(itemsList);
        }

        /// <summary>
        /// Manipulate the console interface to ask for input to register a new client in the current client context
        /// </summary>
        private void RegisterClient()
        {
            
            string firstName = GetInput("First name");
            string lastName = GetInput("Last name");
            string email = GetInput("Email");
            string rawPassword = GetPassword("Password");
            string address = GetInput("Address");

            try
            {
                Client newClient = new Client()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Password = rawPassword,
                    Email = email,
                    Address = address
                };
                clientContext.AddUser(newClient);
                DisplaySuccess($"{newClient.ToString()} registered successfully");
            } catch (InvalidOperationException ex)
            {
                DisplayError($"Failed to register {firstName} as a user, {ex.Message}");
            }
            catch (Exception ex)
            {
                DisplayError($"Failed to register {firstName} as a user, {ex.Message}");
            }
            
            
        }

        /// <summary>
        /// Manipulate the console interface to authenticate against the current client context
        /// </summary>
        private void LoginClient()
        {
            string email = GetInput("Email");
            string password = GetPassword("Password");

            if(IdentityController.AuthenticateUser(email,password)) {
                DisplaySuccess($"\nWelcome {IdentityController.CurrentUser.ToString()}");
                
                //Program.UserInterface = new Application();
            } else { 
                DisplayError("The email or password is incorrect");
            }
        }
    }
}
