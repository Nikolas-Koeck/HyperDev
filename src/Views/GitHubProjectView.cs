using HyperDev.src.Viewmodels;

namespace HyperDev.src.Views {
    public partial class GitHubProjectView : ContentView {
        ProjectDetailViewModel projectDetailViewModel;
        public GitHubProjectView(ProjectDetailViewModel _project)
        {
            this.projectDetailViewModel = _project;

            projectDetailViewModel.LoadItemsAsync();
            BindingContext = projectDetailViewModel;

            var titleLabel = new Label
            {
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Start
            };
            titleLabel.SetBinding(Label.TextProperty, nameof(ProjectDetailViewModel.ProjectTitle));

            var descriptionLabel = new Label
            {
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            descriptionLabel.SetBinding(Label.TextProperty, nameof(ProjectDetailViewModel.ProjectDescription));

            var collectionView = CreateItemCollection();
            Content = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
                Spacing = 8,
                Children =
                {
                    titleLabel,
                    descriptionLabel,
                    collectionView
                }
            };
        }

        private CollectionView CreateItemCollection()
        {
            CollectionView collectionView = new CollectionView
            {
                SelectionMode = SelectionMode.None,
                HeightRequest = 220
            };

            collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(ProjectDetailViewModel.Items));
            collectionView.ItemTemplate = new DataTemplate(() =>
            {
                // Creator name (small, muted)
                var creator = new Label
                {
                    FontSize = 12,
                    TextColor = Microsoft.Maui.Graphics.Colors.Gray,
                    LineBreakMode = LineBreakMode.TailTruncation
                };
                creator.SetBinding(Label.TextProperty, "CreatorName");

                // Title (primary)
                var title = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = LineBreakMode.TailTruncation
                };
                title.SetBinding(Label.TextProperty, "Title");

                // Item URL (smaller, blue, underlined) - shows "Open link" as placeholder when URL is null
                var itemUrl = new Label
                {
                    FontSize = 12,
                    TextColor = Microsoft.Maui.Graphics.Colors.Blue,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    TextDecorations = TextDecorations.Underline
                };
                itemUrl.SetBinding(Label.TextProperty, new Binding("ItemUrl") { TargetNullValue = "Open link", FallbackValue = "Open link" });

                // Tap to open URL in browser. We resolve the actual ItemUrl from the BindingContext to avoid showing placeholder text as the URL.
                var tap = new TapGestureRecognizer();
                tap.Tapped += async (s, e) =>
                {
                    if(s is not Label lbl) return;
                    var item = lbl.BindingContext;
                    if(item == null) return;
                    var prop = item.GetType().GetProperty("ItemUrl");
                    if(prop == null) return;
                    var url = prop.GetValue(item) as string;
                    if(string.IsNullOrWhiteSpace(url)) return;

                    try
                    {
                        await Launcher.OpenAsync(new Uri(url));
                    }
                    catch(Exception)
                    {
                        // Swallow for now; optionally show an alert or log the error
                    }
                };
                itemUrl.GestureRecognizers.Add(tap);

                var stack = new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        creator,
                        title,
                        itemUrl
                    }
                };

                var container = new Frame
                {
                    Padding = 8,
                    Margin = new Thickness(4),
                    BorderColor = Microsoft.Maui.Graphics.Colors.LightGray,
                    Content = stack
                };

                return container;
            });

            return collectionView;
        }
    }
}
