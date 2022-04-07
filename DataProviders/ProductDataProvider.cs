using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EBuy.Models;
using EBuy.Helpers;

namespace EBuy.DataProviders
{
    public class ProductDataProvider : BaseDataProvider<Product>
    {
        /// <summary>
        /// Middleware interception method to add a product to the database and manipulate data 
        /// </summary>
        /// <param name="productObject"></param>
        public void AddProduct(Product productObject)
        {
            // The product is assigned a date stamp when the object is created as backup
            // but must be reassigned the latest timestamp for data integrity
            productObject.ListedOn = DateTime.Now;
            AddEntity(productObject);
        }

        /// <summary>
        /// Fetch products from the datasource and apply filters for certain sellers (Clients)
        /// </summary>
        /// <param name="sellerFilter">The list of users to pass into the filter</param>
        /// <param name="filterOut">Specifies whether or not the filter is inclusive or exclusive</param>
        /// <returns>Returns the list of products from the datasource with the specified filter applied</returns>
        public IEnumerable<Product> GetProducts(List<Client> sellerFilter = null, bool filterOut = true)
        {
            //Avoid exceptions due to no seller filter
            sellerFilter = sellerFilter ?? new List<Client>() ;

            IEnumerable<Product> filteredProducts;
            if (filterOut) {
                // Filter exclusive
                 filteredProducts = GetEntities().Where(prod => !prod.Sold && !sellerFilter.Contains(prod.Seller));
            } else
            {
                // Filter inclusive
                filteredProducts = GetEntities().Where(prod => !prod.Sold && sellerFilter.Contains(prod.Seller));
            }

            // If data source has data then return filtered results else return empty list of products
            return HasData ? filteredProducts.OrderByDescending(prod => prod.ListedOn) : new List<Product>();
        }

        /// <summary>
        /// Fetch products from the datasource and apply filters for certain sellers (Clients)
        /// </summary>
        /// <param name="productFilter">The list of products to pass into the filter</param>
        /// <param name="filterOut">Specifies whether or not the filter is inclusive or exclusive</param>
        /// <returns>Returns the list of products from the datasource with the specified filter applied</returns>
        public IEnumerable<Product> GetProducts(List<Product> productFilter = null, bool filterOut = true)
        {
            //Avoid exceptions due to no product filter
            productFilter = productFilter ?? new List<Product>() ;

            IEnumerable<Product> filteredProducts;
            if (filterOut) {
                // Filter exclusive
                filteredProducts = GetEntities().Where(prod => !prod.Sold && !productFilter.Contains(prod));
            } else
            {
                // Filter inclusive
                filteredProducts = GetEntities().Where(prod => !prod.Sold && productFilter.Contains(prod));
            }
             
            // If data source has data then return filtered results else return empty list of products
            return HasData ? filteredProducts.OrderByDescending(prod => prod.ListedOn) : new List<Product>();
        }

        /// <summary>
        /// Duedillegence to mark the product as sold and ensure appropriate flags are set
        /// </summary>
        /// <param name="product">The product in question to be marked as sold</param>
        public void MarkSold(Product product)
        {
            IEnumerable<Bid> bidders = product.GetAllBids().OrderByDescending(prod => prod.Amount).AsEnumerable();
            Bid highestBidder = bidders.First();

            product.WinningBid = highestBidder;
            if (!EnsureEntity(product)) {
                throw new KeyNotFoundException("The product entity doesn't exist in the current context");
            }
        }

        /// <summary>
        /// Populate the datasource with random prpducts for demo purpeses.
        /// </summary>
        /// <param name="clientContext">The client context used to assign products to (Temporary access to context)</param>
        public void SetupDemoProducts(ClientDataProvider clientContext)
        {
            Client[] clients = clientContext.GetUsers().ToArray();
            string[] prodType = { "Dress", "Shirt", "Pants", "Underwear" };
            string[] prodDesc = { "Best dress", "Double pair", "You'll love this", "Wide fit", "Very loose", "Little room", "Stretchy" };

            for (var i = 0; i < clients.Count() * 3; i++)
            {
                Random rnd = new Random();

                var seller = clients[rnd.Next(0, clients.Count())];
                var desc = prodDesc[rnd.Next(0, prodDesc.Count())];
                var type = prodType[rnd.Next(0, prodType.Count())];
                var bids = new List<Bid>();
                var cost = rnd.Next(0, 500);
                for (var j = 0; j < rnd.Next(0, 10); j++)
                {
                    int amt = rnd.Next(cost, 9999);
                    Client[] potentialBidders = clients.Where(client => client != seller).ToArray();
                    Client bidder = potentialBidders[rnd.Next(0, potentialBidders.Count())];
                    bool homeDelivery = Convert.ToBoolean(rnd.Next(-1, 1));
                    bids.Add(new Bid(amt, bidder,homeDelivery));
                }

                Product productToAdd = new Product(desc, type, cost, seller, bids);

                AddProduct(productToAdd);

            }
        }
    }
}
