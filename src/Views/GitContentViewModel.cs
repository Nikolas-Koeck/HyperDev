using HyperDev.src.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HyperDev.src.Viewmodels;

public sealed class GitContentViewModel : INotifyPropertyChanged {
    private readonly IGitService _gitService;
    private CancellationTokenSource? _cts;
    private bool _isBusy;
    private string _repositoryPath = string.Empty;
    private string _status = string.Empty;
    private string _currentBranch = string.Empty;
    private string _newBranchName = string.Empty;
    private string _commitMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> Branches { get; } = new();

    public ICommand RefreshCommand { get; }
    public ICommand CheckoutCommand { get; }
    public ICommand CreateBranchCommand { get; }
    public ICommand CommitCommand { get; }
    public ICommand PushCommand { get; }
    public ICommand PullCommand { get; }

    public GitContentViewModel(IGitService gitService)
    {
        _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
        /*
        RefreshCommand = new Command(async () => await ExecuteSafeAsync(RefreshAsync));
        */
        CheckoutCommand = new Command<string>(async branch => await ExecuteSafeAsync(() => CheckoutAsync(branch)));
        CreateBranchCommand = new Command(async () => await ExecuteSafeAsync(CreateBranchAsync));
        CommitCommand = new Command(async () => await ExecuteSafeAsync(CommitAsync));
        PushCommand = new Command(async () => await ExecuteSafeAsync(PushAsync));
        PullCommand = new Command(async () => await ExecuteSafeAsync(PullAsync));
    }

    public string RepositoryPath
    {
        get => _repositoryPath;
        set => SetProperty(ref _repositoryPath, value);
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public string CurrentBranch
    {
        get => _currentBranch;
        private set => SetProperty(ref _currentBranch, value);
    }

    public string NewBranchName
    {
        get => _newBranchName;
        set => SetProperty(ref _newBranchName, value);
    }

    public string CommitMessage
    {
        get => _commitMessage;
        set => SetProperty(ref _commitMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if(SetProperty(ref _isBusy, value))
            {
                ((Command)RefreshCommand).ChangeCanExecute();
                ((Command)CreateBranchCommand).ChangeCanExecute();
                ((Command)CommitCommand).ChangeCanExecute();
                ((Command)PushCommand).ChangeCanExecute();
                ((Command)PullCommand).ChangeCanExecute();
            }
        }
    }

    // Public: load repository info (status, branches, current branch)
    public async Task LoadAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        RepositoryPath = repositoryPath ?? string.Empty;
        await ExecuteSafeAsync(() => RefreshAsync(cancellationToken));
    }

    // Refreshes status, branches and current branch
    private async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(RepositoryPath))
        {
            Status = "No repository selected.";
            Branches.Clear();
            CurrentBranch = string.Empty;
            return;
        }

        _cts?.Cancel();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var ct = _cts.Token;

        IsBusy = true;
        try
        {
            var statusTask = _gitService.GetStatusAsync(RepositoryPath, ct);
            var branchesTask = _gitService.ListBranchesAsync(RepositoryPath, ct);
            var currentBranchTask = _gitService.GetCurrentBranchAsync(RepositoryPath, ct);

            await Task.WhenAll(statusTask, branchesTask, currentBranchTask);

            Status = await statusTask;
            Branches.Clear();
            foreach(var b in await branchesTask)
            {
                Branches.Add(b);
            }

            CurrentBranch = await currentBranchTask ?? string.Empty;
        }
        catch(OperationCanceledException)
        {
            Status = "Refresh cancelled.";
        }
        catch(Exception ex)
        {
            Status = $"Error refreshing repository: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CheckoutAsync(string branch)
    {
        if(string.IsNullOrWhiteSpace(RepositoryPath) || string.IsNullOrWhiteSpace(branch))
        {
            Status = "Repository path or branch is empty.";
            return;
        }

        IsBusy = true;
        try
        {
            var ok = await _gitService.CheckoutAsync(RepositoryPath, branch);
            Status = ok ? $"Checked out {branch}." : $"Failed to checkout {branch}.";
            if(ok) await RefreshAsync();
        }
        catch(Exception ex)
        {
            Status = $"Checkout error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateBranchAsync()
    {
        if(string.IsNullOrWhiteSpace(RepositoryPath) || string.IsNullOrWhiteSpace(NewBranchName))
        {
            Status = "Repository path or branch name is empty.";
            return;
        }

        IsBusy = true;
        try
        {
            var ok = await _gitService.CreateBranchAsync(RepositoryPath, NewBranchName);
            Status = ok ? $"Created branch {NewBranchName}." : $"Failed to create branch {NewBranchName}.";
            if(ok)
            {
                NewBranchName = string.Empty;
                await RefreshAsync();
            }
        }
        catch(Exception ex)
        {
            Status = $"Create branch error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CommitAsync()
    {
        if(string.IsNullOrWhiteSpace(RepositoryPath) || string.IsNullOrWhiteSpace(CommitMessage))
        {
            Status = "Repository path or commit message is empty.";
            return;
        }

        IsBusy = true;
        try
        {
            var ok = await _gitService.CommitAsync(RepositoryPath, CommitMessage);
            Status = ok ? "Commit successful." : "Commit failed.";
            if(ok)
            {
                CommitMessage = string.Empty;
                await RefreshAsync();
            }
        }
        catch(Exception ex)
        {
            Status = $"Commit error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PushAsync()
    {
        if(string.IsNullOrWhiteSpace(RepositoryPath))
        {
            Status = "Repository path is empty.";
            return;
        }

        IsBusy = true;
        try
        {
            var ok = await _gitService.PushAsync(RepositoryPath, "origin", CurrentBranch);
            Status = ok ? "Push successful." : "Push failed.";
        }
        catch(Exception ex)
        {
            Status = $"Push error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PullAsync()
    {
        if(string.IsNullOrWhiteSpace(RepositoryPath))
        {
            Status = "Repository path is empty.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _gitService.PullAsync(RepositoryPath, "origin", CurrentBranch);
            Status = result.Success ? "Pull successful." : $"Pull failed: {result.Error}";
            if(result.Success) await RefreshAsync();
        }
        catch(Exception ex)
        {
            Status = $"Pull error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Helper: wrap calls to set IsBusy and catch exceptions
    private async Task ExecuteSafeAsync(Func<Task> operation)
    {
        if(IsBusy) return;
        IsBusy = true;
        try
        {
            await operation();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // INotifyPropertyChanged helpers
    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if(Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}