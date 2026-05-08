using HyperDev.src.Models;
using HyperDev.src.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HyperDev.src.Viewmodels;

public class ProjectDetailViewModel : INotifyPropertyChanged {
    public ObservableCollection<ProjectItem> Items { get; } = new();

    private readonly GitHubProjectService? githubService;
    public string Organization { get; init; }
    public int ProjectId { get; init; }

    private string? _projectTitle;
    public string? ProjectTitle
    {
        get => _projectTitle;
        private set => SetProperty(ref _projectTitle, value);
    }

    private string? _projectDescription;
    public string? ProjectDescription
    {
        get => _projectDescription;
        private set => SetProperty(ref _projectDescription, value);
    }

    private string? _projectUrl;
    public string? ProjectUrl
    {
        get => _projectUrl;
        private set => SetProperty(ref _projectUrl, value);
    }

    public bool HasProjectUrl => !string.IsNullOrWhiteSpace(ProjectUrl);

    public string ItemCountText => $"{Items.Count} item(s)";

    private DateTimeOffset? _lastUpdated;
    public DateTimeOffset? LastUpdated
    {
        get => _lastUpdated;
        private set
        {
            if(SetProperty(ref _lastUpdated, value))
                OnPropertyChanged(nameof(LastUpdatedText));
        }
    }

    public string LastUpdatedText => LastUpdated.HasValue ? $"Updated: {LastUpdated:yyyy-MM-dd HH:mm}" : string.Empty;

    public ICommand OpenProjectCommand { get; }
    public ICommand OpenItemUrlCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ProjectDetailViewModel(GitHubProjectService _githubService, string _organization, int projectId)
    {
        this.Organization = _organization;
        this.ProjectId = projectId;
        this.githubService = _githubService;

        // subscribe to collection changes to update derived properties
        Items.CollectionChanged += Items_CollectionChanged;

        OpenProjectCommand = new Command(async () => await TryOpenUrl(ProjectUrl));
        OpenItemUrlCommand = new Command<string?>(async (url) => await TryOpenUrl(url));
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ItemCountText));
    }

    // Public loader: call this from the page lifecycle (e.g. OnAppearing)
    public async Task LoadItemsAsync()
    {
        if(githubService is null)
            return;

        var items = await githubService.GetProjectItemsAsync(Organization, ProjectId).ConfigureAwait(false);

        // ensure collection modifications happen on UI thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Items.Clear();
            foreach(var item in items)
                Items.Add(item);

            LastUpdated = DateTimeOffset.Now;
            OnPropertyChanged(nameof(ItemCountText));
            OnPropertyChanged(nameof(HasProjectUrl));
        });
    }

    private async Task TryOpenUrl(string? maybeUrl)
    {
        if(string.IsNullOrWhiteSpace(maybeUrl))
            return;

        try
        {
            await Launcher.Default.OpenAsync(new Uri(maybeUrl));
        }
        catch(Exception)
        {
            // swallow for now - UI should surface errors if needed later
        }
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if(EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value!;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}