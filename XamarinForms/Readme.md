# Introduction

This is a presentation I gave at the SORT 2019 conference.  It introduced the group to Xamarin Forms and cross platform development using C#

# Starting Point
The Starting Point zip file is the starting point for the project - if you want to do the walk through on your own you will want to start here.

https://github.com/worthingtonjg/Sort2019/blob/master/XamarinForms/00-Starting-point.zip

# Powerpoint

The powerpoint for my presentation is:

https://github.com/worthingtonjg/Sort2019/blob/master/XamarinForms/Final%20-%20Xamarin%20Forms%20-%20Sort%202019%20-%20Jon%20Worthington.pptx

# Pre-session Prep

- Open powerpoint
- Open https://rbscorekeeper.azurewebsites.net
- Open https://rbscorekeeper.azurewebsites.net/swagger
- Open starting point and build on both android and ios
- Open finished app and have it running
- Connect and test bluetooth buttons and finished app

# Xamarin Forms Walkthrough

New => Multiplatform => App => 

Discuss templates => Including Xamarin Native

Choose: Tabbed Forms App

Discuss 3 projects:
- Shared => Will contain most your UI and code
- Android => MainActivity.cs => 
- iOS => Main.cs => AppDelegate.cs

Open starting point project
- Common
- Models
- Services
- ViewModels
- NewtonSoft

Android => Resources 

iOS => Resources

Show Main Page.xaml => TabbedPage

Show how to add a new page

Show MatchPage

**Build and run**

Stop

---
# Match Page

So let’s start making our match page.

To do that let’s start in our view model

Open *MatchPageViewModel.cs*

We want to be able to Start a Match and End a Match, do that we will need some variables to keep track of our match

```
        public Match Match { get; set; }
        public bool MatchIsActive { get; set; }
```

To start a match we will need to select players, so lets add a list of players …

```
	public List<Player> Players { get; set; }
```

Once a match is started, we need to show the match, so lets add a URL for our web page 

```
        public string RbScoreKeeperSite { get; set; }

        public async Task LoadAsync()
        {
            var match = await HttpHelper.Instance.GetAsync<Match>("match");
            SetValue(() => Match, match);
            SetValue(() => MatchIsActive, match != null);
        }
```

If the match is active, then we will refresh the url …

```
            if (MatchIsActive)
            {
                SetValue(() => RbScoreKeeperSite, "https://rbscorekeeper.azurewebsites.net/mobile");
            }
```

Otherwise we will set the list of players …

```
            else
            {
                var players = await HttpHelper.Instance.GetListAsync<Player>("players");
                SetValue(() => Players, players);
            }
```

**--------------------------------- MatchPage.xaml.cs ---------------------------------**
```
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await viewModel.LoadAsync();
        }
```

**Build and Run**

Lets remove the Label and change the StackLayout to a grid

```
        <Grid>
            <!-- Match is active-->
            <StackLayout IsVisible="{Binding MatchIsActive}">
                <Label>Match is active</Label>
            </StackLayout>

            <!-- Match is not active-->
            <StackLayout IsVisible="{Binding MatchIsActive, Converter={StaticResource InverseBooleanConverter}}">
                <Label>Match is NOT active</Label>
            </StackLayout>
        </Grid>
```

If the match is not active, lets show a list of players

lets delete the label 

And add a list view 

```
                <ListView ItemsSource="{Binding Players}" SelectionMode="None">
                </ListView>
```

Okay now we need to the list view how to render our player …

```
                    <ListView.ItemTemplate>
                        <DataTemplate>
                            <TextCell Text="{Binding Name}"></TextCell>
                        </DataTemplate>
                    </ListView.ItemTemplate>
```

Okay, but this isn’t really what we want, we also want to show a checkbox, so we can select our players
To do that we need to use a ViewCell

