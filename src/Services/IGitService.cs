src\Services\IGitService.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HyperDev.src.Services;

public class GitResult
{
    public int ExitCode { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public bool Success => ExitCode == 0;
}   

public interface IGitService
{
    /// <summary>
    /// Runs a raw git command (e.g. "status", "log --oneline") and returns outputs.
    /// </summary>
    Task<GitResult> RunGitCommandAsync(string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default);

    Task<string> GetStatusAsync(string repositoryPath, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> ListBranchesAsync(string repositoryPath, CancellationToken cancellationToken = default);

    Task<string?> GetCurrentBranchAsync(string repositoryPath, CancellationToken cancellationToken = default);

    Task<bool> CloneAsync(string repositoryUrl, string destinationPath, CancellationToken cancellationToken = default);

    Task<bool> CreateBranchAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default);

    Task<bool> CheckoutAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default);

    Task<bool> CommitAsync(string repositoryPath, string message, CancellationToken cancellationToken = default);

    Task<bool> PushAsync(string repositoryPath, string? remote = "origin", string? branch = null, CancellationToken cancellationToken = default);

    Task<GitResult> PullAsync(string repositoryPath, string? remote = "origin", string? branch = null, CancellationToken cancellationToken = default);
}