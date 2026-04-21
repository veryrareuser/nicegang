using NICE.EAT.Data;
using NICE.EAT.Interfaces;
using NICE.EAT.Models;
using NICE.EAT.Services;
using NICE.EAT.Services.Filters;
using NICE.EAT.Utilities;

// ═════════════════════════════════════════════════════════════════════════════
//  COMPOSITION ROOT — all logic lives in Services/Models, NOT here.
//  Program.cs is purely I/O (Requirement #7: No Logic in UI).
// ═════════════════════════════════════════════════════════════════════════════

var restaurantRepo = new RestaurantRepository();
var menuRepo       = new MenuItemRepository();
var userRepo       = new UserRepository();
var orderRepo      = new OrderRepository();

var waitService           = new WaitTimeService();
var recommendationService = new RecommendationService(restaurantRepo, menuRepo, waitService);
var orderService          = new OrderService(orderRepo);
var popularityService     = new PopularityService(menuRepo);

// ─── Startup ─────────────────────────────────────────────────────────────────
ConsoleHelper.PrintBanner();

User currentUser = HandleLogin(userRepo);

// ─── Main Loop ───────────────────────────────────────────────────────────────
bool running = true;
while (running)
{
    PrintMainMenu(currentUser);
    string choice = Console.ReadLine()?.Trim() ?? "";

    switch (choice)
    {
        case "1": HandleRecommendation(); break;
        case "2": HandleOrderHistory();   break;
        case "3": HandleFavorite();       break;
        case "4": HandlePopularity();     break;
        case "5": HandlePlaceOrder();     break;
        case "6":
            ConsoleHelper.PrintSuccess("Thanks for using NICE.EAT! Selamat makan! 🍽️");
            running = false;
            break;
        default:
            ConsoleHelper.PrintError("Invalid choice. Please pick 1–6.");
            break;
    }
}

// ═════════════════════════════════════════════════════════════════════════════
//  LOCAL FUNCTIONS — I/O only, all real logic is in services/models
// ═════════════════════════════════════════════════════════════════════════════

// ─── Login / Register ────────────────────────────────────────────────────────
User HandleLogin(UserRepository repo)
{
    ConsoleHelper.PrintHeader("Welcome — Login or Register");

    while (true)
    {
        string username = ConsoleHelper.AskInput("Enter your username (or 'new' to register)");

        if (username.Equals("new", StringComparison.OrdinalIgnoreCase))
        {
            string newUsername = ConsoleHelper.AskInput("Choose a username");
            string email       = ConsoleHelper.AskInput("Enter your email");
            bool   isPremium   = ConsoleHelper.AskYesNo("Register as Premium member? (10% discount)");

            try
            {
                User created = isPremium
                    ? repo.CreatePremiumUser(newUsername, email)
                    : repo.CreateUser(newUsername, email);

                ConsoleHelper.PrintSuccess($"Welcome aboard, {created.Username}! ({created.GetUserType()}) 🎉");
                ConsoleHelper.PressAnyKey();
                return created;
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Registration failed: {ex.Message}");
                continue;
            }
        }

        User? user = repo.GetByUsername(username);
        if (user is null)
        {
            ConsoleHelper.PrintError("Username not found. Try again or type 'new'.");
            continue;
        }

        ConsoleHelper.PrintSuccess($"Welcome back, {user.Username}! ({user.GetUserType()}) 👋");

        // POLYMORPHISM: GetProfileSummary() behaves differently for PremiumUser
        Console.WriteLine($"  {user.GetProfileSummary()}");
        ConsoleHelper.PressAnyKey();
        return user;
    }
}

// ─── Main Menu ───────────────────────────────────────────────────────────────
void PrintMainMenu(User user)
{
    Console.Clear();
    ConsoleHelper.PrintBanner();

    // POLYMORPHISM: GetProfileSummary() differs for User vs PremiumUser
    Console.WriteLine($"  {user.GetProfileSummary()}");
    ConsoleHelper.PrintSeparator();
    Console.WriteLine();
    Console.WriteLine("  [1] 🔍  Get Restaurant Recommendation");
    Console.WriteLine("  [2] 📋  View My Order History");
    Console.WriteLine("  [3] ❤️   My Favorite Restaurant");
    Console.WriteLine("  [4] 🔥  Most Popular Menu Today");
    Console.WriteLine("  [5] 🛒  Record a New Order");
    Console.WriteLine("  [6] 🚪  Exit");
    Console.WriteLine();
    ConsoleHelper.PrintSeparator();
    Console.Write("  ▶ Your choice: ");
}