```
                            <ViewCell>
                                <StackLayout Orientation="Horizontal" Margin="5">
                                    <CheckBox IsChecked="{Binding Selected}" Color="DarkGray" WidthRequest="30" />
                                    <Label Text="{Binding Name}" VerticalOptions="Center"></Label>
                                </StackLayout>
                            </ViewCell>
```

Notice the Binding Errors: ‘Selected’ property not found

We will fix that in a minute

Now lets add a button to start the match …

```
<Button Text="Start Match" Command="{Binding StartMatchCommand}"></Button>
```

Notice another binding error for the StartMatchCommand

Lets fix the binding error

**Stop**

We can’t bind to a list of Player, lets replace it with a view model that has a selected property

```
        public List<MatchPlayersViewModel> Players { get; set; }
```

And lets change the load async to use the MatchPlayersViewModel …

```
                SetValue(() => Players, players.Select(p => new MatchPlayersViewModel(p)).ToList());
```

Now we need to add a command binding for our “Start Match” button, and while we are at it lets create one for End Match as well …

```
        public Command StartMatchCommand { get; set; }
        public Command EndMatchCommand { get; set; }
```

In the ctor, we will wire them up …

```
            StartMatchCommand = new Command(async () => await DoStartMatchCommand());
            EndMatchCommand = new Command(async () => await DoEndMatchCommand());
```

And their implementations ….

```
        private async Task DoStartMatchCommand()
        {
            var selected = Players.Where(p => p.Selected).ToList();

            if (selected.Count < 2) return;

            var playerIds = selected.Select(s => s.Id).ToList();

            await HttpHelper.Instance.PostAsync($"match/create?winningScore=15&oneButtonMode=true", playerIds);
            await LoadAsync();
        }

        private async Task DoEndMatchCommand()
        {
            await HttpHelper.Instance.PostAsync("match/end", "");
            await LoadAsync();
        }
```

Let’s build and run again and see what we have…

Delete the label

Lets change the StakeLayout to a Grid

And add two row definitions …

```
                <Grid.RowDefinitions>
                    <RowDefinition Height="*"></RowDefinition>
                    <RowDefinition Height="Auto"></RowDefinition>
                </Grid.RowDefinitions>
```

Add a web view in grid row 0 …

```
	<WebView 
                    Grid.Row="0" 
                    Source="{Binding RbScoreKeeperSite}"></WebView>
```

Add stack layout in grid row 1 with button inside

```
                <StackLayout Grid.Row="1">
                    <Button Text="End Match" Command="{Binding EndMatchCommand}"></Button>
                </StackLayout>
```

Show EndMatch and StartMatch, notice you can see stats for a split second, lets add a Loading property

```
        public bool Loading { get; set; }
```

Ctor, and top of Load Async …

```
            SetValue(() => Loading, true);
```

At bottom of LoadAsync …

```
	SetValue(() => Loading, false);
```

Add at bottom

```
            <Grid BackgroundColor="White" IsVisible="{Binding Loading}">
                <ActivityIndicator IsRunning="{Binding Loading}" HorizontalOptions="Center" VerticalOptions="Center" />
            </Grid>
```

---
# Navigation

So now let’s add some more tabs to our MainPage for stats, players and flics …

```
        <NavigationPage Title="Stats">
            <NavigationPage.Icon>
                <OnPlatform x:TypeArguments="FileImageSource">
                    <On Platform="iOS" Value="tab_stats.png"/>
                </OnPlatform>
            </NavigationPage.Icon>
            <x:Arguments>
                <views:StatsPage />
            </x:Arguments>
        </NavigationPage> 
        <NavigationPage Title="Players">
            <NavigationPage.Icon>
                <OnPlatform x:TypeArguments="FileImageSource">
                    <On Platform="iOS" Value="users_flic.png" />
                </OnPlatform>
            </NavigationPage.Icon>
            <x:Arguments>
                <v:PlayersPage></v:PlayersPage>
            </x:Arguments>
        </NavigationPage>
        <NavigationPage Title="Flics">
            <NavigationPage.Icon>
                <OnPlatform x:TypeArguments="FileImageSource">
                    <On Platform="iOS" Value="tab_flic.png" />
                </OnPlatform>
            </NavigationPage.Icon>
            <x:Arguments>
                <v:FlicPage></v:FlicPage>
            </x:Arguments>
        </NavigationPage>
```

