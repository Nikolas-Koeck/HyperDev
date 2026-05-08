using HyperDev.src.Viewmodels;

namespace HyperDev.src.Views;

public sealed class GitContentView : ContentView {
    private readonly GitContentViewModel _vm;
    private readonly CollectionView _branchesView;

    public GitContentView(GitContentViewModel viewModel)
    {
        _vm = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _vm;

        // Repository entry + Load button
        var repoEntry = new Entry
        {
            Placeholder = "Repository path",
            HorizontalOptions = LayoutOptions.FillAndExpand
        };
        repoEntry.SetBinding(Entry.TextProperty, nameof(GitContentViewModel.RepositoryPath), BindingMode.TwoWay);

        var loadButton = new Button
        {
            Text = "Load",
            HorizontalOptions = LayoutOptions.End
        };
        loadButton.Clicked += async (_, _) => await OnLoadClickedAsync();

        var repoRow = new HorizontalStackLayout
        {
            Spacing = 8,
            Children = { repoEntry, loadButton }
        };

        // Status label and busy indicator
        var statusLabel = new Label { LineBreakMode = LineBreakMode.WordWrap };
        statusLabel.SetBinding(Label.TextProperty, nameof(GitContentViewModel.Status));

        var busy = new ActivityIndicator { VerticalOptions = LayoutOptions.Center };
        busy.SetBinding(ActivityIndicator.IsRunningProperty, nameof(GitContentViewModel.IsBusy));
        busy.SetBinding(ActivityIndicator.IsVisibleProperty, nameof(GitContentViewModel.IsBusy));

        // Branches collection
        _branchesView = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            ItemTemplate = new DataTemplate(() =>
            {
                var lbl = new Label { Padding = new Thickness(8, 12) };
                lbl.SetBinding(Label.TextProperty, ".");
                return new ContentView { Content = lbl };
            }),
            HeightRequest = 220
        };
        _branchesView.SetBinding(ItemsView.ItemsSourceProperty, nameof(GitContentViewModel.Branches));
        _branchesView.SelectionChanged += BranchesView_SelectionChanged;

        // New branch input + create button
        var newBranchEntry = new Entry { Placeholder = "New branch name" };
        newBranchEntry.SetBinding(Entry.TextProperty, nameof(GitContentViewModel.NewBranchName), BindingMode.TwoWay);

        var createBranchButton = new Button { Text = "Create Branch" };
        createBranchButton.Clicked += (_, _) =>
        {
            // Prefer ICommand if provided; otherwise call through Execute pattern.
            if(_vm.CreateBranchCommand?.CanExecute(null) == true)
                _vm.CreateBranchCommand.Execute(null);
            else
                _ = _vm.CreateBranchAsync();
        };

        var createRow = new HorizontalStackLayout { Spacing = 8, Children = { newBranchEntry, createBranchButton } };

        // Commit input + commit button
        var commitEntry = new Entry { Placeholder = "Commit message" };
        commitEntry.SetBinding(Entry.TextProperty, nameof(GitContentViewModel.CommitMessage), BindingMode.TwoWay);

        var commitButton = new Button { Text = "Commit" };
        commitButton.Clicked += (_, _) =>
        {
            if(_vm.CommitCommand?.CanExecute(null) == true)
                _vm.CommitCommand.Execute(null);
            else
                _ = _vm.CommitAsync();
        };

        var commitRow = new HorizontalStackLayout { Spacing = 8, Children = { commitEntry, commitButton } };

        // Push / Pull buttons
        var pushButton = new Button { Text = "Push" };
        pushButton.Clicked += (_, _) =>
        {
            if(_vm.PushCommand?.CanExecute(null) == true)
                _vm.PushCommand.Execute(null);
            else
                _ = _vm.PushAsync();
        };

        var pullButton = new Button { Text = "Pull" };
        pullButton.Clicked += (_, _) =>
        {
            if(_vm.PullCommand?.CanExecute(null) == true)
                _vm.PullCommand.Execute(null);
            else
                _ = _vm.PullAsync();
        };

        var actionsRow = new HorizontalStackLayout { Spacing = 8, Children = { pushButton, pullButton } };

        // Refresh button (explicit; some viewmodels may expose RefreshCommand)
        var refreshButton = new Button { Text = "Refresh" };
        refreshButton.Clicked += async (_, _) =>
        {
            // Prefer command if available
            if(_vm.RefreshCommand?.CanExecute(null) == true)
                _vm.RefreshCommand.Execute(null);
            else
                await _vm.LoadAsync(_vm.RepositoryPath);
        };

        // Put it all together
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(16),
                Spacing = 12,
                Children =
                {
                    repoRow,
                    new HorizontalStackLayout { Spacing = 8, Children = { busy, statusLabel } },
                    refreshButton,
                    new Label { Text = "Branches", FontAttributes = FontAttributes.Bold },
                    _branchesView,
                    createRow,
                    new Label { Text = "Commit", FontAttributes = FontAttributes.Bold },
                    commitRow,
                    actionsRow
                }
            }
        };
    }

    // Convenience constructor for when a service is registered and viewmodel needs to be created elsewhere.
    public GitContentView(Func<GitContentViewModel> vmFactory) : this(vmFactory?.Invoke() ?? throw new ArgumentNullException(nameof(vmFactory)))
    {
    }

    private async Task OnLoadClickedAsync()
    {
        // call LoadAsync and surface exceptions minimally
        try
        {
            await _vm.LoadAsync(_vm.RepositoryPath);
        }
        catch(Exception ex)
        {
            var page = Application.Current?.MainPage;
            if(page != null)
                await page.DisplayAlert("Load error", ex.Message, "OK");
        }
    }

    private void BranchesView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection?.FirstOrDefault() as string;
        if(string.IsNullOrWhiteSpace(selected))
            return;

        try
        {
            if(_vm.CheckoutCommand?.CanExecute(selected) == true)
            {
                _vm.CheckoutCommand.Execute(selected);
            }
            else
            {
                // Fallback if command is not set up
                _ = _vm.CheckoutAsync(selected);
            }
        }
        finally
        {
            // clear selection so the same item can be selected again
            if(_branchesView.SelectedItem != null)
                _branchesView.SelectedItem = null;
        }
    }
}