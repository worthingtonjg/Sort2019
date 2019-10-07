
# Introduction

This is a presentation I gave at the SORT 2019 conference.  It introduced the group to Web Assembly and C#/Blazor on the client side.

### Workshop Link

This presentation is based on the following workshop created by the dot net foundation, the instructions below walk through the files found at the link below.  

https://github.com/dotnet-presentations/blazor-workshop

### My Starting Point

I have created a modified starting point to simplify some of the steps so that the presentation would go smoother and be compressed to an hour and a half.  If you are trying to follow my version of this walk-through you may want to use my starting point, otherwise use theirs.

I am also using a preview version of Blazor and Visual Studio 2019, so you may be better off taking the code directly from the workshop link above, since they will likely keep it updated as Blazor moves forward.

https://github.com/worthingtonjg/Sort2019/blob/master/Blazor/00-Starting-point.zip

### Powerpoint

The powerpoint for my presentation is: 

https://github.com/worthingtonjg/Sort2019/blob/master/Blazor/Final%20-%20Blazor%20-%20Sort%202019%20-%20Jon%20Worthington.pptx

---
Pre-Session Prep

- Open powerpoint
- Make sure starting point is extracted and open in visual studio preview
- Make sure final web app is open and running
- Have link to blazor workshop up: https://github.com/dotnet-presentations/blazor-workshop

---
# Start Demo

Start by walking through starting point code:
- Show Shared Project
- Show Server Project
- Show Client Project - Point out that this is where the Blazor code lives => All this code is on the client side!!!! Should look familiar, very similar to razor

Explain Bootstrap process (at least as best as I understand it) ...

```
	index.html 
	=> (blazor.webassembly.js) and mono.wasm 
	=> <app> 
	=> Your DLL 
	=> App.Razor 
	=> Program.cs 
	=> Startup.cs 
	=> MainLayout.razor 
	=> Index.Razor
```

---
# Lets Look at Our First Blazor Page

Open *Index.Razor* and talk about
- Razor Components
- Pages => Routing => @page
- Inject => Dependency Injection
- @ Razor
- @ One-way binding
- Blazor Lifecycle Methods
- Show specials controller

**Build and run starting app**
- F12 
	- Show DLL's loading (mono, etc)
	- Show json

---
Now we will add some css to make our page look nice

**----------------- Index.razor -----------------------**
```
<div class="main">
    <ul class="pizza-cards">
        @if (specials != null)
        {
            @foreach (var special in specials)
            {
                <li style="background-image: url('@special.ImageUrl')">
                    <div class="pizza-info">
                        <span class="title">@special.Name</span>
                        @special.Description
                        <span class="price">@special.GetFormattedBasePrice()</span>
                    </div>
                </li>
            }
        }
    </ul>
</div>
```
**Build and run**

---
# Now lets add navigation

Open MainLayout.razor and discuss

**-----------------  MainLayout.razor -----------------------**

```
@inherits LayoutComponentBase

<div class="top-bar">
    <img class="logo" src="img/logo.svg" />

    <NavLink href="" class="nav-tab" Match="NavLinkMatch.All">
        <img src="img/pizza-slice.svg" />
        <div>Get Pizza</div>
    </NavLink>
</div>

<div class="content">
    @Body
</div>
```

- **Build and run**
- Talk about Navlink

---
# Now let's add our first shared component

Lets create a pizza customization dialog => Our first Blazor Component

Create *ConfigurePizzaDialog.razor* in *Shared* 

**-----------------  ConfigurePizzaDialog.razor -----------------------**
```
<div class="dialog-container">
    <div class="dialog">
        <div class="dialog-title">
            <h2>@Pizza.Special.Name</h2>
            @Pizza.Special.Description
        </div>
        <form class="dialog-body"></form>
        <div class="dialog-buttons">
            <button class="btn btn-secondary mr-auto">Cancel</button>
            <span class="mr-center">
                Price: <span class="price">@(Pizza.GetFormattedTotalPrice())</span>
            </span>
            <button class="btn btn-success ml-auto">Order ></button>
        </div>
    </div>
</div>

@code {
	[Parameter] public Pizza Pizza { get; set; }
}
```

- Discuss Parameters
- Show Pizza class
- Discuss Binding

**------------------------ Index.Razor --------------------------------**

Replace <li ... with 
```
<li @onclick="@(() => ShowConfigurePizzaDialog(special))" style="background-image: url('@special.ImageUrl')">
```

in @Code add ...

```
    Pizza configuringPizza;
    bool showingConfigureDialog;

    void ShowConfigurePizzaDialog(PizzaSpecial special)
    {
        configuringPizza = new Pizza()
        {
            Special = special,
            SpecialId = special.Id,
            Size = Pizza.DefaultSize,
            Toppings = new List<PizzaTopping>(),
        };

        showingConfigureDialog = true;
    }
```
	
