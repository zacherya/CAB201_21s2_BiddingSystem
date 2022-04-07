using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBuy.Models
{
    public class Bid
    {
        public int Amount { get; private set; }
        public Client Bidder { get; set; }
        public bool HomeDelivery { get; private set; }

        public Bid(Bid existingBid)
        {
            Amount = existingBid.Amount;
            Bidder = existingBid.Bidder;
            HomeDelivery = existingBid.HomeDelivery;
        }
        public Bid(int bidAmount, Client customer)
        {
            Bidder = customer;
            Amount = bidAmount;
            HomeDelivery = false;
        }
        public Bid(int bidAmount, Client customer, bool homeDelivery)
        {
            Bidder = customer;
            Amount = bidAmount;
            HomeDelivery = homeDelivery;
        }

    }
}
