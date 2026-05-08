using HyperDev.src.Viewmodels;

namespace HyperDev.src.Views {
    public partial class GitHubProjectView : ContentView {
        ProjectDetailViewModel projectDetailViewModel;
        public GitHubProjectView(ProjectDetailViewModel _project)
        {
            this.projectDetailViewModel = _project;

            projectDetailViewModel.LoadItemsAsync();
            // Ensure bindings resolve against the VM
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
                var title = new Label { LineBreakMode = LineBreakMode.TailTruncation };
                title.SetBinding(Label.TextProperty, "Title");

                var container = new Frame
                {
                    Padding = 8,
                    Margin = new Thickness(4),
                    BorderColor = Microsoft.Maui.Graphics.Colors.LightGray,
                    Content = title
                };

                return container;
            });

            return collectionView;
        }
    }
}