At bottom of HTML add ...

```
@if (showingConfigureDialog)
{
    <ConfigurePizzaDialog Pizza="configuringPizza" />
}
```

**Build and Run to test dialog**

---
# Now we will talk about two way Data Binding

- Let's let the user choose the size of their pizza
- Replace empty <form> with ...

**-----------------  ConfigurePizzaDialog.razor -----------------------**
```
<form class="dialog-body">
    <div>
        <label>Size:</label>
        <input type="range" min="@Pizza.MinimumSize" max="@Pizza.MaximumSize" step="1" @bind-value="@Pizza.Size" @bind-value:event="oninput" />
        <span class="size-label">
            @(Pizza.Size)" (£@(Pizza.GetFormattedTotalPrice()))
        </span>
    </div>
</form>
```

- So far we have been using just one way data binding
- @bind / @bind-value
- **Build and run**
- The user should also be able to select additional toppings on ConfigurePizzaDialog.

---
# Now lets give the user the ability to add toppings

**-----------------  ConfigurePizzaDialog.razor -----------------------**

At top of page

```
@inject HttpClient HttpClient
```

in @code

```
    List<Topping> toppings;

    protected async override Task OnInitializedAsync()
    {
        toppings = await HttpClient.GetJsonAsync<List<Topping>>("toppings");
    }
```

Put this inside the `<form class="dialog-body">`, below the existing DIV block

```
<div>
    <label>Extra Toppings:</label>
    @if (toppings == null)
    {
        <select class="custom-select" disabled>
            <option>(loading...)</option>
        </select>
    }
    else if (Pizza.Toppings.Count >= 6)
    {
        <div>(maximum reached)</div>
    }
    else
    {
        <select class="custom-select" @onchange="@ToppingSelected">
            <option value="-1" disabled selected>(select)</option>
            @for (var i = 0; i < toppings.Count; i++)
            {
                <option value="@i">@toppings[i].Name - (£@(toppings[i].GetFormattedPrice()))</option>
            }
        </select>
    }
</div>

<div class="toppings">
    @foreach (var topping in Pizza.Toppings)
    {
        <div class="topping">
            @topping.Topping.Name
            <span class="topping-price">@topping.Topping.GetFormattedPrice()</span>
            <button type="button" class="delete-topping" @onclick="@(() => RemoveTopping(topping.Topping))">x</button>
        </div>
    }
</div>
```

Also add the following event handlers for topping selection and removal:

```
	void ToppingSelected(UIChangeEventArgs e)
	{
		if (int.TryParse((string)e.Value, out var index) && index >= 0)
		{
			AddTopping(toppings[index]);
		}
	}

	void AddTopping(Topping topping)
	{
		if (Pizza.Toppings.Find(pt => pt.Topping == topping) == null)
		{
			Pizza.Toppings.Add(new PizzaTopping() { Topping = topping });
		}
	}

	void RemoveTopping(Topping topping)
	{
		Pizza.Toppings.RemoveAll(pt => pt.Topping == topping);
	}
```

- Walk through code
- **Build and run**

---
# Component Events

Lets wire up the Cancel and Order buttons

**-----------------  ConfigurePizzaDialog.razor -----------------------**
```
[Parameter] public EventCallback OnCancel { get; set; }
[Parameter] public EventCallback OnConfirm { get; set; }
```

Replace `<div class="dialog-buttons">` with ...

```
<div class="dialog-buttons">
    <button class="btn btn-secondary mr-auto" @onclick="@OnCancel">Cancel</button>
    <span class="mr-center">
        Price: <span class="price">@(Pizza.GetFormattedTotalPrice())</span>
    </span>
    <button class="btn btn-success ml-auto" @onclick="@OnConfirm">Order ></button>
</div>
```

Modify our reference to ConfigurePizzaDialog in our Index to handle the EventCallbacks

**----------------- Index.razor -----------------------**
```
<ConfigurePizzaDialog 
    Pizza="configuringPizza" 
    OnCancel="CancelConfigurePizzaDialog"  
    OnConfirm="ConfirmConfigurePizzaDialog" />
```
in @code add empty methods ...

```
	void CancelConfigurePizzaDialog()
	{
	}

	void ConfirmConfigurePizzaDialog()
	{
	}
```

Add new variable ...

```
	Order order = new Order();
```

CancelConfigurePizzaDialog ...

```
		configuringPizza = null;
		showingConfigureDialog = false;
```

ConfirmConfigurePizzaDialog ...

```
		order.Pizzas.Add(configuringPizza);
		configuringPizza = null;
		showingConfigureDialog = false;
```

**Build and Run**

