using HyperDev.src.Services;
using HyperDev.src.Viewmodels;
using HyperDev.src.Views;

namespace HyperDev;

public partial class MainPage : ContentPage {
    ProjectDetailViewModel projectDetailViewModel;
    GitHubProjectView projectDetailView;

    // Added fields for the Git view
    GitContentViewModel gitContentViewModel;
    GitContentView gitContentView;

    public MainPage()
    {
        InitializeComponent();

        var githubService = MauiProgram.Services.GetRequiredService<GitHubProjectService>();
        this.projectDetailViewModel = new ProjectDetailViewModel(githubService, "simon-hegele", 14);
        this.projectDetailView = new GitHubProjectView(projectDetailViewModel);

        rootLayout.Children.Add(projectDetailView);

        // Create and add the GitContentView using the IGitService from DI
        var gitService = MauiProgram.Services.GetRequiredService<IGitService>();
        this.gitContentViewModel = new GitContentViewModel(gitService);
        this.gitContentView = new GitContentView(gitContentViewModel);

        rootLayout.Children.Add(gitContentView);
    }

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        var label = sender as Label;
        var feature = label?.BindingContext as Feature;

        if(feature != null)
        {
            e.Data.Text = feature.Name;
        }
    }

    private async void OnDrop(object sender, DropEventArgs e)
    {
        string featureName = await e.Data.GetTextAsync();

        var frame = sender as Frame;
        var slot = frame?.BindingContext as Slot;

        if(slot == null || slot.IsLocked)
            return;

        // find feature in ViewModel
        var vm = BindingContext as MainViewModel;
        var feature = vm?.Features.FirstOrDefault(f => f.Name == featureName);

        if(feature != null)
        {
            slot.AssignedFeature = feature;

            // force UI update (simple way)
            var index = vm.Slots.IndexOf(slot);
            vm.Slots[index] = slot;
        }
    }


    private void DropGestureRecognizer_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

}
