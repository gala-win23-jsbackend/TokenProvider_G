using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TokenProvider_G
{
    public class GenerateToken
    {
        private readonly ILogger<GenerateToken> _logger;

        public GenerateToken(ILogger<GenerateToken> logger)
        {
            _logger = logger;
        }

        [Function("GenerateToken")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