Show cancel and order buttons working

---
# Now lets show the user their orders by creating a ConfiguredPizzaItem component

- Create *ConfiguredPizzaItem.razor*

**----------------- ConfiguredPizzaItem.razor -----------------------**
```
<div class="cart-item">
    <a @onclick="@OnRemoved" class="delete-item">x</a>
    <div class="title">@(Pizza.Size)" @Pizza.Special.Name</div>
    <ul>
        @foreach (var topping in Pizza.Toppings)
        {
        <li>+ @topping.Topping.Name</li>
        }
    </ul>
    <div class="item-price">
        @Pizza.GetFormattedTotalPrice()
    </div>
</div>

@code {
    [Parameter] public Pizza Pizza { get; set; }
    [Parameter] public EventCallback OnRemoved { get; set; }
}
```

Explain code

Now lets add the new component to our page, just below `<div class="main">` add a side bar

**----------------- Index.razor -----------------------**
```
<div class="sidebar">
    @if (order.Pizzas.Any())
    {
        <div class="order-contents">
            <h2>Your order</h2>

            @foreach (var configuredPizza in order.Pizzas)
            {
                <ConfiguredPizzaItem Pizza="configuredPizza" OnRemoved="@(() => RemoveConfiguredPizza(configuredPizza))" />
            }
        </div>
    }
    else
    {
        <div class="empty-cart">Choose a pizza<br>to get started</div>
    }

    <div class="order-total @(order.Pizzas.Any() ? "" : "hidden")">
        Total:
        <span class="total-price">@order.GetFormattedTotalPrice()</span>
        <button class="btn btn-warning" disabled="@(order.Pizzas.Count == 0)" @onclick="@PlaceOrder">
            Order >
        </button>
    </div>
</div>
```

And add these new methods ...

**----------------- Index.razor -----------------------**
```
	void RemoveConfiguredPizza(Pizza pizza)
	{
		order.Pizzas.Remove(pizza);
	}

	async Task PlaceOrder()
	{
		await HttpClient.PostJsonAsync("orders", order);
		order = new Order();
	}
```
Explain code

**Build and run**

---
# Now need to show the order status

- Lets implement a My Orders page
- Create *MyOrders.razor* page
- Add routing

**----------------- MyOrders.razor -----------------------**
```
@page "/myorders"
@inject HttpClient HttpClient

    <div class="main">
        @if (ordersWithStatus == null)
        {
            <text>Loading...</text>
        }
        else if (ordersWithStatus.Count == 0)
        {
            <h2>No orders placed</h2>
            <a class="btn btn-success" href="">Order some pizza</a>
        }
        else
        {
            <div class="list-group orders-list">
                @foreach (var item in ordersWithStatus)
                {
                    <div class="list-group-item">
                        <div class="col">
                            <h5>@item.Order.CreatedTime.ToLongDateString()</h5>
                            Items:
                            <strong>@item.Order.Pizzas.Count()</strong>;
                            Total price:
                            <strong>£@item.Order.GetFormattedTotalPrice()</strong>
                        </div>
                        <div class="col">
                            Status: <strong>@item.StatusText</strong>
                        </div>
                        <div class="col flex-grow-0">
                            <a href="myorders/@item.Order.OrderId" class="btn btn-success">
                                Track &gt;
                            </a>
                        </div>
                    </div>
                }
            </div>
        }
    </div>

@code {
    List<OrderWithStatus> ordersWithStatus;

    protected override async Task OnInitializedAsync()
    {
        ordersWithStatus = await HttpClient.GetJsonAsync<List<OrderWithStatus>>("orders");
    }
}
```

Now lets modify the site navigation

**----------------- MainLayout.razor -----------------------**
```
<NavLink href="myorders" class="nav-tab">
    <img src="img/bike.svg" />
    <div>My Orders</div>
</NavLink>
```
**Build and run**

- Show orders page with no orders
- Add some orders show page
- Click track button

Now lets add OrderDetails.razor

**----------------- OrderDetails.razor -----------------------**