**Build and Run**

Click on stats tab (don’t click on players or flics)

Now we will implement the stats page (since that is an easy one)

---
# Stats Page

```
        <Grid>
            <WebView x:Name="WebView1" Source="https://rbscorekeeper.azurewebsites.net/mobile/stats"></WebView>
        </Grid>
```

**Build and Run**

Stop

---
# Players Page

Now lets start our players page

I’ve already created a players page

The view model already has …

- a list of players defined, 
- a LoadAsync method that gets the list of players
- And commands stubbed for adding and deleting players 

The LoadAsync method is already being called in OnAppearing method 

And in the Xaml …

- we’ve defined and ImageButton that uses a Delete image
- Add and add player button

**Build and Run**

Show that add player works

Show the binding errors when the page is loaded

Explain why.

Fix delete command

Add name to Content Page …

```
 x:Name="Root"
```

Change the command binding for the delete …

```
Command="{Binding Source={x:Reference Root}, Path=BindingContext.DeletePlayerCommand}" 
```

Build and Run

Talk about Command Parameters, show in view model

Add ….

```
CommandParameter = "{Binding }"
```

Show it working

When we add a player we want to show a new page, so lets add AddItemPage
 
AddItemPage has already been created ahead of time

Show ViewModel and walk through each part

Show AddItemPage.xaml.cs, talk about changes to constructor

Show XAML and talk about Toolbar and Entry

PlayersViewModel.cs

Now lets go back to our PlayersViewModel and figure out how to show this new page …

So what we need to do now is use Navigation to Push a new page onto the stack of pages

Show BaseViewModel, and reference to Page.Navigation

Lets change AddPlayer to do this …

```
        private async Task AddPlayer()
        {
            await Navigation.PushAsync(new AddItemPage("Player"));
        }
```

Show ctor of AddItemPage, and it calling SetType

**Build and Run**

Show Add Item

Now let’s make the save button work.

To do this we are going to use the message center.  

In our AddItemPageViewModel lets modify the Save method

```
            SaveCommand = new Command(() => {
                MessagingCenter.Send(this, $"DoSave{Type}", Name);
            });
```

Message center is a pub / sub pattern that allows us to send messages between controls.

So receive that message lets change our PlayersPageViewModel …

In PlayerPageViewModel ctor …

```
MessagingCenter.Subscribe<AddItemPageViewModel, string>(this, "DoSavePlayer", async(s, a) => await DoSavePlayer(s, a));
```

And add … 

```
        private async Task DoSavePlayer(AddItemPageViewModel sender, string newPlayerName)
        {
            await HttpHelper.Instance.PostAsync($"players?name={newPlayerName}", "");
            await Navigation.PopAsync();
        }
```

**Build and Demo**

Now lets make delete work …

```
        private async Task DeletePlayer(object o)
        {
            Player p = o as Player;

            bool confirm = await Page.DisplayAlert("Confirm Delete", $"Delete Player: {p.Name}", "Accept", "Cancel");
            if (!confirm) return;

            await HttpHelper.Instance.DeleteAsync($"players/{p.PlayerId}");
            await LoadAsync();
        }
```

**Build and Run**

---
# Flic Page

The Flic page, is exactly the same code as the Player page - show view model briefly

We are reusing the same AddItemPage to add Flics

Build and show Flic page

---
# Conclusion

There is a lot more to Xamarin Forms, but this gives a good idea of how it works and how to get started.

Recap pages, show what we’ve build, run in iOS if necessary

Back to presentation to show links and thank you page






