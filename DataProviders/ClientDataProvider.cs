using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EBuy.Models;
using EBuy.Helpers;

namespace EBuy.DataProviders
{
    public class ClientDataProvider : BaseDataProvider<Client>
    {

        /// <summary>
        /// Manipulate the new user information for privacy and check if they already exist.
        /// Middleware to intercept incoming users before adding to the data source. 
        /// </summary>
        /// <param name="newUser">An instance of Client populated with client details to register into the system</param>
        public void AddUser(Client newUser)
        {
            if (GetEntities().Where(clt => clt.Email == newUser.Email).Any())
            {
                throw new InvalidOperationException("A user with that email already exists");
            }
            newUser.Password = SecurityHelper.HashString(newUser.Password);
            AddEntity(newUser);
        }

        /// <summary>
        /// Fetch users from the datasource and apply filters for certain users
        /// </summary>
        /// <param name="userFilter">The list of users to pass into the filter</param>
        /// <param name="filterOut">Specifies whether or not the filter is inclusive or exclusive</param>
        /// <returns>Returns the list of clients from the datasource with the specified filter applied</returns>
        public IEnumerable<Client> GetUsers(List<Client> userFilter = null, bool filterOut = true)
        {
            //Avoid exceptions due to no seller filter
            userFilter = userFilter ?? new List<Client>();

            IEnumerable<Client> filteredClients;
            if (filterOut)
            {
                // Filter exclusive
                filteredClients = GetEntities().Where(client => !userFilter.Contains(client));
            }
            else
            {
                // Filter inclusive
                filteredClients = GetEntities().Where(client => userFilter.Contains(client));
            }

            // If data source has data then return filtered results else return empty list of clients
            return HasData ? filteredClients.OrderByDescending(client => client.FullName) : new List<Client>();
        }

        /// <summary>
        /// Populate the datasource with random users for demo purpeses
        /// </summary>
        public void SetupDemoClients()
        {
            string[] firstNames = { "Bob", "Jane", "Mark", "Sally", "Zoe", "Zac", "Fiona", "David" };
            string[] lastNames = { "Adams", "Freeman", "Edwards", "Eagles", "Parascos", "Silverman" };

            for (var i = 0; i < 8; i++)
            {
                Random rnd = new Random();

                Client demoUserToAdd = new Client();
                demoUserToAdd.FirstName = firstNames[i];
                demoUserToAdd.LastName = lastNames[rnd.Next(0, lastNames.Count())];
                demoUserToAdd.Address = $"{i} Demo Street, Brisbane QLD 4000, Australia";
                demoUserToAdd.Password = "Pass123$";
                demoUserToAdd.Email = $"{demoUserToAdd.FirstName.ToLower()}@email.com";
                AddUser(demoUserToAdd);

            }
        }
    }
}