```
@page "/myorders/{orderId:int}"
@using System.Threading
@inject HttpClient HttpClient
@implements IDisposable

    <div class="main">
        @if (invalidOrder)
        {
            <h2>Nope</h2>
            <p>Sorry, this order could not be loaded.</p>
        }
        else if (orderWithStatus == null)
        {
            <text>Loading...</text>
        }
        else
        {
            <div class="track-order">
                <div class="track-order-title">
                    <h2>
                        Order placed @orderWithStatus.Order.CreatedTime.ToLongDateString()
                    </h2>
                    <p class="ml-auto mb-0">
                        Status: <strong>@orderWithStatus.StatusText</strong>
                    </p>
                </div>
                <div class="track-order-body">
                    TODO: show more details
                </div>
            </div>
        }
    </div>

@code {
    [Parameter] public int OrderId { get; set; }

    OrderWithStatus orderWithStatus;
    bool invalidOrder;
    CancellationTokenSource pollingCancellationToken;

    protected override void OnParametersSet()
    {
        // If we were already polling for a different order, stop doing so
        pollingCancellationToken?.Cancel();

        // Start a new poll loop
        PollForUpdates();
    }

    private async void PollForUpdates()
    {
        pollingCancellationToken = new CancellationTokenSource();
        while (!pollingCancellationToken.IsCancellationRequested)
        {
            try
            {
                invalidOrder = false;
                orderWithStatus = await HttpClient.GetJsonAsync<OrderWithStatus>($"orders/{OrderId}");
            }
            catch (Exception ex)
            {
                invalidOrder = true;
                pollingCancellationToken.Cancel();
                Console.Error.WriteLine(ex);
            }

            StateHasChanged();

            await Task.Delay(4000);
        }
    }
	
	void IDisposable.Dispose()
	{
		pollingCancellationToken?.Cancel();
	}
}
```

- Explain code - routing and disposing specifically
- **Build and run**
- Click Track Button

*******************************************************************
- Point out the TODO: show more details
- Notice we want to show the order details, we will build another component for that
- Create OrderReview.razor in the shared folder

**----------------- OrderReview.razor -----------------------**
```
@foreach (var pizza in Order.Pizzas)
{
    <p>
        <strong>
            @(pizza.Size)"
            @pizza.Special.Name
            (£@pizza.GetFormattedTotalPrice())
        </strong>
    </p>

    <ul>
        @foreach (var topping in pizza.Toppings)
        {
            <li>+ @topping.Topping.Name</li>
        }
    </ul>
}

<p>
    <strong>
        Total price:
        £@Order.GetFormattedTotalPrice()
    </strong>
</p>

@code {
    [Parameter] public Order Order { get; set; }
}
```

Then replace todo...

----------------- OrderDetails.razor -----------------------
```
    <div class="track-order-details">
        <OrderReview Order="@orderWithStatus.Order" />
    </div>
```

- **Build and run**
- Show order details on tracking page

*******************************************************************
# Automatic Navigation

When we place an order, it should automatically navigate to that order...

Add ...

**----------------- Index.razor -----------------------**
```
@inject IUriHelper UriHelper
```
Replace ...

```
async Task PlaceOrder()
{
    var newOrderId = await HttpClient.PostJsonAsync<int>("orders", order);
    order = new Order();

    UriHelper.NavigateTo($"myorders/{newOrderId}");
}
```

Explain

**Build and run (Don't close though - show the AppState problem below first)**

---
# AppState pattern

A problem You might have noticed this already, but our application has a bug! Since we're storing the list of pizzas in the current order on the Index component, the user's state can be lost if the user leaves the Index page. 

So we want to store the state of the order.  We can do this by adding an object to the DI container that you will use to coordinate state between related components.

Create a new OrderState.cs in Client

**----------------- OrderState.cs -----------------------**
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazingPizza.Client
{
    public class OrderState
    {
        public bool ShowingConfigureDialog { get; private set; }

        public Pizza ConfiguringPizza { get; private set; }

        public Order Order { get; private set; } = new Order();

        public void ShowConfigurePizzaDialog(PizzaSpecial special)
        {
            ConfiguringPizza = new Pizza()
            {
                Special = special,
                SpecialId = special.Id,
                Size = Pizza.DefaultSize,
                Toppings = new List<PizzaTopping>(),
            };

            ShowingConfigureDialog = true;
        }

        public void CancelConfigurePizzaDialog()
        {
            ConfiguringPizza = null;

            ShowingConfigureDialog = false;
        }

        public void ConfirmConfigurePizzaDialog()
        {
            Order.Pizzas.Add(ConfiguringPizza);
            ConfiguringPizza = null;

            ShowingConfigureDialog = false;
        }

        public void ResetOrder()
        {
            Order = new Order();
        }

        public void RemoveConfiguredPizza(Pizza pizza)
        {
            Order.Pizzas.Remove(pizza);
        }
		
		public void ReplaceOrder(Order order)
		{
			Order = order;
		}
    }
}
```

Register with the OrderState as a Scoped service in the DI container

Because the AppState object is managed by the DI container, it can outlive the components and hold on to state even when the UI is changing  scoped means for the current unit-of-work Startup.cs !!!! IN THE CLIENT !!!

**----------------- Startup.cs -----------------------**
```
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddScoped<OrderState>();
	}
