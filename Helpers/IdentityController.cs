using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EBuy.Models;
using EBuy.DataProviders;

namespace EBuy.Helpers
{
    public static class IdentityController
    {
        private static ClientDataProvider clientContext { get; set; }
        private static Client LoggedInUser { get; set; }

        /// <summary>
        /// Gets the current user object and returns it.
        /// Modifications can be made here before returned to the user.
        /// </summary>
        public static Client CurrentUser
        {
            get
            {
                return LoggedInUser;
            }
        }

        /// <summary>
        /// Simple method to determine if the user is logged in or not
        /// </summary>
        public static bool IsAuthenticated { 
            get {
                return LoggedInUser != null;
            } 
        }

        /// <summary>
        /// Provide the client context to the identity controller.
        /// Referenced to the inital object created on program initalisation.
        /// </summary>
        /// <param name="cdp">The client data provider context object</param>
        public static void ProvideContext(ClientDataProvider cdp)
        {
            //Assign to client context reference in Identity Provider class
            clientContext = cdp;
        }

        /// <summary>
        /// Log out the user by removing the client object from the reference point.
        /// This will trigger the IsAuthenticated method to return false
        /// </summary>
        public static void UnAuthenticateUser()
        {
            LoggedInUser = null;
        }

        /// <summary>
        /// Authenticate the users login attempt against the data source.
        /// Find matches of enabled users and attempt to match password with stored hash.
        /// </summary>
        /// <param name="email">The email at question to query against the data source</param>
        /// <param name="password">The password attempt to query against the hashed one in datasource</param>
        /// <returns>True or false depending if the user was found and if the password is a match</returns>
        public static bool AuthenticateUser(string email, string password)
        {
            try
            {
                IEnumerable<Client> matches = clientContext.GetUsers().Where(client => client.Email.ToLower() == email.ToLower() && client.Enabled);
                if (!matches.Any()) { return false; } // Any matches found with email provided, no then return false
                if (matches.Count() > 1) { throw new Exception("Deuplicate user in data source"); } // More then 1 match an error has occured, throw exception

                Client client = matches.First(); // Get the match

                var unHashed = SecurityHelper.UnHashString(client.Password); // Unhash password in datasource

                if (password == unHashed)
                {
                    // Passwords match
                    if(client.Enabled) { 
                        // Ensure client is enabled
                        LoggedInUser = client;
                        return true;
                    }
                }
            }
            catch { return false; }
            return false;
        }
    }
}
