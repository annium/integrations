using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Annium.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Annium.Integrations.Social.LinkedIn.Demo.Controllers;

[Route("linkedin")]
public class LinkedInController : ControllerBase, ILogSubject
{
    public ILogger Logger { get; }
    private readonly HttpClient _httpClient;

    private const string ClientId = "86xwy84pgmznfb";
    private const string ClientSecret = "LINKEDIN_CLIENT_SECRET_REMOVED";
    private const string BaseUrl = "https://www.linkedin.com";
    private const string ApiBaseUrl = "https://api.linkedin.com";

    public LinkedInController(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        Logger = logger;
    }

    [HttpGet("auth")]
    public IActionResult StartAuth()
    {
        var redirectUri = $"{Request.Scheme}://{Request.Host}/linkedin/callback";
        var scope = "openid profile email w_member_social";
        var state = Guid.NewGuid().ToString("N");

        // Store state in session for validation
        HttpContext.Session.SetString("oauth_state", state);

        var authUrl = BuildAuthorizationUrl(ClientId, redirectUri, scope, state);

        this.Info<string>("Redirecting to LinkedIn authorization: {AuthUrl}", authUrl);

        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> HandleCallbackAsync(string? code, string? state, string? error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            this.Error<string>("LinkedIn OAuth error: {Error}", error);
            return BadRequest($"OAuth error: {error}");
        }

        if (string.IsNullOrEmpty(code))
        {
            return BadRequest("Authorization code is missing");
        }

        if (string.IsNullOrEmpty(state))
        {
            return BadRequest("State parameter is missing");
        }

        // Validate state
        var storedState = HttpContext.Session.GetString("oauth_state");
        if (state != storedState)
        {
            this.Error<string?, string?>(
                "State mismatch. Expected: {Expected}, Received: {Received}",
                storedState,
                state
            );
            return BadRequest("Invalid state parameter");
        }

        try
        {
            var redirectUri = $"{Request.Scheme}://{Request.Host}/linkedin/callback";

            // Exchange code for access token
            var tokenResponse = await ExchangeCodeForTokenAsync(code, redirectUri);

            if (tokenResponse?.AccessToken == null)
            {
                return BadRequest("Failed to obtain access token");
            }

            this.Info("Successfully obtained access token");

            // Get user profile
            var userProfile = await GetUserProfileAsync(tokenResponse.AccessToken);

            return Ok(
                new
                {
                    message = "LinkedIn OAuth successful!",
                    token = new
                    {
                        access_token = tokenResponse.AccessToken,
                        expires_in = tokenResponse.ExpiresIn,
                        scope = tokenResponse.Scope,
                    },
                    profile = userProfile,
                }
            );
        }
        catch (Exception ex)
        {
            this.Error(ex);
            return StatusCode(500, "Internal server error during OAuth processing");
        }
    }

    private static string BuildAuthorizationUrl(string clientId, string redirectUri, string scope, string state)
    {
        var parameters = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri,
            ["scope"] = scope,
            ["state"] = state,
        };

        var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));
        return $"{BaseUrl}/oauth/v2/authorization?{queryString}";
    }

    private async Task<TokenResponse?> ExchangeCodeForTokenAsync(string code, string redirectUri)
    {
        var tokenUrl = $"{BaseUrl}/oauth/v2/accessToken";

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["redirect_uri"] = redirectUri,
        };

        var content = new FormUrlEncodedContent(parameters);

        this.Info("Exchanging authorization code for access token");

        var response = await _httpClient.PostAsync(tokenUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            this.Error<HttpStatusCode, string>(
                "Token exchange failed. Status: {StatusCode}, Response: {Response}",
                response.StatusCode,
                responseContent
            );
            return null;
        }

        this.Info("Token exchange successful");

        return JsonSerializer.Deserialize<TokenResponse>(
            responseContent,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }
        );
    }

    private async Task<UserProfile?> GetUserProfileAsync(string accessToken)
    {
        var profileUrl = $"{ApiBaseUrl}/v2/userinfo";

        var request = new HttpRequestMessage(HttpMethod.Get, profileUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        this.Info("Fetching user profile from LinkedIn API");

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            this.Error<HttpStatusCode, string>(
                "Profile API call failed. Status: {StatusCode}, Response: {Response}",
                response.StatusCode,
                responseContent
            );
            return null;
        }

        this.Info("Successfully retrieved user profile");

        var profile = JsonSerializer.Deserialize<UserProfile>(
            responseContent,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }
        );

        return profile;
    }

    private class TokenResponse
    {
        public string AccessToken { get; init; } = string.Empty;
        public int ExpiresIn { get; init; }
        public string Scope { get; init; } = string.Empty;
    }

    private record UserProfile
    {
        public string Sub { get; init; } = string.Empty;
        public bool EmailVerified { get; init; }
        public string Name { get; init; } = string.Empty;
        public UserProfileLocale Locale { get; init; } = new();
        public string GivenName { get; init; } = string.Empty;
        public string FamilyName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Picture { get; init; } = string.Empty;
    }

    private record UserProfileLocale
    {
        public string Country { get; init; } = string.Empty;
        public string Language { get; init; } = string.Empty;
    }
}