```

Now that we have our OrderState regiistered

----------------- Index.razor -----------------------
```
@inject OrderState OrderState
```
----------------- Index.razor -----------------------

We now need to modify the Index.razor to use the OrderState instead of order

Delete (these are part of our OrderState now)

**----------------- Index.razor -----------------------**
```
    Pizza configuringPizza;
    bool showingConfigureDialog;
    Order order = new Order();
```
Delete Methods:

```
	void ShowConfigurePizzaDialog(PizzaSpecial special)	
	void CancelConfigurePizzaDialog()
	void ConfirmConfigurePizzaDialog()
	void RemoveConfiguredPizza(Pizza pizza)
```

Change markup to reference OrderState

**----------------- Index.razor -----------------------**
```
<div class="main">
    <ul class="pizza-cards">
        @if (specials != null)
        {
            @foreach (var special in specials)
            {
                <li @onclick="@(() => OrderState.ShowConfigurePizzaDialog(special))" style="background-image: url('@special.ImageUrl')">
                    <div class="pizza-info">
                        <span class="title">@special.Name</span>
                        @special.Description
                        <span class="price">@special.GetFormattedBasePrice()</span>
                    </div>
                </li>
            }
        }
    </ul>
</div>

<div class="sidebar">
    @if (OrderState.Order.Pizzas.Any())
    {
        <div class="order-contents">
            <h2>Your order</h2>

            @foreach (var configuredPizza in OrderState.Order.Pizzas)
            {
                <ConfiguredPizzaItem Pizza="configuredPizza" OnRemoved="@(() => OrderState.RemoveConfiguredPizza(configuredPizza))" />
            }
        </div>
    }
    else
    {
        <div class="empty-cart">Choose a pizza<br>to get started</div>
    }

    <div class="order-total @(OrderState.Order.Pizzas.Any() ? "" : "hidden")">
        Total:
        <span class="total-price">@OrderState.Order.GetFormattedTotalPrice()</span>
        <button class="btn btn-warning" disabled="@(OrderState.Order.Pizzas.Count == 0)" @onclick="@PlaceOrder">
            Order >
        </button>
    </div>
</div>


@if (OrderState.ShowingConfigureDialog)
{
    <ConfigurePizzaDialog Pizza="OrderState.ConfiguringPizza"
                          OnCancel="OrderState.CancelConfigurePizzaDialog"
                          OnConfirm="OrderState.ConfirmConfigurePizzaDialog" />
}	
```

Change PlaceOrder to reference OrderState

**----------------- Index.razor -----------------------**
```
	async Task PlaceOrder()
	{
		var newOrderId = await HttpClient.PostJsonAsync<int>("orders", OrderState.Order);
		OrderState.ResetOrder();
		UriHelper.NavigateTo($"myorders/{newOrderId}");
	}
```

Build and run - show AppState is fixed

---
# Add Checkout process - to capture delivery address

If you take a look at the Order class in BlazingPizza.Shared, you might notice that it holds a DeliveryAddress property of type Address. 

However, nothing in the pizza ordering flow yet populates this data, so all your orders just have a blank delivery address.

It's time to fix this by adding a "checkout" screen that requires customers to enter a valid address.

Add new page: Checkout.razor

**----------------- Checkout.razor -----------------------**
```
@page "/checkout"
@inject HttpClient HttpClient
@inject IUriHelper UriHelper;
@inject OrderState OrderState;

<div class="main">
    <div class="checkout-cols">
        <div class="checkout-order-details">
            <h4>Review order</h4>
            <OrderReview Order="OrderState.Order" />
        </div>
    </div>

    <button class="checkout-button btn btn-warning" @onclick="PlaceOrder">
        Place order
    </button>
</div>

@code {
    async Task PlaceOrder()
    {
        var newOrderId = await HttpClient.PostJsonAsync<int>("orders", OrderState.Order);
        OrderState.ResetOrder();
        UriHelper.NavigateTo($"myorders/{newOrderId}");
    }
}
```

Back in Index

Delete PlaceHolder method

Replace Button with link ...

**----------------- Index.razor -----------------------**
```
<a href="checkout" class="btn btn-warning" disabled="@(OrderState.Order.Pizzas.Count == 0)">
    Checkout >
</a>
```

**Build and Run**

*******************************************************************
# Now lets create a resusable Address Editor Component

Create: AddressEditor.razor in shared

**----------------- AddressEditor.razor -----------------------**
```
<div class="form-field">
    <label>Name:</label>
    <div>
        <input @bind="Address.Name" />
    </div>
</div>

<div class="form-field">
    <label>Line 1:</label>
    <div>
        <input @bind="Address.Line1" />
    </div>
</div>

<div class="form-field">
    <label>Line 2:</label>
    <div>
        <input @bind="Address.Line2" />
    </div>
</div>

<div class="form-field">
    <label>City:</label>
    <div>
        <input @bind="Address.City" />
    </div>
