using HyperDev.src.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HyperDev.src.Services;

public class GitHubProjectService {
    private readonly HttpClient _httpClient;

    public GitHubProjectService(HttpClient httpClient, string personalAccessToken)
    {
        _httpClient = httpClient;

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", personalAccessToken);

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp");
    }

    public async Task<List<ProjectItem>> GetProjectItemsAsync(
        string organization,
        int projectNumber)
    {
        var url = $"https://api.github.com/orgs/{organization}/projectsV2/{projectNumber}/items";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var restItems = JsonSerializer.Deserialize<List<ProjectsV2ItemRest>>(responseString, options);
        if(restItems == null)
            throw new InvalidOperationException($"Failed to parse REST project items. Raw response: {responseString}");

        var list = restItems.Select(item =>
        {
            string title = "(No title)";
            string urlValue = null;
            string? creatorName = null;
            string? itemLink = null;

            if(item.Content.HasValue && item.Content.Value.ValueKind == JsonValueKind.Object)
            {
                var obj = item.Content.Value;

                if(obj.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                    title = t.GetString() ?? title;

                // Common URL properties returned by content objects
                if(obj.TryGetProperty("html_url", out var hu) && hu.ValueKind == JsonValueKind.String)
                    urlValue = hu.GetString();
                else if(obj.TryGetProperty("url", out var u) && u.ValueKind == JsonValueKind.String)
                    urlValue = u.GetString();
                else if(obj.TryGetProperty("node_id", out var nid) && nid.ValueKind == JsonValueKind.String)
                    urlValue = nid.GetString();
            }

            // Extract creator name (prefer "name", fallback to "login")
            if(item.Creator.HasValue && item.Creator.Value.ValueKind == JsonValueKind.Object)
            {
                var c = item.Creator.Value;
                if(c.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                    creatorName = nameProp.GetString();
                else if(c.TryGetProperty("login", out var loginProp) && loginProp.ValueKind == JsonValueKind.String)
                    creatorName = loginProp.GetString();
            }

            // Prefer API-provided item_url if present, otherwise fallback to the content url we discovered
            itemLink = string.IsNullOrEmpty(item.ItemUrl) ? urlValue : item.ItemUrl;

            return new ProjectItem
            {
                Id = item.Id.ToString(),
                Title = title,
                Url = urlValue,
                ContentType = item.ContentType,
                CreatorName = creatorName,
                ItemUrl = itemLink
            };
        }).ToList();

        return list;
    }
}

// REST model for Projects v2 items (GitHub REST API v3)
public class ProjectsV2ItemRest {
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("node_id")]
    public string? NodeId { get; set; }

    [JsonPropertyName("project_url")]
    public string? ProjectUrl { get; set; }

    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    // content can be an object with different shapes or null; use JsonElement to handle polymorphism
    [JsonPropertyName("content")]
    public JsonElement? Content { get; set; }

    [JsonPropertyName("creator")]
    public JsonElement? Creator { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("archived_at")]
    public DateTime? ArchivedAt { get; set; }

    [JsonPropertyName("item_url")]
    public string? ItemUrl { get; set; }

    [JsonPropertyName("fields")]
    public JsonElement? Fields { get; set; }
}