// ─── 1. Recommendation (uses IFilterStrategy polymorphically) ────────────────
void HandleRecommendation()
{
    ConsoleHelper.PrintHeader("Find Your Perfect Restaurant");

    decimal budget     = decimal.Parse(ConsoleHelper.AskInput("Max budget per item (IDR, e.g. 30000)").Replace(",", "").Replace(".", ""));
    int     spicyLevel = ConsoleHelper.AskInt("Max spicy level", 0, 5);
    bool    veganOnly  = ConsoleHelper.AskYesNo("Vegan only?");

    Console.Write("\n  Supported moods: ");
    Console.WriteLine(string.Join(", ", MoodMapper.SupportedMoods));
    string mood = ConsoleHelper.AskInput("Your current mood");

    Console.WriteLine("\n  ⏳ Applying filter strategies...");

    // Build filter pipeline using IFilterStrategy (POLYMORPHISM)
    recommendationService.ClearFilters();
    recommendationService.AddFilter(new BudgetFilter(budget));   // IFilterStrategy impl #1
    recommendationService.AddFilter(new SpicyFilter(spicyLevel)); // IFilterStrategy impl #3
    recommendationService.AddFilter(new MoodFilter(mood));        // IFilterStrategy impl #2
    if (veganOnly)
        recommendationService.AddFilter(new VeganFilter());       // IFilterStrategy impl #4

    var results = recommendationService.GetRecommendations();

    if (results.Count == 0)
    {
        ConsoleHelper.PrintError("No restaurants match your criteria. Try relaxing your filters.");
        ConsoleHelper.PressAnyKey();
        return;
    }

    ConsoleHelper.PrintHeader($"Recommendations ({results.Count} Found)");

    int i = 1;
    foreach (var r in results)
    {
        // IRecommendable polymorphism: Restaurant.GetDisplayInfo()
        IRecommendable recommendable = r;
        Console.WriteLine($"\n  ┌─ [{i++}] {recommendable.GetDisplayInfo()}");
        ConsoleHelper.PrintLabel("Spicy Level", ConsoleHelper.SpicyBar(r.SpicyLevel));
        ConsoleHelper.PrintLabel("Diet", ConsoleHelper.VeganBadge(r.IsVegan));
        ConsoleHelper.PrintLabel("Wait Time", ConsoleHelper.WaitBadge(r.WaitTimeMinutes));
        ConsoleHelper.PrintLabel("Mood Tags", r.MoodTags);
        ConsoleHelper.PrintLabel("Score", $"{recommendable.GetRecommendationScore(budget, spicyLevel):F0}/100");

        if (r.Menu.Count > 0)
        {
            Console.WriteLine("\n  Menu:");
            foreach (var m in r.Menu)
            {
                // POLYMORPHISM: GetDescription() differs for FoodItem vs DrinkItem
                Console.WriteLine($"    • {m.GetDescription()}");
            }
        }

        ConsoleHelper.PrintSeparator();
    }

    ConsoleHelper.PressAnyKey();
}

// ─── 2. Order History ────────────────────────────────────────────────────────
void HandleOrderHistory()
{
    ConsoleHelper.PrintHeader($"Order History — {currentUser.Username}");

    var history = orderService.GetHistory(currentUser.Id);

    if (history.Count == 0)
    {
        ConsoleHelper.PrintInfo("You haven't placed any orders yet.");
        ConsoleHelper.PressAnyKey();
        return;
    }

    int idx = 1;
    foreach (var o in history)
    {
        // POLYMORPHISM: GetOrderSummary() differs for Order vs DeliveryOrder
        Console.WriteLine($"\n  [{idx++}] {o.GetOrderSummary()}");
        ConsoleHelper.PrintLabel("Date", $"{o.CreatedAt:yyyy-MM-dd HH:mm}");
        ConsoleHelper.PrintLabel("Type", o.GetOrderType());

        if (o is DeliveryOrder delivery)
        {
            ConsoleHelper.PrintLabel("Delivery To", delivery.DeliveryAddress);
            ConsoleHelper.PrintLabel("Delivery Fee", $"Rp {delivery.DeliveryFee:N0}");
        }

        if (!string.IsNullOrEmpty(o.Note))
            ConsoleHelper.PrintInfo($"Note: {o.Note}");
    }

    ConsoleHelper.PressAnyKey();
}