</div>

<div class="form-field">
    <label>Region:</label>
    <div>
        <input @bind="Address.Region" />
    </div>
</div>

<div class="form-field">
    <label>Postal code:</label>
    <div>
        <input @bind="Address.PostalCode" />
    </div>
</div>

@code {
    [Parameter] public Address Address { get; set; }
}
```

Inside `<div class="checkout-cols">` below 1st div, add ...

**----------------- Checkout.razor -----------------------**
```
    <div class="checkout-delivery-address">
        <h4>Deliver to...</h4>
        <AddressEditor Address="@OrderState.Order.DeliveryAddress" />
    </div>
```
- ***Build and Run***
- Show that we can submit with no address

---
# Adding Validation

1st add server side validation

----------------- Address.cs -----------------------
```
	public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string Line1 { get; set; }

        [MaxLength(100)]
        public string Line2 { get; set; }

        [Required, MaxLength(50)]
        public string City { get; set; }

        [Required, MaxLength(20)]
        public string Region { get; set; }

        [Required, MaxLength(20)]
        public string PostalCode { get; set; }
```

- **Build and Run**
- Show 400 Bad Request

*******************************************************************
# Now need to add client side validation

To do that we will ...

- add an EditForm component around contents of main div, 
- With a <DataAnnotationsValidator /> and <ValidationSummary /> at the bottom of the EditForm
- EditForm renders a form tag but also sets up an EditContext that tracks changs and helps with validation

**----------------- Checkout.razor -----------------------**
```
    <EditForm Model="OrderState.Order.DeliveryAddress">
        <div class="checkout-cols">
            <div class="checkout-order-details">
                <h4>Review order</h4>
                <OrderReview Order="OrderState.Order" />
            </div>

            <div class="checkout-delivery-address">
                <h4>Deliver to...</h4>
                <AddressEditor Address="@OrderState.Order.DeliveryAddress" />
            </div>
        </div>

        <button class="checkout-button btn btn-warning" @onclick="PlaceOrder">
            Place order
        </button>
		
		<DataAnnotationsValidator />
		<ValidationSummary />
    </EditForm>
```    
- **Build and Run**
- This is ugly, lets make it better

**----------------- Checkout.razor -----------------------**
- Remove:  `<ValidationSummary />`
- Change EditForm tag: add `OnValidSubmit="PlaceOrder"`
- Change button to submit and remove onclick: `<button type="submit">`

Now in AddressEditor.razor

Add ValidationMessage to each input item

**----------------- AddressEditor.razor -----------------------**
```
<div class="form-field">
    <label>Name:</label>
    <div>
        <input @bind="Address.Name" />
        <ValidationMessage For="@(() => Address.Name)" />
    </div>
</div>

<div class="form-field">
    <label>Line 1:</label>
    <div>
        <input @bind="Address.Line1" />
        <ValidationMessage For="@(() => Address.Line1)" />
    </div>
</div>

<div class="form-field">
    <label>Line 2:</label>
    <div>
        <input @bind="Address.Line2" />
        <ValidationMessage For="@(() => Address.Line2)" />
    </div>
</div>

<div class="form-field">
    <label>City:</label>
    <div>
        <input @bind="Address.City" />
        <ValidationMessage For="@(() => Address.City)" />
    </div>
</div>

<div class="form-field">
    <label>Region:</label>
    <div>
        <input @bind="Address.Region" />
        <ValidationMessage For="@(() => Address.Region)" />
    </div>
</div>

<div class="form-field">
    <label>Postal code:</label>
    <div>
        <input @bind="Address.PostalCode" />
        <ValidationMessage For="@(() => Address.PostalCode)" />
    </div>
</div>
```

- **Build and Run**
- But notice the error messages don't go away when we fill out the fields
- Lets fix that with InpuText component
- InputText isn't the only built-in input component. Others include InputCheckbox, InputDate, InputSelect, etc.
- Note: @bind changes to @bind-Value (note: upper case V)

**----------------- AddressEditor.razor -----------------------**
```
<div class="form-field">
    <label>Name:</label>
    <div>
        <InputText @bind-Value="Address.Name" />
        <ValidationMessage For="@(() => Address.Name)" />
    </div>
</div>

<div class="form-field">
    <label>Line 1:</label>
    <div>
        <InputText @bind-Value="Address.Line1" />
        <ValidationMessage For="@(() => Address.Line1)" />
    </div>
</div>

<div class="form-field">
    <label>Line 2:</label>
    <div>
        <InputText @bind-Value="Address.Line2" />
        <ValidationMessage For="@(() => Address.Line2)" />
    </div>
</div>

<div class="form-field">
    <label>City:</label>
    <div>
        <InputText @bind-Value="Address.City" />
        <ValidationMessage For="@(() => Address.City)" />
    </div>
