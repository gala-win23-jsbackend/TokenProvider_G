

using Infrastructure.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services;

public class TokenGenerator(IRefreshTokenService refreshTokenService)
{
    private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;

    #region GenerateRefreshTokenAsync
    public async Task<RefreshTokenResult> GenerateRefreshTokenAsync(string userId, CancellationToken cts)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new RefreshTokenResult
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Error = "Please provide a valid UserId"
                };
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
            };

            var token = GenerateJwtToken(new ClaimsIdentity(claims), DateTime.Now.AddMinutes(5));
            if (token == null)
            {
                return new RefreshTokenResult
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Error = "Error generating token"
                };
            }
            else
            {
                var cookieOptions = CookieGenerator.GenerateCookie(DateTimeOffset.Now.AddDays(7));
                if (cookieOptions == null)
                {
                    return new RefreshTokenResult
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        Error = "Error when cookie was generated"
                    };
                }

                var result = await _refreshTokenService.SaveRefreshTokenAsync(token, userId, cts);
                if (!result)
                {
                    return new RefreshTokenResult
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        Error = "Error saving token"
                    };
                }
                else
                {
                    return new RefreshTokenResult
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Token = token,
                        CookieOptions = cookieOptions
                    };
                }

            }

        }
        catch (Exception ex)
        {
            return new RefreshTokenResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Error = ex.Message,
            };
        }
    }

    #endregion

    #region GenerateJwtToken
    public static string GenerateJwtToken(ClaimsIdentity claims, DateTime expires)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("Token_Secret")!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = expires,
            Issuer = Environment.GetEnvironmentVariable("Token_Issuer"),
            Audience = Environment.GetEnvironmentVariable("Token_Audience"),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    #endregion

    #region GenerateAccessToken
    public AccessTokenResult GenerateAccessToken(TokenRequest tokenRequest, string refreshToken)
    {
        try
        {
            if (string.IsNullOrEmpty(tokenRequest.UserId) || string.IsNullOrEmpty(tokenRequest.Email))
            {
                return new AccessTokenResult
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Error = "Invalid request body. Parameters userId and Email must be provided"
                };
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, tokenRequest.UserId),
                new Claim(ClaimTypes.Name, tokenRequest.Email),
                new Claim(ClaimTypes.Email, tokenRequest.Email),
            };
            if (!string.IsNullOrEmpty(refreshToken))
            {
                claims = [.. claims, new Claim("refreshToken", refreshToken)];
            }

            var token = GenerateJwtToken(new ClaimsIdentity(claims), DateTime.Now.AddMinutes(5));
            if (token == null)
            {
                return new AccessTokenResult
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Error = "Error generating token"
                };
            }
            else
            {
                return new AccessTokenResult
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Token = token
                };
            }
        }
        catch (Exception ex)
        {
            return new AccessTokenResult
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Error = ex.Message
            };
        }
    }
    #endregion



}
