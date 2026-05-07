using System.Text.Json.Serialization;

namespace HyperDev.src.Models;

public class Feature {
    public string Name { get; set; }
}

public class Slot {
    public string Title { get; set; }
    public Feature AssignedFeature { get; set; }
    public bool IsLocked { get; set; }
}

public class GitHubIssue {
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("user")]
    public GitHubUser? User { get; set; }

    [JsonPropertyName("html_url")]
    public string Url { get; set; } = string.Empty;
}

public class GitHubUser {
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string ProfileUrl { get; set; } = string.Empty;
}

public class ProjectItem {
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? ContentType { get; set; }
}

public class GraphQLResponse {
    public GraphQLData? Data { get; set; }
}

public class GraphQLData {
    public Organization? Organization { get; set; }
}

public class Organization {
    public ProjectV2? ProjectV2 { get; set; }
}

public class ProjectV2 {
    public ProjectItemConnection? Items { get; set; }
}

public class ProjectItemConnection {
    public List<ProjectItemNode> Nodes { get; set; } = new();
}

public class ProjectItemNode {
    public string Id { get; set; } = string.Empty;
    public ProjectContent? Content { get; set; }
}

public class ProjectContent {
    public string? Title { get; set; }
    public string? Url { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("__typename")]
    public string? Typename { get; set; }
}