</div>

<div class="form-field">
    <label>Region:</label>
    <div>
        <InputText @bind-Value="Address.Region" />
        <ValidationMessage For="@(() => Address.Region)" />
    </div>
</div>

<div class="form-field">
    <label>Postal code:</label>
    <div>
        <InputText @bind-Value="Address.PostalCode" />
        <ValidationMessage For="@(() => Address.PostalCode)" />
    </div>
</div>
```

- **Build and Run**
- Note: Red and Green can be styled

---
# Authentication and Authorization

The server side of our application has already been configured to do OAuth with Twitter

- Show: appsettings.Development.json
- Show: Startup.cs

We just need to turn it on

**----------------- OrderController.cs -----------------------**

Uncomment `[Authorize]`

**----------------- OrderController.cs -----------------------**

**Build and run**

Try to view orders => we can no longer do anything with orders (because we are not authorized)

- So now we need to enforce authorization on the client side
- In client project create: ServerAuthenticationStateProvider.cs

**----------------- ServerAuthenticationStateProvider.razor -----------------------**
```
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazingPizza.Client
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Currently, this returns fake data
            // In a moment, we'll get real data from the server
            var claim = new Claim(ClaimTypes.Name, "Fake user");
            var identity = new ClaimsIdentity(new[] { claim }, "serverauth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}
```

Note: for now this is just a fake user

Now register this with the DI service in Startup.cs

**----------------- Startup.cs -----------------------**
```
    // Add auth services
    services.AddAuthorizationCore();
    services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
```

Modify App.Razor

**----------------- App.Razor -----------------------**
```
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly">
        <NotFoundContent>Page not found</NotFoundContent>
    </Router>
</CascadingAuthenticationState>
```

This has made available a cascading parameter to all descendant components. 

Now create a new LoginDisplay component in Shared

**----------------- LoginDisplay.Razor -----------------------**
```
<div class="user-info">
    <AuthorizeView>
        <Authorizing>
            <text>...</text>
        </Authorizing>
        <Authorized>
            <img src="img/user.svg" />
            <div>
                <span class="username">@context.User.Identity.Name</span>
                <a class="sign-out" href="user/signout">Sign out</a>
            </div>
        </Authorized>
        <NotAuthorized>
            <a class="sign-in" href="user/signin">Sign in</a>
        </NotAuthorized>
    </AuthorizeView>
</div>
```
`<AuthorizeView>` is a built-in component that displays different content depending on whether the user meets specified authorization conditions. 

Now lets add the LoginDisplay component to the MainLayout

**----------------- MainLayout.Razor -----------------------**
```
<div class="top-bar">
    (... leave existing content in place ...)

    <LoginDisplay />
</div>
```

**Build and Run**

Note: you will not be able to sign-out or sign-in because this user is faked

So now let's finish our ServerAuthenticationStateProvider ...

**----------------- ServerAuthenticationStateProvider.cs -----------------------**
```
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazingPizza.Client
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;

        public ServerAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var userInfo = await _httpClient.GetJsonAsync<UserInfo>("user");

            var identity = userInfo.IsAuthenticated
                ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userInfo.Name) }, "serverauth")
                : new ClaimsIdentity();

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}
```

**Build and Run**

Test Sign-in

Test Sign-out

Sign-out: Notice we can still try to place an order when signed out - lets fix that

**----------------- Checkout.razor -----------------------**

in @code add ...

```
    [CascadingParameter] Task<AuthenticationState> AuthenticationStateTask { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateTask;
        if (!authState.User.Identity.IsAuthenticated)
        {
            // The server won't accept orders from unauthenticated users, so avoid
            // an error by making them log in at this point
            UriHelper.NavigateTo("user/signin?redirectUri=/checkout", true);
        }
    }
```

also need to modify the UI as follow (then explain) ...

```
    <div class="main">
        <AuthorizeView Context="authContext">
            <NotAuthorized>
                <h2>Redirecting you...</h2>
            </NotAuthorized>
            <Authorized>
                <EditForm Model="OrderState.Order.DeliveryAddress" OnValidSubmit="PlaceOrder">
                    <div class="checkout-cols">
                        <div class="checkout-order-details">
                            <h4>Review order</h4>
                            <OrderReview Order="OrderState.Order" />
                        </div>

                        <div class="checkout-delivery-address">
                            <h4>Deliver to...</h4>
                            <AddressEditor Address="@OrderState.Order.DeliveryAddress" />
                        </div>
                    </div>

                    <button type="submit">
                        Place order
                    </button>

                    <DataAnnotationsValidator />
                </EditForm>
            </Authorized>
        </AuthorizeView>
    </div>