// ─── 3. Favorite Restaurant ──────────────────────────────────────────────────
void HandleFavorite()
{
    ConsoleHelper.PrintHeader("My Favorite Restaurant");

    var fav = orderService.GetFavoriteRestaurant(currentUser.Id);

    if (fav is null)
    {
        ConsoleHelper.PrintInfo("No orders yet. Start ordering to see your favorite!");
    }
    else
    {
        Console.WriteLine($"\n  🏆  Your top spot: {fav.Value.Name}");
        Console.WriteLine($"  📊  Total orders : {fav.Value.Count} time(s)");
    }

    ConsoleHelper.PressAnyKey();
}

// ─── 4. Most Popular Menu ────────────────────────────────────────────────────
void HandlePopularity()
{
    ConsoleHelper.PrintHeader("🔥 Most Popular Menu Items");

    var topItems = popularityService.GetTopMenuItems(5);

    if (topItems.Count == 0)
    {
        ConsoleHelper.PrintInfo("Not enough order data yet.");
        ConsoleHelper.PressAnyKey();
        return;
    }

    string[] medals = { "🥇", "🥈", "🥉", "4️⃣ ", "5️⃣ " };

    for (int j = 0; j < topItems.Count; j++)
    {
        var m = topItems[j];
        string medal = j < medals.Length ? medals[j] : "  ";

        // POLYMORPHISM: GetDescription() differs for FoodItem vs DrinkItem
        Console.WriteLine($"\n  {medal} {m.GetDescription()}");
        ConsoleHelper.PrintLabel("From", m.RestaurantName);
        ConsoleHelper.PrintLabel("Total Orders", $"{m.OrderCount}x");
        ConsoleHelper.PrintLabel("Type", m.GetItemType()); // "Food" or "Drink"
    }

    ConsoleHelper.PressAnyKey();
}

// ─── 5. Place Order ──────────────────────────────────────────────────────────
void HandlePlaceOrder()
{
    ConsoleHelper.PrintHeader("Record a New Order");

    var allRestaurants = restaurantRepo.GetAll();
    Console.WriteLine("\n  Available Restaurants:");
    for (int k = 0; k < allRestaurants.Count; k++)
        Console.WriteLine($"    [{k + 1}] {allRestaurants[k].Name}  (Rp {allRestaurants[k].AveragePrice:N0} avg)");

    int rChoice = ConsoleHelper.AskInt("Select restaurant number", 1, allRestaurants.Count);
    var selectedRestaurant = allRestaurants[rChoice - 1];

    var menuItems = menuRepo.GetByRestaurant(selectedRestaurant.Id);
    Console.WriteLine($"\n  Menu at {selectedRestaurant.Name}:");
    for (int m = 0; m < menuItems.Count; m++)
    {
        // POLYMORPHISM: GetDescription() shows different info for FoodItem vs DrinkItem
        Console.WriteLine($"    [{m + 1}] {menuItems[m].GetDescription()}");
    }

    int mChoice = ConsoleHelper.AskInt("Select menu item number", 1, menuItems.Count);
    var selectedItem = menuItems[mChoice - 1];

    bool isDelivery = ConsoleHelper.AskYesNo("Is this a delivery order?");
    string? deliveryAddress = null;
    decimal deliveryFee = 0;

    if (isDelivery)
    {
        deliveryAddress = ConsoleHelper.AskInput("Delivery address");
        deliveryFee = 5000; // fixed fee
        ConsoleHelper.PrintInfo($"Delivery fee: Rp {deliveryFee:N0}");
    }

    string? note = ConsoleHelper.AskInput("Any notes? (press Enter to skip)");
    if (string.IsNullOrWhiteSpace(note)) note = null;

    // POLYMORPHISM: user.CalculateOrderPrice() differs for User vs PremiumUser
    decimal finalPrice = currentUser.CalculateOrderPrice(selectedItem.Price);
    Console.WriteLine($"\n  💰 Item price : Rp {selectedItem.Price:N0}");
    Console.WriteLine($"  🏷️  Your price : Rp {finalPrice:N0} ({currentUser.GetUserType()})");
    if (isDelivery) Console.WriteLine($"  🚚 + Delivery : Rp {deliveryFee:N0}");
    Console.WriteLine($"  💵 Total      : Rp {finalPrice + deliveryFee:N0}");

    int orderId = orderService.PlaceOrder(
        currentUser,
        selectedRestaurant.Id,
        selectedItem.Id,
        selectedItem.Price,
        note,
        isDelivery,
        deliveryAddress,
        deliveryFee);

    ConsoleHelper.PrintSuccess($"Order #{orderId} placed! Enjoy your {selectedItem.Name}! 😊");

    if (currentUser is PremiumUser p)
        ConsoleHelper.PrintInfo($"💎 Loyalty points: {p.LoyaltyPoints}");

    ConsoleHelper.PressAnyKey();
}


