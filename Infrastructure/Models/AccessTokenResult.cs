
namespace Infrastructure.Models;

public class AccessTokenResult
{
    public int? StatusCode { get; set; }
    public string? Token { get; set; } = null!;
    public string? Error { get; set; } = null!;
}
