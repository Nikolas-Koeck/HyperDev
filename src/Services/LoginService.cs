using HtmlAgilityPack;
using System.Net;

namespace HyperDev.src.Services;

public interface ILoginService {
    Task<LoginResult> LoginAsync();
}

public class LoginService : ILoginService {
    private readonly HttpClient _httpClient;
    private string Username { get; }
    private string Password { get; }
    public LoginService(HttpClient httpClient, string username, string password)
    {
        _httpClient = httpClient;
        Username = username;
        Password = password;
    }

    public async Task<LoginResult> LoginAsync()
    {
        var url = "https://avero.hegele.de/Account/Login.aspx";

        // Step 1: GET the login page (this often sets session cookies)
        var loginPageResponse = await _httpClient.GetAsync(url);
        var loginPageHtml = await loginPageResponse.Content.ReadAsStringAsync();

        // Collect cookies from the GET response (Set-Cookie) and forward them with the POST
        string? cookieHeader = null;
        if(loginPageResponse.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            // Keep only the "name=value" parts and join with "; "
            cookieHeader = string.Join("; ", setCookies.Select(sc => sc.Split(';')[0].Trim()));
        }

        // Step 2: Extract hidden fields
        var hiddenFields = ExtractHiddenFields(loginPageHtml);

        // Step 3: Build form payload
        var formData = new Dictionary<string, string>(hiddenFields)
        {
            { "ctl00$ContentPlaceHolder1$UserName", this.Username },
            { "ctl00$ContentPlaceHolder1$Password", this.Password },
            { "ctl00$ContentPlaceHolder1$LoginButton", "Anmelden" },
            { "ctl00$ContentPlaceHolder1$Employee", "on" }
        };

        var content = new FormUrlEncodedContent(formData);

        // Step 4: POST login request
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        // Headers to better mimic the working browser request
        request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        request.Headers.TryAddWithoutValidation("Accept-Language", "de-DE,de;q=0.9,en-US;q=0.8,en;q=0.7");
        request.Headers.TryAddWithoutValidation("Cache-Control", "max-age=0");
        request.Headers.TryAddWithoutValidation("Origin", "https://avero.hegele.de");
        request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
        request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Chromium\";v=\"148\", \"Google Chrome\";v=\"148\", \"Not/A)Brand\";v=\"99\"");
        request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
        request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
        request.Headers.TryAddWithoutValidation("sec-fetch-dest", "document");
        request.Headers.TryAddWithoutValidation("sec-fetch-mode", "navigate");
        request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
        request.Headers.TryAddWithoutValidation("sec-fetch-user", "?1");

        // Common headers that can be set via typed properties
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36");
        request.Headers.Referrer = new Uri(url);

        // Forward cookies from the initial GET (if any)
        if(!string.IsNullOrEmpty(cookieHeader))
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        }

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        // Note: the server usually responds with a redirect (302 Found). Do NOT call EnsureSuccessStatusCode()
        // because 302 is not a 2xx status and EnsureSuccessStatusCode would throw.
        return new LoginResult
        {
            Success = response.StatusCode == HttpStatusCode.Found,
            HttpResponseMessage = response
        };
    }

    private Dictionary<string, string> ExtractHiddenFields(string html)
    {
        var result = new Dictionary<string, string>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes("//input[@type='hidden']");
        if(nodes == null)
            return result;

        foreach(var node in nodes)
        {
            var name = node.GetAttributeValue("name", null);
            if(string.IsNullOrEmpty(name))
                continue;

            // Value may be absent or empty
            var value = node.GetAttributeValue("value", string.Empty) ?? string.Empty;
            result[name] = WebUtility.HtmlDecode(value);
        }

        return result;
    }
}

public struct LoginResult {
    public bool Success { get; init; }
    public HttpResponseMessage HttpResponseMessage { get; init; }
}