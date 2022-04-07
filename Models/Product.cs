using EBuy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBuy.Models
{
    public class Product
    {
        public string Description { get; set; }
        public string Type { get; set; }
        public int InitialCost { get; set; }
        private List<Bid> Bids { get; set; }
        public Client Seller { get; set; }
        public DateTime ListedOn { get; set; } = DateTime.Now;
        public Bid WinningBid { get; set; }

        public bool Sold { 
            get {
                return WinningBid != null;
            } 
        }

        public Product(Product existingProduct)
        {
            Description = existingProduct.Description;
            Type = existingProduct.Type;
            InitialCost = existingProduct.InitialCost;
            Bids = existingProduct.Bids;
            Seller = existingProduct.Seller;
            ListedOn = existingProduct.ListedOn;
            WinningBid = existingProduct.WinningBid;
        }
        public Product(string description, int cost) : this(description, "Uncategorised", cost) { }
        public Product(string description, string type, int cost)
        {
            Description = description;
            Type = type;
            InitialCost = cost;
            Bids = new List<Bid>();
            Seller = IdentityController.CurrentUser;
        }
        public Product(string description, string type, int cost, Client seller, List<Bid> bids)
        {
            Description = description;
            Type = type;
            InitialCost = cost;
            Bids = bids;
            Seller = seller;
        }
        public void MakeBid(Bid newBid)
        {
            Bids.Add(newBid);
        }
        public List<Bid> GetAllBids()
        {
            if(IdentityController.CurrentUser != Seller)
            {
                List<Bid> refactoredBids = new List<Bid>();
                foreach (Bid aBid in Bids)
                {
                    Bid modifiedBid = new Bid(aBid);
                    modifiedBid.Bidder = null;
                    refactoredBids.Add(modifiedBid);
                }
                return refactoredBids.OrderByDescending(bid => bid.Amount).ToList();
            } else
            {
                return Bids.OrderByDescending(bid => bid.Amount).ToList();
            }
        }

        public Bid GetHighestBid()
        {
            if(Bids.Count > 0)
            {
                return Bids.OrderByDescending(b => b.Amount).First();
            }
            return null;
        }
        public int GetHighestBidAmount()
        {
            if(Bids.Count > 0)
            {
                return Bids.Select(b => b.Amount).Max();
            }
            return InitialCost;
        }

        public override string ToString()
        {
            return $"{this.Description} ({this.Type})";
        }
    }
}
