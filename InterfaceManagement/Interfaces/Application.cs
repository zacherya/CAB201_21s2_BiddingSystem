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
    public class Application : Interface
    {
        private ClientDataProvider clientContext { get; set; }
        private ProductDataProvider productContext { get; set; }

        /// <summary>
        /// Initalise the interface object with context parameters to access
        /// Compose the menu items to the menu constructor
        /// </summary>
        /// <param name="cdp">The client data provider context the interface should interact with</param>
        /// <param name="pdp">The product data provider context the interface should interact with</param>
        public Application(ClientDataProvider cdp, ProductDataProvider pdp)
        {
            Console.Title = $"EBuy - {IdentityController.CurrentUser}";
            clientContext = cdp;
            productContext = pdp;
            ComposeMenu();
        }

        /// <summary>
        /// Compose the menu items required for this interface and assign to the base class interface constructor
        /// </summary>
        public override void ComposeMenu()
        {
            // Construct the Menu Constructor with the basic exit application function
            List<MenuItem> itemsList = new List<MenuItem>();
            itemsList.Add(new MenuItem(999, "Logout", LogoutClient));
            itemsList.Add(new MenuItem(1, "Register item for sale", RegisterItem));
            itemsList.Add(new MenuItem(2, "List my items", ListCurrentClientItems));
            itemsList.Add(new MenuItem(3, "Search for items", SearchForItem));
            itemsList.Add(new MenuItem(4, "Place a bid on an item",PlaceBid));
            itemsList.Add(new MenuItem(5, "List bids received for my items", ListBidsOnClientsItems));
            itemsList.Add(new MenuItem(6, "Sell one of my items to the highest bidder", SellToHighestBidder));
            InterfaceConstructor = new MenuConstructor(itemsList);
        }

        /// <summary>
        /// Log out the current user to tell the program to return to the authentication interface
        /// Clear out history to protect the previous clients data (Act like a kiosk)
        /// </summary>
        private static void LogoutClient()
        {
            Client client = IdentityController.CurrentUser; // Access and store the client reference to reference in interface prompts after tecnical signout
            DisplayWarning($"!! Logging out {client} !!");
            IdentityController.UnAuthenticateUser(); // Log out the user (Technical)
            Console.Title = "EBuy - Welcome";
            Thread.Sleep(750); // Wait to indicate to the user what is happening
            DisplaySuccess($"{client} has been logged out!");
            Thread.Sleep(500); // Wait to indicate to the user what is happening
            Console.Clear(); // Clear the session history from the console window
        }

        /// <summary>
        /// Manipulate the console interface to ask for input to register a new item
        /// </summary>
        private void RegisterItem()
        {
            string type = GetInput("Type", allowNoInput:true);
            if (string.IsNullOrEmpty(type) || string.IsNullOrWhiteSpace(type)) Console.Write("Uncategorised"); // If no input is type uncategorise the item
            string description = GetInput("Description");
            int initalCost = GetInt("Inital Bid");

            try
            {
                Product productToRegister; // Create a new product instance to register
                if (string.IsNullOrEmpty(type) || string.IsNullOrWhiteSpace(type))
                {
                    // If type is empty create the product with default uncategorised initaliser
                    productToRegister = new Product(description, initalCost);
                } else
                {
                    // If type is not empty create the product with default initaliser for new product
                    productToRegister = new Product(description, type, initalCost);
                }
                
                // Tell the context to add a new product
                productContext.AddProduct(productToRegister);
                // If no exception was thrown display a success
                DisplaySuccess($"{productToRegister.Type}, {productToRegister.Description} was listed successfully!");
            } catch
            {
                DisplayError("There was an error listing your product, try again.");
            }

        }

        /// <summary>
        /// Manipulate the console interface to list all the items that the current logged in user is selling
        /// </summary>
        private void ListCurrentClientItems()
        {
            //Get current user and append to list to specifiy a filter
            Client user = IdentityController.CurrentUser;
            List<Client> filter = new List<Client>() { user };

            // Filtering in products by seller (List of clients)
            IEnumerable<Product> products = productContext.GetProducts(filter,false);

            if(products.Count() > 0)
            {
                // There are items
                Console.WriteLine($"Items owned by {user.FullName}:");
                string productsTable = products.ToConsoleDataGrid( // create data grid string 
                        prod => prod.Description,
                        prod => prod.Type,
                        prod => new { StartingCost = $"{ToCurrency(prod.InitialCost)}" }.StartingCost,
                        prod => new { Bids = prod.GetAllBids().Count() }.Bids,
                        prod => new { HighestBid = (prod.GetAllBids().Count() < 1 ? "N/A" : $"{ToCurrency(prod.GetHighestBidAmount())}") }.HighestBid,
                        prod => prod.ListedOn
                    );
                Console.WriteLine(productsTable); // Write the table to the console
            } else
            {
                // THere are no items the user is currently selling
                DisplayError($"{user.FullName} has no items for sale");
            }
        }

        /// <summary>
        /// Manipulate the console interface to search for an item in the whole data providers list of items
        /// </summary>
        private void SearchForItem()
        {
            // Ask for input of product type
            string typeFilter = GetInput("Type");

            //Get current user and append to list to specifiy a filter
            Client user = IdentityController.CurrentUser;
            List<Client> filter = new List<Client>() { user };

            // Get the products where the type contains the user input and where the current logged in user is not the seller
            IEnumerable<Product> queriedProducts = productContext.GetProducts(filter, true).Where(prod => prod.Type.ToLower().Contains(typeFilter.ToLower()));
            if (queriedProducts.Count() > 0)    
            {
                // There is products found with search query
                Console.WriteLine($"Found {queriedProducts.Count()} results for '{typeFilter}':");
                string queryTable = queriedProducts.ToConsoleDataGrid( // create a data table string
                        prod => prod.Description,
                        prod => prod.Type,
                        prod => new { ListedBy = prod.Seller.FullName }.ListedBy,
                        prod => new { Bids = prod.GetAllBids().Count() }.Bids,
                        prod => new { CurrentPrice = (prod.GetAllBids().Count() < 1 ? $"{ToCurrency(prod.InitialCost)}" : $"{ToCurrency(prod.GetHighestBidAmount())}") }.CurrentPrice,
                        prod => prod.ListedOn
                    );
                Console.WriteLine(queryTable); // write table string to the console
            } else
            {
                // No products found from query
                DisplayError($"No products were found containing the query {typeFilter}");
            }
        }

        /// <summary>
        /// Manipulate the console interface to place a bid on a particular item
        /// </summary>
        private void PlaceBid()
        {
            // Ask for input of product type
            string typeFilter = GetInput("Type");

            //Get current user and append to list to specifiy a filter
            Client user = IdentityController.CurrentUser;
            List<Client> filter = new List<Client>() { user };

            // Get the products where the type contains the user input and where the current logged in user is not the seller
            // Convert to array to make the list indexable
            Product[] queriedProducts = productContext.GetProducts(filter, true).Where(prod => prod.Type.ToLower().Contains(typeFilter.ToLower())).ToArray();
            if (queriedProducts.Count() > 0)
            {
                Console.WriteLine($"Found {queriedProducts.Count()} results for '{typeFilter}':");
                string queryTable = queriedProducts.ToConsoleDataGrid( // create a data grid string
                        prod => new { ItemNo = Array.IndexOf(queriedProducts, prod) + 1 }.ItemNo,
                        prod => prod.Description,
                        prod => prod.Type,
                        prod => new { ListedBy = prod.Seller.FullName }.ListedBy,
                        prod => new { Bids = prod.GetAllBids().Count() }.Bids,
                        prod => new { CurrentPrice = (prod.GetAllBids().Count() < 1 ? $"{ToCurrency(prod.InitialCost)}" : $"{ToCurrency(prod.GetHighestBidAmount())}") }.CurrentPrice,
                        prod => prod.ListedOn
                    );
                Console.WriteLine(queryTable); // print the data grid to the console
                // Ask the user to select an item from the above list to bid on
                int choice;
                if (AskForChoice(queriedProducts.Count(), out choice, "Item #"))
                {
                    // The product from the list that the user chose
                    Product selectedProduct = queriedProducts[choice - 1];

                    // Re iterate the product to the user
                    Console.WriteLine($"Bidding on item {choice} - {selectedProduct}");

                    // Assign the current user as the bidder and ask for input on how much they'd like to bid
                    Client bidder = IdentityController.CurrentUser;
                    int amount = GetInt("Amount ($)");
                    bool homeDelivery = GetBoolean("Home Delivery");

                    // Ensure the user is sure about their bid before proceeding
                    Console.WriteLine();
                    DisplayWarning($"Are you sure you want to bid on {selectedProduct}?", false);
                    bool areYouSure = GetBoolean($"Proceed?");

                    if(areYouSure)
                    {
                        // THey want to make the bid (YES)
                        Bid bidToMake = new Bid(amount, bidder, homeDelivery);
                        selectedProduct.MakeBid(bidToMake);
                        DisplaySuccess($"Your bid of {ToCurrency(amount)} was placed successfully!");
                    } else
                    {
                        // THey cancelled the bid (NO)
                        DisplayCriticalError("Bid discarded! No action has been made.");
                    }

                    
                }
            }
            else
            {
                DisplayError($"No products were found containing the word {typeFilter}");
            }
        }

        /// <summary>
        /// Manipulate the console interface to list bids on a particular item the client is selling
        /// </summary>
        private void ListBidsOnClientsItems()
        {
            //Get current user and append to list to specifiy a filter
            Client user = IdentityController.CurrentUser;
            List<Client> filter = new List<Client>() { user };

            // Filtering in products by seller (List of clients)
            Product[] products = productContext.GetProducts(filter, false).ToArray();

            if (products.Count() > 0)
            {
                // THere are products being sold by the client
                Console.WriteLine($"Please choose one of the following:");
                string productsTable = products.ToConsoleDataGrid( // create a data grid string to show products
                        prod => new { ItemNo = Array.IndexOf(products, prod)+1 }.ItemNo,
                        prod => prod.Description,
                        prod => prod.Type,
                        prod => prod.ListedOn
                    );
                Console.WriteLine(productsTable); // print the data grid string

                // Ask the user to choose an item from the list
                int choice;
                if (AskForChoice(products.Count(), out choice, "Item #"))
                {
                    //List the bids for given product
                    List<Bid> productBids = products[choice-1].GetAllBids();
                    if(productBids.Count() > 0)
                    {
                        // THere are bids
                        Console.WriteLine($"Bids received on item {choice}:");
                        string bidsTable = productBids.ToConsoleDataGrid( // Create a data grid string to show bids
                            bid => new { BidderName = bid.Bidder.FullName }.BidderName,
                            bid => new { BidderEmail = bid.Bidder.Email }.BidderEmail,
                            bid => new { Amount = $"{ToCurrency(bid.Amount)}" }.Amount
                        );
                        Console.WriteLine(bidsTable); // Print the data grid string to the console
                    } else
                    {
                        // THere are no bids
                        DisplayError($"There are no bids yet on item {choice}");
                    }
                }
            }
            else
            {
                // There are no items
                DisplayError($"You have no items for sale");
            } 
        }

        /// <summary>
        /// Manipulate the console interface to sell an item the current user is selling to the highest bidder
        /// </summary>
        private void SellToHighestBidder()
        {
            //Get current user and append to list to specifiy a filter
            Client user = IdentityController.CurrentUser;
            List<Client> filter = new List<Client>() { user };

            // Filtering in products by seller (List of clients)
            Product[] products = productContext.GetProducts(filter, false).ToArray();

            if (products.Count() > 0)
            {
                // There are products
                Console.WriteLine($"Please choose one of the following to sell:");
                string productsTable = products.ToConsoleDataGrid( // Create a data grid string
                        prod => new { ItemNo = Array.IndexOf(products, prod) + 1 }.ItemNo,
                        prod => prod.Description,
                        prod => prod.Type,
                        prod => new { Bids = prod.GetAllBids().Count() }.Bids,
                        prod => new { CurrentPrice = (prod.GetAllBids().Count() < 1 ? $"{ToCurrency(prod.InitialCost)}" : $"{ToCurrency(prod.GetHighestBidAmount())}") }.CurrentPrice,
                        prod => prod.ListedOn
                    );
                Console.WriteLine(productsTable); // print the data grid string to the console

                // Ask the user to select an item from the list generated 
                int choice;
                if (AskForChoice(products.Count(), out choice, "Item #"))
                {
                    //Sell the product here
                    Product selectedProduct = products[choice - 1];
                    if (selectedProduct.GetAllBids().Count() > 0)
                    {
                        DisplayWarning("Commencing sale of product");

                        //Tell the datasource to mark the chosen product as sold
                        productContext.MarkSold(selectedProduct);

                        //Define winning bid attirbutes
                        Bid winningBid = selectedProduct.WinningBid;
                        Client winningBidder = winningBid.Bidder;
                        bool homeDelivery = winningBid.HomeDelivery;

                        // Calculate the total amount the bidder will have/has paid
                        int totalPaid = winningBid.Amount + (homeDelivery ? 5 : 0);

                        // Calculate the total tax
                        int homeDeliveryTax = (homeDelivery ? 5 : 0);
                        double governmentTax = winningBid.Amount * 0.15;
                        double totalTax = homeDeliveryTax + governmentTax;

                        // Calculate the deductions taken by auction house
                        int auctionHouseDeduction = (homeDelivery ? 20 : 10);

                        // Determine the net profit (Total Paid - Inital cost) - (Deductions + Total Tax)
                        double netProfit = (totalPaid - selectedProduct.InitialCost) - (totalTax + auctionHouseDeduction);

                        //Start printing calculation to the console in user friendly format
                        Console.WriteLine($"Total Paid: {ToCurrency(totalPaid)}");
                        Console.WriteLine();

                        Console.WriteLine($"Tax");
                        Console.WriteLine("".PadLeft(2) + $"Home Delivery Tax: {ToCurrency(homeDeliveryTax)}");
                        Console.WriteLine("".PadLeft(2) + $"Governement (15%): {ToCurrency(governmentTax)}");
                        Console.WriteLine("".PadLeft(1) + $"Total: {ToCurrency(totalTax)}");
                        Console.WriteLine();

                        Console.WriteLine($"Auction House Deductions");
                        Console.WriteLine("".PadLeft(2) + $"Home Delivery: {ToCurrency(homeDelivery ? 20 : 0)}");
                        Console.WriteLine("".PadLeft(2) + $"Click & Collect: {ToCurrency(homeDelivery ? 0 : 10)}");
                        Console.WriteLine("".PadLeft(1) + $"Total: {ToCurrency(auctionHouseDeduction)}");
                        Console.WriteLine();

                        Console.WriteLine($"Net Profit: {ToCurrency(netProfit)}");
                        Console.WriteLine();

                        // Display confirmation messages
                        DisplaySuccess($"{winningBidder} has paid a total of {ToCurrency(totalPaid)} including total tax of {ToCurrency(totalTax)}.");

                        string requested = (homeDelivery ? $"Home Delivery for an additional {ToCurrency(5)}." : "Click & Collect from the auction house.");
                        DisplaySuccess($"{selectedProduct} has been sold to {winningBidder} for {ToCurrency(winningBid.Amount)} and has requested {requested}");

                        if(homeDelivery) DisplaySuccess($"You must ship the item to: {winningBidder.Address}");

                        // Wait a few seconds for the user to realise the sale was successful
                        Thread.Sleep(2000);
                    } else
                    {
                        // No bids so display error
                        DisplayError("This item has no bids");
                    }

                }
            }
            else
            {
                // No items so display error
                DisplayError($"You have no items for sale");
            }
        }
    }
}
