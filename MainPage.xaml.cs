using HyperDev.src.Services;

namespace HyperDev;

public partial class MainPage : ContentPage {
    int count = 0;
    GitHubProjectService _service;
    public MainPage()
    {
        InitializeComponent();

        // Resolve the registered service from the app service provider (no hard-coded token).
        _service = MauiProgram.Services.GetRequiredService<GitHubProjectService>();
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

    private async void OnLoadItemsClicked(object sender, EventArgs e)
    {
        var items = await _service.GetProjectItemsAsync(
            organization: "simon-hegele",
            projectNumber: 14);

        foreach(var item in items)
        {
            Console.WriteLine($"{item.Title} ({item.ContentType})");
        }
    }

    private void DropGestureRecognizer_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }
}
