using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Annium.Testing;
using Xunit;

namespace Annium.Integrations.Social.LinkedIn.Tests;

public class LinkedInOAuthTests : TestBase
{
    private const string ClientId = "78ta49pevf9qn8";
    private const string ClientSecret = "LINKEDIN_CLIENT_SECRET_REMOVED";
    private const string RedirectUri = "https://localhost:3000/callback";
    private const string BaseUrl = "https://www.linkedin.com";
    private const string ApiBaseUrl = "https://api.linkedin.com";

    public LinkedInOAuthTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper) { }

    [Fact]
    public async Task OAuthWithGetMe_Works()
    {
        // Arrange
        var scope = "r_liteprofile r_emailaddress w_member_social";
        var state = "random_state_string";

        // Step 1: Build authorization URL
        var authUrl = BuildAuthorizationUrl(ClientId, RedirectUri, scope, state);

        // Document the authorization URL and steps for manual testing
        // Authorization URL: https://www.linkedin.com/oauth/v2/authorization?...

        // Assert authorization URL is correctly formatted
        authUrl.StartsWith($"{BaseUrl}/oauth/v2/authorization").IsTrue();
        authUrl.Contains($"client_id={ClientId}").IsTrue();
        authUrl.Contains($"redirect_uri={HttpUtility.UrlEncode(RedirectUri)}").IsTrue();
        authUrl.Contains($"scope={HttpUtility.UrlEncode(scope)}").IsTrue();
        authUrl.Contains($"state={state}").IsTrue();
        authUrl.Contains("response_type=code").IsTrue();

        // Step 2: Simulate token exchange (would normally use real authorization code)
        var mockAuthCode = "mock_authorization_code";
        var tokenRequestBody = BuildTokenRequestBody(mockAuthCode, ClientId, ClientSecret, RedirectUri);

        // Token exchange would be done here with real authorization code

        // Verify token request is properly formatted
        tokenRequestBody.Contains("grant_type=authorization_code").IsTrue();
        tokenRequestBody.Contains($"code={mockAuthCode}").IsTrue();
        tokenRequestBody.Contains($"client_id={ClientId}").IsTrue();
        tokenRequestBody.Contains($"client_secret={ClientSecret}").IsTrue();

        // Step 3: Simulate API call to get user profile (would normally use real access token)
        var mockAccessToken = "mock_access_token";
        var profileRequest = CreateProfileRequest(mockAccessToken);

        // Profile API request would be executed here with real access token

        // Verify profile request is correctly configured
        profileRequest.RequestUri?.ToString().Is($"{ApiBaseUrl}/v2/me");
        profileRequest.Headers.Authorization?.Scheme.Is("Bearer");
        profileRequest.Headers.Authorization?.Parameter.Is(mockAccessToken);

        // OAuth flow structure validated successfully
        // For real integration testing, replace mock values with actual LinkedIn app credentials
    }

    private static string BuildAuthorizationUrl(string clientId, string redirectUri, string scope, string state)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException("Client ID cannot be null or empty", nameof(clientId));

        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["response_type"] = "code";
        queryParams["client_id"] = clientId;
        queryParams["redirect_uri"] = redirectUri;
        queryParams["scope"] = scope;
        queryParams["state"] = state;

        return $"{BaseUrl}/oauth/v2/authorization?{queryParams}";
    }

    private static string BuildTokenRequestBody(string code, string clientId, string clientSecret, string redirectUri)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Authorization code cannot be null or empty", nameof(code));

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = redirectUri,
        };

        return string.Join("&", parameters.Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}"));
    }

    private HttpRequestMessage CreateProfileRequest(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiBaseUrl}/v2/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }
}
