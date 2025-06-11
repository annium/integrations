using Microsoft.AspNetCore.Mvc;

namespace Annium.Integrations.Social.LinkedIn.Demo.Controllers;

[Route("/")]
public class IndexController : ControllerBase
{
    public IndexController() { }

    [HttpGet]
    public IActionResult Index()
    {
        var html = """
            <!DOCTYPE html>
            <html>
            <head>
                <title>LinkedIn OAuth Demo</title>
                <style>
                    body { font-family: Arial, sans-serif; max-width: 800px; margin: 50px auto; padding: 20px; }
                    .button { display: inline-block; padding: 12px 24px; background: #0077b5; color: white;
                             text-decoration: none; border-radius: 4px; font-weight: bold; }
                    .button:hover { background: #005885; }
                    pre { background: #f5f5f5; padding: 15px; border-radius: 4px; overflow-x: auto; }
                </style>
            </head>
            <body>
                <h1>LinkedIn OAuth Demo</h1>
                <p>This demo shows how to authenticate with LinkedIn using OAuth 2.0 and retrieve user profile information.</p>

                <h2>Steps:</h2>
                <ol>
                    <li>Click the "Login with LinkedIn" button below</li>
                    <li>You'll be redirected to LinkedIn for authentication</li>
                    <li>After successful login, you'll be redirected back with your profile data</li>
                </ol>

                <a href="/linkedin/auth" class="button">Login with LinkedIn</a>

                <h2>What this demo does:</h2>
                <ul>
                    <li>Generates OAuth authorization URL with proper parameters</li>
                    <li>Handles the callback from LinkedIn</li>
                    <li>Exchanges authorization code for access token</li>
                    <li>Fetches user profile and email information</li>
                    <li>Displays the retrieved data as JSON</li>
                </ul>

                <h2>Endpoint Information:</h2>
                <pre>
                    GET /linkedin/auth      - Start OAuth flow
                    GET /linkedin/callback  - Handle OAuth callback
                </pre>
            </body>
            </html>
            """;

        return Content(html, "text/html");
    }
}
