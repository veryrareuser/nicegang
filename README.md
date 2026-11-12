# 🍽️ NICE.EAT

**A smart restaurant recommendation & ordering system — built to demonstrate object-oriented design in C#.**

NICE.EAT takes your mood, budget, spice tolerance and diet, filters a seeded SQLite
database of restaurants and menus through a pluggable strategy pipeline, and lets you
build a cart, check out with a member discount, and review your order history — all in a
dark-themed WPF desktop client.

> **Course project** — Object-Oriented Programming (OOP) showcase.
> The code was written to demonstrate real OOP principles (encapsulation, inheritance,
> polymorphism, abstraction, composition) and classic design patterns (Strategy, Repository,
> Singleton, Factory) rather than to be a production restaurant system.

---

## ✨ Features

- **Login / Register** — sign in with an existing username, or register as a **Basic** or
  **Premium** member. The member tier is materialised as a different runtime type
  (`User` vs `PremiumUser`) and changes pricing behaviour everywhere in the app.
- **Smart recommendations** — a filter bar of *max budget*, *max spicy level*, *mood* and
  *vegan only* produces a ranked list of matching restaurants, each with rating, price,
  address, spice meter, vegan badge and a simulated wait time.
- **Mood-aware matching** — 8 moods (`happy`, `bored`, `angry`, `sad`, `stressed`, `calm`,
  `adventurous`, `tired`) map to one or more restaurant mood tags, so *"sad"* also matches
  restaurants tagged *happy*.
