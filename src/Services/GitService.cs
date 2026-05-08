src\Services\GitService.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HyperDev.src.Services;

public class GitService : IGitService
{
    private readonly ILogger<GitService> _logger;
    private readonly string _gitExecutable;

    public GitService(ILogger<GitService> logger, string gitExecutable = "git")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gitExecutable = gitExecutable;
    }

    public Task<GitResult> RunGitCommandAsync(string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default)
    {
        if (arguments == null) throw new ArgumentNullException(nameof(arguments));
        return Task.Run(() =>
        {
            var psi = new ProcessStartInfo
            {
                FileName = _gitExecutable,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (!string.IsNullOrWhiteSpace(workingDirectory))
                psi.WorkingDirectory = workingDirectory;

            using var process = new Process { StartInfo = psi };

            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    output.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    error.AppendLine(e.Data);
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.WaitForExit(200))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        try { process.Kill(true); } catch { }
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                return new GitResult
                {
                    ExitCode = process.ExitCode,
                    Output = output.ToString().TrimEnd(),
                    Error = error.ToString().TrimEnd()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running git {Args} in {Wd}", arguments, workingDirectory ?? Environment.CurrentDirectory);
                return new GitResult { ExitCode = -1, Output = string.Empty, Error = ex.Message };
            }
        }, cancellationToken);
    }

    public async Task<string> GetStatusAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        var res = await RunGitCommandAsync("status --porcelain=1 --branch", repositoryPath, cancellationToken);
        if (!res.Success)
            _logger.LogWarning("git status returned non-zero. Error: {Error}", res.Error);
        return res.Output;
    }

    public async Task<IEnumerable<string>> ListBranchesAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        var res = await RunGitCommandAsync("branch --all --no-color", repositoryPath, cancellationToken);
        if (!res.Success)
        {
            _logger.LogWarning("git branch returned non-zero. Error: {Error}", res.Error);
            return Array.Empty<string>();
        }

        var lines = res.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var list = new List<string>(lines.Length);
        foreach (var l in lines)
        {
            // remove leading "* " or "  " and remotes/ prefix if desired
            var trimmed = l.TrimStart('*', ' ').Trim();
            list.Add(trimmed);
        }

        return list;
    }

    public async Task<string?> GetCurrentBranchAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        var res = await RunGitCommandAsync("rev-parse --abbrev-ref HEAD", repositoryPath, cancellationToken);
        if (!res.Success)
        {
            _logger.LogWarning("git rev-parse returned non-zero. Error: {Error}", res.Error);
            return null;
        }

        return string.IsNullOrWhiteSpace(res.Output) ? null : res.Output.Trim();
    }

    public async Task<bool> CloneAsync(string repositoryUrl, string destinationPath, CancellationToken cancellationToken = default)
    {
        var res = await RunGitCommandAsync($"clone \"{repositoryUrl}\" \"{destinationPath}\"", null, cancellationToken);
        if (!res.Success)
            _logger.LogWarning("git clone failed. Error: {Error}", res.Error);
        return res.Success;
    }

    public async Task<bool> CreateBranchAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(branchName)) throw new ArgumentNullException(nameof(branchName));
        var res = await RunGitCommandAsync($"branch \"{branchName}\"", repositoryPath, cancellationToken);
        if (!res.Success)
            _logger.LogWarning("git branch {Branch} failed. Error: {Error}", branchName, res.Error);
        return res.Success;
    }

    public async Task<bool> CheckoutAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default)
    {
        var res = await RunGitCommandAsync($"checkout \"{branchName}\"", repositoryPath, cancellationToken);
        if (!res.Success)
            _logger.LogWarning("git checkout {Branch} failed. Error: {Error}", branchName, res.Error);
        return res.Success;
    }

    public async Task<bool> CommitAsync(string repositoryPath, string message, CancellationToken cancellationToken = default)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        // Stage all changes, then commit
        var add = await RunGitCommandAsync("add -A", repositoryPath, cancellationToken);
        if (!add.Success)
        {
            _logger.LogWarning("git add failed. Error: {Error}", add.Error);
            return false;
        }

        var commit = await RunGitCommandAsync($"commit -m \"{message.Replace("\"", "\\\"")}\"", repositoryPath, cancellationToken);
        if (!commit.Success)
        {
            // if there's nothing to commit, git returns non-zero; that should not be treated as an exception necessarily
            _logger.LogInformation("git commit returned: {Output}. Error: {Error}", commit.Output, commit.Error);
            return false;
        }

        return true;
    }

    public async Task<bool> PushAsync(string repositoryPath, string? remote = "origin", string? branch = null, CancellationToken cancellationToken = default)
    {
        var branchPart = string.IsNullOrWhiteSpace(branch) ? string.Empty : $" {branch}";
        var res = await RunGitCommandAsync($"push {remote}{branchPart}", repositoryPath, cancellationToken);
        if (!res.Success)
            _logger.LogWarning("git push failed. Error: {Error}", res.Error);
        return res.Success;
    }

    public async Task<GitResult> PullAsync(string repositoryPath, string? remote = "origin", string? branch = null, CancellationToken cancellationToken = default)
    {
        var branchPart = string.IsNullOrWhiteSpace(branch) ? string.Empty : $" {branch}";
        var res = await RunGitCommandAsync($"pull {remote}{branchPart}", repositoryPath, cancellationToken);
        if (!res.Success)
            _logger.LogWarning("git pull failed. Error: {Error}", res.Error);
        return res;
    }
}