using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBuy.Models
{
    public class Client
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName {
            get {
                return this.FirstName +" "+ this.LastName;
            }
        }

        public string Password { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool Enabled { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public Client()
        {

        }
        public Client(Client obj)
        {
            FirstName = obj.FirstName;
            LastName = obj.LastName;
            Password = obj.Password;
            Email = obj.Email;
            Address = obj.Address;
        }

        public override string ToString()
        {
            return $"{FullName} ({Email})";
        }

    }

}
