using Infrastructure.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TokenProvider_G.Functions;

public class RefreshTokens(ILogger<RefreshTokens> logger, IRefreshTokenService refreshTokenService, ITokenGenerator tokenGenerator)
{
    private readonly ILogger<RefreshTokens> _logger = logger;
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;

    [Function("RefreshTokens")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "token/refresh")] HttpRequest req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var tokenRequest = JsonConvert.DeserializeObject<TokenRequest>(body);

        if (tokenRequest == null || tokenRequest.UserId == null || tokenRequest.Email == null)
        {
            return new BadRequestObjectResult(new { Error = "Please provide a valid UserId and Email" });
        }

        try
        {
            RefreshTokenResult refreshTokenResult = null!;
            AccessTokenResult accessTokenResult = null!;

            using var ctsTimeOut = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ctsTimeOut.Token, req.HttpContext.RequestAborted);

            req.HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);
            if (string.IsNullOrEmpty(refreshToken))
                return new UnauthorizedObjectResult(new { Error = "No refresh token found" });

            refreshTokenResult = await _refreshTokenService.GetRefreshTokenAsync(refreshToken, cts.Token);
            if (refreshTokenResult.ExpiryDate < DateTime.Now)
                return new UnauthorizedObjectResult(new { Error = "Refresh token expired" });

            if (refreshTokenResult.ExpiryDate < DateTime.Now.AddDays(1))
                refreshTokenResult = await _tokenGenerator.GenerateRefreshTokenAsync(tokenRequest.UserId, cts.Token);

            accessTokenResult = _tokenGenerator.GenerateAccessToken(tokenRequest, refreshTokenResult.Token!);

            if (refreshTokenResult.CookieOptions != null && refreshTokenResult.Token != null)
                req.HttpContext.Response.Cookies.Append("refreshToken", refreshTokenResult.Token, refreshTokenResult.CookieOptions);


            if (accessTokenResult != null && accessTokenResult.Token != null && refreshTokenResult.Token != null)
                return new OkObjectResult(new { AccessToken = accessTokenResult.Token, RefreshToken = refreshTokenResult.Token });


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token");
            return new BadRequestObjectResult(new { Error = "Error generating token" });
        }
        return new UnauthorizedObjectResult(new { Error = "Unexpected error while trying to refresh tokens." });
    }
}
