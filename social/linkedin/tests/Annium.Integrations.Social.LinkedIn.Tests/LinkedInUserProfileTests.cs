using Annium.Testing;
using Xunit;

namespace Annium.Integrations.Social.LinkedIn.Tests;

public class LinkedInUserProfileTests : TestBase
{
    // private const string ApiBaseUrl = "https://api.linkedin.com";
    // private const string TestAccessToken = "test_access_token";

    public LinkedInUserProfileTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper) { }

    // [Fact]
    // public void GetUserProfile_ValidRequest_HasCorrectEndpointAndHeaders()
    // {
    //     // Arrange
    //     var expectedUrl = $"{ApiBaseUrl}/v2/me";
    //
    //     // Act
    //     var request = CreateAuthorizedRequest(HttpMethod.Get, expectedUrl);
    //
    //     // Assert
    //     request.RequestUri?.ToString().Should().Be(expectedUrl);
    //     request.Headers.Authorization?.Scheme.Should().Be("Bearer");
    //     request.Headers.Authorization?.Parameter.Should().Be(TestAccessToken);
    // }
    //
    // [Fact]
    // public void GetUserProfileWithProjection_ValidRequest_HasCorrectParameters()
    // {
    //     // Arrange
    //     var projection = "(id,firstName,lastName,profilePicture(displayImage~:playableStreams))";
    //     var expectedUrl = $"{ApiBaseUrl}/v2/me?projection={Uri.EscapeDataString(projection)}";
    //
    //     // Act
    //     var request = CreateAuthorizedRequest(HttpMethod.Get, expectedUrl);
    //
    //     // Assert
    //     request.RequestUri?.ToString().Should().Be(expectedUrl);
    //     request.Headers.Authorization?.Scheme.Should().Be("Bearer");
    // }
    //
    // [Fact]
    // public void GetUserEmail_ValidRequest_HasCorrectEndpointAndQuery()
    // {
    //     // Arrange
    //     var expectedUrl = $"{ApiBaseUrl}/v2/emailAddress?q=members&projection=(elements*(handle~))";
    //
    //     // Act
    //     var request = CreateAuthorizedRequest(HttpMethod.Get, expectedUrl);
    //
    //     // Assert
    //     request.RequestUri?.ToString().Should().Be(expectedUrl);
    //     request.Headers.Authorization?.Scheme.Should().Be("Bearer");
    // }
    //
    // [Fact]
    // public void ParseUserProfileResponse_ValidJson_ReturnsCorrectData()
    // {
    //     // Arrange
    //     var jsonResponse = """
    //         {
    //             "id": "test-user-id",
    //             "firstName": {
    //                 "localized": {
    //                     "en_US": "John"
    //                 }
    //             },
    //             "lastName": {
    //                 "localized": {
    //                     "en_US": "Doe"
    //                 }
    //             },
    //             "profilePicture": {
    //                 "displayImage": "urn:li:digitalmediaAsset:test-image-id"
    //             }
    //         }
    //         """;
    //
    //     // Act
    //     var profileData = ParseUserProfile(jsonResponse);
    //
    //     // Assert
    //     profileData.Id.Should().Be("test-user-id");
    //     profileData.FirstName.Should().Be("John");
    //     profileData.LastName.Should().Be("Doe");
    //     profileData.ProfilePictureUrn.Should().Be("urn:li:digitalmediaAsset:test-image-id");
    // }
    //
    // [Fact]
    // public void ParseEmailResponse_ValidJson_ReturnsCorrectEmail()
    // {
    //     // Arrange
    //     var jsonResponse = """
    //         {
    //             "elements": [
    //                 {
    //                     "handle~": {
    //                         "emailAddress": "john.doe@example.com"
    //                     },
    //                     "handle": "urn:li:emailAddress:test-email-id"
    //                 }
    //             ]
    //         }
    //         """;
    //
    //     // Act
    //     var email = ParseUserEmail(jsonResponse);
    //
    //     // Assert
    //     email.Should().Be("john.doe@example.com");
    // }
    //
    // [Theory]
    // [InlineData("")]
    // [InlineData("invalid-json")]
    // [InlineData("{}")]
    // public void ParseUserProfile_InvalidJson_ThrowsException(string invalidJson)
    // {
    //     // Act & Assert
    //     Wrap.It(() => ParseUserProfile(invalidJson)).Throws<JsonException>();
    // }
    //
    // [Theory]
    // [InlineData("")]
    // [InlineData(null)]
    // public void CreateAuthorizedRequest_InvalidToken_ThrowsArgumentException(string? token)
    // {
    //     // Act & Assert
    //     Wrap.It(() => CreateAuthorizedRequest(HttpMethod.Get, $"{ApiBaseUrl}/v2/me", token))
    //         .Throws<ArgumentException>();
    // }
    //
    // private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string? token = null)
    // {
    //     var accessToken = token ?? TestAccessToken;
    //
    //     if (string.IsNullOrEmpty(accessToken))
    //         throw new ArgumentException("Access token cannot be null or empty", nameof(token));
    //
    //     var request = new HttpRequestMessage(method, url);
    //     request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    //
    //     return request;
    // }
    //
    // private static LinkedInUserProfile ParseUserProfile(string jsonResponse)
    // {
    //     if (string.IsNullOrEmpty(jsonResponse))
    //         throw new JsonException("Response cannot be null or empty");
    //
    //     var document = JsonDocument.Parse(jsonResponse);
    //     var root = document.RootElement;
    //
    //     if (!root.TryGetProperty("id", out var idElement))
    //         throw new JsonException("Missing required 'id' property");
    //
    //     var firstName = ExtractLocalizedValue(root, "firstName");
    //     var lastName = ExtractLocalizedValue(root, "lastName");
    //     var profilePictureUrn =
    //         root.TryGetProperty("profilePicture", out var pictureElement)
    //         && pictureElement.TryGetProperty("displayImage", out var imageElement)
    //             ? imageElement.GetString()
    //             : null;
    //
    //     return new LinkedInUserProfile
    //     {
    //         Id = idElement.GetString() ?? throw new JsonException("User ID cannot be null"),
    //         FirstName = firstName,
    //         LastName = lastName,
    //         ProfilePictureUrn = profilePictureUrn,
    //     };
    // }
    //
    // private static string ParseUserEmail(string jsonResponse)
    // {
    //     if (string.IsNullOrEmpty(jsonResponse))
    //         throw new JsonException("Response cannot be null or empty");
    //
    //     var document = JsonDocument.Parse(jsonResponse);
    //     var root = document.RootElement;
    //
    //     if (
    //         !root.TryGetProperty("elements", out var elementsProperty)
    //         || elementsProperty.ValueKind != JsonValueKind.Array
    //         || elementsProperty.GetArrayLength() == 0
    //     )
    //     {
    //         throw new JsonException("Missing or empty 'elements' array");
    //     }
    //
    //     var firstElement = elementsProperty[0];
    //     if (
    //         !firstElement.TryGetProperty("handle~", out var handleProperty)
    //         || !handleProperty.TryGetProperty("emailAddress", out var emailProperty)
    //     )
    //     {
    //         throw new JsonException("Missing email address in response");
    //     }
    //
    //     return emailProperty.GetString() ?? throw new JsonException("Email address cannot be null");
    // }
    //
    // private static string ExtractLocalizedValue(JsonElement element, string propertyName)
    // {
    //     if (
    //         !element.TryGetProperty(propertyName, out var nameProperty)
    //         || !nameProperty.TryGetProperty("localized", out var localizedProperty)
    //     )
    //     {
    //         return string.Empty;
    //     }
    //
    //     var firstLocalization = localizedProperty.EnumerateObject().FirstOrDefault();
    //     return firstLocalization.Value.GetString() ?? string.Empty;
    // }
}

public record LinkedInUserProfile
{
    public required string Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? ProfilePictureUrn { get; init; }
}