```

**Build and run**

We've just introduced a pretty serious defect into the application. 

Login redirect loses OrderState

Fix it with session Storage

Show sessionStorage.js

Add SessionStorage.cs

**----------------- SessionStorage.cs -----------------------**
```
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingPizza.Client
{
    public static class SessionStorage
    {
        public static Task<T> GetAsync<T>(IJSRuntime jsRuntime, string key)
            => jsRuntime.InvokeAsync<T>("blazorSessionStorage.get", key);

        public static Task SetAsync(IJSRuntime jsRuntime, string key, object value)
            => jsRuntime.InvokeAsync<object>("blazorSessionStorage.set", key, value);

        public static Task DeleteAsync(IJSRuntime jsRuntime, string key)
            => jsRuntime.InvokeAsync<object>("blazorSessionStorage.delete", key);
    }
}
```

**----------------- Checkout.razor -----------------------**
```
@inject IJSRuntime JSRuntime
```

Then, inside OnInitializedAsync, add the following line just above the UriHelper.NavigateTo call:

```
	await SessionStorage.SetAsync(JSRuntime, "currentorder", OrderState.Order);
```

At the bottom of OnInitializedAsync ...

```
        // Try to recover any temporary saved order
        if (!OrderState.Order.Pizzas.Any())
        {
            var savedOrder = await SessionStorage.GetAsync<Order>(JSRuntime, "currentorder");
            if (savedOrder != null)
            {
                OrderState.ReplaceOrder(savedOrder);
                await SessionStorage.DeleteAsync(JSRuntime, "currentorder");
            }
            else
            {
                // There's nothing check out - go to home
                UriHelper.NavigateTo("");
            }
        }
```

**Build and Run**

Now you should no longer be able to reproduce the "lost order state" bug. Your order should be preserved across the redirection flow.

Sign-out and Visit My Orders => Request will be rejected

Don't close app yet - do next step

---
# Signout and show MyOrders 

Just says loading...

This is because we are not authorized

You can place an `[Authorize]` attribute on a routable @page component. 

This is useful if you want to control the reachability of an entire page based on authorization conditions.

**----------------- MyOrders.razor -----------------------**
```
@attribute [Authorize]
```

**Build and run**

Go to My Orders and show Not Authorized

We can do better than that ...

**----------------- App.razor -----------------------**
```
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly">
        <NotFoundContent>Page not found</NotFoundContent>

        <NotAuthorizedContent>
            <div class="main">
                <h2>You're signed out</h2>
                <p>To continue, please sign in.</p>
                <a class="btn btn-danger" href="user/signin">Sign in</a>
            </div>
        </NotAuthorizedContent>

        <AuthorizingContent>
            Please wait...
        </AuthorizingContent>
    </Router>
</CascadingAuthenticationState>
```

**Build and Run**

Now if you're logged out and try to go to My orders, you'll get a much nicer outcome

While signed out ... navigate to: /myorders/1

**----------------- OrderDetails.razor -----------------------**
```
@attribute [Authorize]
```


**Build and Run**

---
# Authorization

Although the server requires authentication before accepting queries for order information, it still doesn't distinguish between users. 
All signed-in users can see the orders from all other signed-in users. We have authentication, but no authorization!

```
OrdersController => order.UserId = GetUserId()
```

Now each order will be stamped with the ID of the user who owns it.

---
# More on Javascript Interop

Users of the pizza store can now track the status of their orders in real time. In this session we'll use JavaScript interop to add a 
real-time map to the order status page that answers the age old question, "Where's my pizza?!?".

In this situation we already have a map built using javascript, and just want to use it in our blazor application

show javascript: deliveryMap.js

Add new component to shared:  Map.razor

**----------------- Map.razor -----------------------**
```
@using Microsoft.JSInterop
@inject IJSRuntime JSRuntime

<div id="@elementId" style="height: 100%; width: 100%;"></div>

@code {
    string elementId = $"map-{Guid.NewGuid().ToString("D")}";
    
    [Parameter] public double Zoom { get; set; }
    [Parameter] public List<Marker> Markers { get; set; }

    protected async override Task OnAfterRenderAsync()
    {
        await JSRuntime.InvokeAsync<object>(
            "deliveryMap.showOrUpdate",
            elementId,
            Markers);
    }
}
```

Explain code

Add the Map component to the *OrderDetails* page by adding the following just below the `track-order-details` div:

**----------------- OrderDetails.razor -----------------------**
```
<div class="track-order-map">
    <Map Zoom="13" Markers="orderWithStatus.MapMarkers" />
</div>
```


# Notes to Self:
- Close by mentioning these links and going back to powerpoint
- https://github.com/dotnet-presentations/blazor-workshop
- https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-3.0
- Shortcut Reminder: Shift+F2



