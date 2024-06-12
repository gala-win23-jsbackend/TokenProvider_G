

using Infrastructure.Data.Contexts;
using Infrastructure.Entities;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> GetRefreshTokenAsync(string refreshToken, CancellationToken cts);
    Task<bool> SaveRefreshTokenAsync(string refreshToken, string userId, CancellationToken cts);

}

public class RefreshTokenService(IDbContextFactory<DataContext> dbContextFactory):IRefreshTokenService
{

    private readonly IDbContextFactory<DataContext> _dbContextFactory = dbContextFactory;

    #region GetRefreshTokenAsync


    public async Task<RefreshTokenResult> GetRefreshTokenAsync(string refreshToken, CancellationToken cts)
    {
        RefreshTokenResult refreshTokenResult = null!;
        try
        {
            await using var context = _dbContextFactory.CreateDbContext();



            var refreshTokenEntity = await context.RefreshTokens.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.ExpiryDate > DateTime.Now, cts);
            if (refreshTokenEntity != null)
            {
                refreshTokenResult = new RefreshTokenResult()
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Token = refreshTokenEntity.RefreshToken,
                    ExpiryDate = refreshTokenEntity.ExpiryDate,
                };
            }
            else
            {
                refreshTokenResult = new RefreshTokenResult()
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Error = "Refresh token not found or expired"
                };
            }
            return refreshTokenResult;
        }


        catch (Exception ex)
        {
            return new RefreshTokenResult()
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Error = ex.Message
            };
        }
    }
    #endregion

    #region SaveRefreshTokenAsync
    public async Task<bool> SaveRefreshTokenAsync(string refreshToken, string userId, CancellationToken cts)
    {
        try
        {
            var tokenLifetime = double.TryParse(Environment.GetEnvironmentVariable("Token_RefreshToken_Lifetime"), out double refreshTokenLifeTime) ? refreshTokenLifeTime : 7;

            await using var context = _dbContextFactory.CreateDbContext();
            var refreshTokenEntity = new RefreshTokenEntity()
            {
                RefreshToken = refreshToken,
                UserId = userId,
                ExpiryDate = DateTime.Now.AddDays(tokenLifetime)
            };
            context.RefreshTokens.Add(refreshTokenEntity);
            await context.SaveChangesAsync(cts);
            return true;
        }
        catch
        {
            return false;
        }
    }
    #endregion

}