- **Menu browsing & cart** — add any menu item to the cart straight from the results list
  (or via the Cart page's quick-add), adjust quantities, remove lines.
- **Delivery or dine-in** — toggle delivery, enter an address, and a Rp 5,000 delivery fee
  is applied at checkout.
- **Member discounts** — Premium members automatically get their discount applied at
  checkout and earn loyalty points per order.
- **Invoice** — a printed-style receipt with line items, subtotal, discount row,
  delivery row and grand total, plus the generated order IDs.
- **Order history & favourite spot** — every order is persisted to SQLite and listed
  newest-first, with a "favourite restaurant" summary computed by aggregate SQL.
- **Dark theme + reusable style dictionary** — all colours, buttons, cards and typography
  live in one `Themes/Styles.xaml` resource dictionary.

---

## 🧬 OOP Concepts Demonstrated

Everything below is present in the code — the class names map 1:1 to files in this repo.

### Inheritance — 3-level chains rooted in `BaseEntity`

```
BaseEntity (abstract)              NICE.EAT/Models/BaseEntity.cs
├── User                            NICE.EAT/Models/User.cs
│   └── PremiumUser                 NICE.EAT/Models/PremiumUser.cs
├── MenuItem (abstract)             NICE.EAT/Models/MenuItem.cs
│   ├── FoodItem                    NICE.EAT/Models/FoodItem.cs
│   └── DrinkItem                   NICE.EAT/Models/DrinkItem.cs
├── Restaurant                      NICE.EAT/Models/Restaurant.cs
└── Order                           NICE.EAT/Models/Order.cs
    └── DeliveryOrder               NICE.EAT/Models/DeliveryOrder.cs
```

`BaseEntity` holds what every domain object shares — `Id`, `CreatedAt`, a `virtual`
`GetEntityLabel()`, plus `Equals`/`GetHashCode`/`ToString` overrides. Every model derives
from it, so those members are written once.

`MenuItem` is a second abstract tier: it adds the fields common to food and drink
(`Name`, `Price`, `SpicyLevel`, `IsVegan`, `Category`) and declares two **abstract**
methods, `GetItemType()` and `GetDescription()`, which every concrete item must implement.
`FoodItem` adds `PortionSize`; `DrinkItem` adds `IsCold` and `VolumeInMl`.

A parallel hierarchy exists for the service layer:

```
BaseService (abstract)             NICE.EAT/Services/BaseService.cs
├── RecommendationService           NICE.EAT/Services/RecommendationService.cs
├── OrderService                   NICE.EAT/Services/OrderService.cs
├── PopularityService              NICE.EAT/Services/PopularityService.cs
└── WaitTimeService                NICE.EAT/Services/WaitTimeService.cs
```

`BaseService` supplies `ServiceName`, a `protected Log()` helper and a `virtual IsReady()`
hook, so none of the four services duplicate logging boilerplate.

### Polymorphism — four distinct flavours, all real

| # | Where | Base | Overrides |
|---|-------|------|-----------|
| 1 | `User.CalculateOrderPrice()` | `User` returns full price | `PremiumUser` returns `price × (1 − DiscountRate)` |
| 2 | `Order.GetTotalPrice()` | `Order` returns item price | `DeliveryOrder` returns `base.GetTotalPrice() + DeliveryFee` |
| 3 | `MenuItem.GetDescription()` | abstract | `FoodItem` → category + portion + spice; `DrinkItem` → volume + cold/hot |
| 4 | `IRecommendable.GetRecommendationScore()` | interface | `Restaurant` → rating-weighted; `FoodItem` → price-weighted |

The polymorphism is *used*, not just declared. `OrderService.PlaceOrder()` receives a
`User` reference and calls `user.CalculateOrderPrice(itemPrice)` — no `if (user is
PremiumUser)` branch is needed for pricing, because the correct override runs
automatically. The same call site in `CartPage.UpdateSummary()` prices the whole cart the
same way.

### Strategy pattern — `IFilterStrategy` and its four implementations

`NICE.EAT/Interfaces/IFilterStrategy.cs` defines one contract:

```csharp
public interface IFilterStrategy
{
    string FilterName { get; }
    List<Restaurant> Filter(List<Restaurant> restaurants);
}
```

Implemented by `BudgetFilter`, `MoodFilter`, `SpicyFilter` and `VeganFilter` in
`NICE.EAT/Services/Filters/`. `RecommendationService` holds a `List<IFilterStrategy>` and
runs them as a pipeline:

```csharp
foreach (IFilterStrategy filter in _filters)
{
    restaurants = filter.Filter(restaurants);   // same call, different behaviour
}
```

The service never references a concrete filter type — add a `RatingFilter` tomorrow and
`RecommendationService` needs **zero** changes.

### Repository pattern & separation of concerns

All SQL lives in `NICE.EAT/Data/`, one repository per aggregate:

| Repository | Responsibility |
|------------|----------------|
| `UserRepository` | lookup by username, create basic / premium users, map rows → `User` / `PremiumUser` |
| `RestaurantRepository` | list all restaurants, fetch one by id |
| `MenuItemRepository` | menu items per restaurant, top-N most popular (`GROUP BY` + `COUNT`) |
| `OrderRepository` | insert orders, order history with `JOIN`s, favourite restaurant |

`DatabaseHelper` owns connection creation (reading the connection string from
`appsettings.json`) and `DbDataReaderExtensions` provides the column-name reader helpers.
The UI never touches SQL — it goes through services, which go through repositories.

### Factory via inheritance mapping (discriminator columns)

Each repository contains a small mapping factory that reads a discriminator column and
instantiates the correct subclass:

- `UserRepository.MapUser()` → `user_type = 'Premium'` ? `PremiumUser` : `User`
- `MenuItemRepository.MapMenuItem()` → `item_type = 'Drink'` ? `DrinkItem` : `FoodItem`
- `OrderRepository.MapOrder()` → `order_type = 'Delivery'` ? `DeliveryOrder` : `Order`

One flat table becomes a polymorphic object graph, and callers only ever see the base type.

### Composition — "has-a" relationships

- `Restaurant.Menu` — a `List<MenuItem>` loaded per recommendation.
- `User.OrderHistory` — a `List<Order>`.
- `Order` holds `OrderedBy` (User), `OrderedItem` (MenuItem) and `OrderedAtRestaurant`
  (Restaurant) references alongside its flat foreign keys.
- `RecommendationService` *composes* a `List<IFilterStrategy>`.

### Encapsulation

- `AppState.Cart` has a `private set` — the cart list can only be mutated through
  `AddToCart()`, `RemoveFromCart()` and `ClearCart()`, so quantity-merging logic is never
  duplicated.
- `CartItem.Subtotal` is a read-only computed property (`UnitPrice × Quantity`) — it cannot
  be set to an inconsistent value.
- `BaseService.Log()` is `protected`; `DatabaseHelper._connectionString` is
  `private static readonly`.

### Abstraction & the `IRecommendable` interface

`NICE.EAT/Interfaces/IRecommendable.cs` is implemented by two unrelated classes —
`Restaurant` and `FoodItem` — that share no base type beyond `BaseEntity`. Because both
satisfy the interface, `RecommendationService.PrintRecommendationScores()` can score them
through one variable and one method call, each running its own algorithm.

### Singleton pattern

`NICE.EAT.WPF/ViewModels/AppState.cs` exposes `AppState.Instance` via lazy initialisation
(`??=`). The logged-in user and the shopping cart therefore survive navigation between
Login → Recommend → Cart → Invoice → Orders.

### SOLID, as demonstrated here

| Principle | Where it shows up |
|-----------|-------------------|
| **S**ingle Responsibility | `RecommendationService` only recommends; `OrderService` only orders; `PopularityService` only ranks; each repository only touches one table |
| **O**pen/Closed | New filters, new user tiers and new order types are added by subclassing, never by editing existing classes |
| **L**iskov Substitution | Any `User` can be used where `User` is expected; any `Order` in a `List<Order>` behaves correctly through `GetTotalPrice()` |
| **I**nterface Segregation | Two small, focused interfaces (`IFilterStrategy`, `IRecommendable`) rather than one fat contract |
| **D**ependency Inversion | Services depend on the `IFilterStrategy` abstraction and on injected repositories, not on concrete filter or SQL implementations |

### Layered architecture

```
NICE.EAT.WPF  (presentation)  →  NICE.EAT  (domain + data)  →  SQLite
```

The WPF project contains only pages, view models and the theme. Every rule about pricing,
filtering or persistence lives in the core library, which has no UI dependency.

---

## 🛠️ Tech Stack

| Technology | Version / Detail |
|------------|------------------|
| Language | C# 13 (`net9.0-windows`) |
| Runtime | .NET 9 |
| Desktop UI | WPF (Windows Presentation Foundation) with XAML |
| Theming | Custom `ResourceDictionary` (`Themes/Styles.xaml`) — buttons, cards, brushes, typography |
| Data access | `Microsoft.Data.Sqlite` 10.0.6, hand-written ADO.NET (`SqliteCommand`, `SqliteDataReader`) with parameterised SQL |
| Database | SQLite — `niceeat.db` (seeded, committed) |
| Configuration | `appsettings.json` → `ConnectionStrings:DefaultConnection` |
| Design | Figma source file `NICE.EAT - UI (new).fig` (committed) |
| Tooling | Visual Studio 2022, .NET CLI |

---

## 📁 Project Structure

```
NICE.EAT/
├─ OOP.sln                     Visual Studio solution (2 projects)
├─ README.md                   This file
├─ walkthrough.md              Deep-dive walkthrough (flow diagrams, code explanations, Q&A)
├─ NICE.EAT - UI (new).fig     Figma design source for the WPF UI
│
├─ NICE.EAT/                   Core library — domain, data and services (no UI)
│  ├─ Data/                    Repository layer (all SQL lives here)
│  │  ├─ DatabaseHelper.cs         Connection factory from appsettings.json
│  │  ├─ DbDataReaderExtensions.cs Column-name helpers for DbDataReader
│  │  ├─ UserRepository.cs         Users table + User/PremiumUser factory
│  │  ├─ RestaurantRepository.cs   Restaurants table
│  │  ├─ MenuItemRepository.cs     Menu items + FoodItem/DrinkItem factory, top-N query
│  │  └─ OrderRepository.cs        Orders table + Order/DeliveryOrder factory
│  ├─ Interfaces/              Contracts
│  │  ├─ IFilterStrategy.cs        Strategy pattern contract (4 implementations)
│  │  └─ IRecommendable.cs         Shared scoring/display contract
│  ├─ Models/                  Domain entities
│  │  ├─ BaseEntity.cs            Abstract root: Id, CreatedAt, equality
│  │  ├─ User.cs / PremiumUser.cs Basic vs premium member (2-level inheritance)
│  │  ├─ MenuItem.cs               Abstract menu item base
│  │  ├─ FoodItem.cs / DrinkItem.cs Concrete menu items (3-level inheritance)
│  │  ├─ Restaurant.cs             Restaurant + has-many MenuItem, IRecommendable
│  │  ├─ Order.cs / DeliveryOrder.cs Dine-in vs delivery order
│  ├─ Services/                Application / business layer
│  │  ├─ BaseService.cs            Abstract service base (logging, ServiceName)
│  │  ├─ RecommendationService.cs  Runs the IFilterStrategy pipeline
│  │  ├─ OrderService.cs           Places orders, applies polymorphic pricing
│  │  ├─ PopularityService.cs      Top-N most-ordered menu items
│  │  ├─ WaitTimeService.cs        Simulated wait times + tier labels
│  │  └─ Filters/                  Strategy implementations
│  │     ├─ BudgetFilter.cs        By average price
│  │     ├─ SpicyFilter.cs         By max spice level
│  │     ├─ MoodFilter.cs          By mood tag (via MoodMapper)
│  │     └─ VeganFilter.cs         Vegan-only
│  ├─ Utilities/               Helpers
│  │  ├─ ConsoleHelper.cs          Formatted console output for the console prototype
│  │  └─ MoodMapper.cs             Mood → mood-tag mapping
│  ├─ Program.cs               Console prototype entry point (composition root)
│  ├─ schema.sql               Full DDL + seed data
│  ├─ appsettings.json         SQLite connection string
│  └─ niceeat.db               Seeded SQLite database
│
└─ NICE.EAT.WPF/               Desktop client — presentation layer
   ├─ App.xaml / App.xaml.cs   Application entry, merges the theme dictionary
   ├─ MainWindow.xaml(.cs)     Shell: sidebar navigation + content Frame
   ├─ Themes/Styles.xaml       Shared brushes, styles, buttons, cards
   ├─ ViewModels/
   │  ├─ AppState.cs              Singleton: current user + shopping cart
   │  └─ CartItem.cs              Cart line item with computed subtotal
   ├─ Views/
   │  ├─ LoginPage.xaml(.cs)      Login / Register tabs
   │  ├─ RecommendPage.xaml(.cs)  Filter bar, results, add-to-cart
   │  ├─ CartPage.xaml(.cs)       Cart lines, quantities, delivery, checkout
   │  ├─ InvoicePage.xaml(.cs)    Receipt after checkout
   │  ├─ OrdersPage.xaml(.cs)     Order history + favourite restaurant
   │  └─ InvoiceData.cs           DTO handed from Cart to Invoice
   ├─ appsettings.json         SQLite connection string
   └─ niceeat.db               Seeded SQLite database
```

---

## ▶️ How to Run

**Requirements:** Windows 10/11, [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0),
and **Visual Studio 2022** with the *".NET desktop development"* workload
(or the `Microsoft.WindowsDesktop.App` runtime for running a published build).
WPF is Windows-only — this project does not build on macOS or Linux.

### Option A — Visual Studio 2022 (recommended)

1. Clone the repository.
2. Open **`OOP.sln`** in Visual Studio 2022.
3. Let NuGet restore `Microsoft.Data.Sqlite` (or right-click the solution → **Restore NuGet Packages**).
4. In the Solution Explorer, set **`NICE.EAT.WPF`** as the startup project.
5. Press **F5** (or `dotnet run --project NICE.EAT.WPF`).

### Option B — .NET CLI

```bash
dotnet restore OOP.sln
dotnet build   OOP.sln
dotnet run --project NICE.EAT.WPF
```

The seeded `niceeat.db` ships with the repository, so there is nothing to import first.

### Demo accounts

| Username | Type | Notes |
|----------|------|-------|
| `demo_user` | Basic | pays full price |
| `vip_member` | Premium | 10% discount + loyalty points |

You can also register a new account straight from the Login page.

---

## 🗄️ Database

`NICE.EAT/schema.sql` contains the full DDL and the seed data for the SQLite database.
Four tables, all with `AUTOINCREMENT` primary keys and cascading foreign keys:

| Table | Purpose | Notable columns |
|-------|---------|-----------------|
| `users` | Customers | `user_type` (`Basic`/`Premium`), `loyalty_points`, `discount_rate` |
| `restaurants` | Restaurants | `spicy_level` (0–5), `is_vegan`, `average_price`, `mood_tags` (CSV), `rating` |
| `menu_items` | Menu | `item_type` (`Food`/`Drink`), `portion_size`, `is_cold`, `volume_ml`, `category` |
| `orders` | Orders | `order_type` (`DineIn`/`Delivery`), `delivery_address`, `delivery_fee`, `note` |

Seed content: **2 users**, **10 restaurants**, **30 menu items** and **8 demo orders**
(including delivery orders) so that popularity ranking and the "favourite restaurant"
summary work immediately.

The discriminator columns (`user_type`, `item_type`, `order_type`) are what the
repositories read to decide which subclass to instantiate.

`niceeat.db` is committed on purpose — it lets every collaborator clone and run the app
without setting up a database. To reset it, delete the file and re-run `schema.sql`:

```bash
sqlite3 niceeat.db < NICE.EAT/schema.sql
```

Connection string (`appsettings.json`):

```json
{ "ConnectionStrings": { "DefaultConnection": "Data Source=niceeat.db" } }
```

---

## 🖥️ Screens

| Screen | File | What it does |
|--------|------|--------------|
| **Login / Register** | `Views/LoginPage.xaml` | Two tabs. Sign in with a username, or register with username, email and an optional *Premium member* checkbox. On success the sidebar unlocks and the app navigates to Recommendations. |
| **Recommendations** | `Views/RecommendPage.xaml` | Filter card (max budget, spicy slider 0–5, mood dropdown, vegan checkbox) above a results list. Each restaurant card shows rating, average price, address, spice meter, vegan badge, simulated wait time and up to 6 menu items, each with an **Add** button that pushes to the cart and raises a toast. |
| **Cart** | `Views/CartPage.xaml` | Line items with quantity −/+ and remove, a quick-add restaurant/menu combo pair, a delivery toggle with address field, and a live summary (subtotal, member discount, delivery fee, total). **Checkout** places the orders and moves to the invoice. |
| **Invoice** | `Views/InvoicePage.xaml` | Confirmation header, customer + date, delivery block, line-item table, subtotal / discount / delivery rows and the grand total, with the created order IDs and buttons back to browsing or the order history. |
| **Orders** | `Views/OrdersPage.xaml` | "Favourite restaurant" card (most-ordered spot, computed by `GROUP BY`), a Refresh button and the full order history with type icon, item, restaurant, total, date, delivery info and note. |

The shell (`MainWindow.xaml`) provides the left sidebar — logo, current-user badge,
navigation buttons (Login, Recommendations, My Orders, Cart with a live item-count badge)
and a Logout button — with a `Frame` on the right hosting the pages.

---

## 📝 Notes

- **`walkthrough.md`** — a deeper companion document: architecture diagram, step-by-step
  user-flow traces through the actual methods, code snippets with explanations, and 15
  lecturer-style questions with model answers. Start there if you want the "why" behind
  the code.
- **`NICE.EAT/Program.cs`** — the original console prototype of the same domain logic
  (banner, menu-driven flow, ASCII art). It is excluded from the library compilation
  (`<Compile Remove="Program.cs" />`) because the library is consumed by the WPF client;
  the service/model layer is shared by both.
- **Design source** — `NICE.EAT - UI (new).fig` is the Figma file for the WPF UI and is
  committed alongside the code.
- **Static styling** — the UI uses code-behind + plain XAML bindings rather than an MVVM
  framework, so the project stays dependency-light and easy to follow.
- `walkthrough.txt`, `walkthrough.html` and `map.txt` are local scratch exports and are
  git-ignored.

---

## 📄 License

Released for educational purposes as an OOP course project.
