using System.Text;
using System.Text.Json.Nodes;
using Grpc.Core;
using Billing;
using Newtonsoft.Json;

namespace Billing.Services;

public class BillingService : Billing.BillingBase
{
    private readonly ILogger<BillingService> _logger;

    public BillingService(ILogger<BillingService> logger)
    {
        _logger = logger;
        string jsonUserProfiles = File.ReadAllText("userProfiles.json", Encoding.Default);
        _userProfiles = JsonConvert.DeserializeObject<List<UserProfile>>(jsonUserProfiles);
        if (_userProfiles == null)
        {
            _userProfiles = new List<UserProfile>();
        }
    }

    private List<UserProfile> _userProfiles;

    public override async Task ListUsers(None request, IServerStreamWriter<UserProfile> responseStream, ServerCallContext context)
    {
        foreach (var userProfile in _userProfiles)
        {
            await responseStream.WriteAsync(userProfile);
        }
    }

    public override Task<Response> CoinsEmission(EmissionAmount request, ServerCallContext context)
    {
        return base.CoinsEmission(request, context);
    }

    public override Task<Response> MoveCoins(MoveCoinsTransaction request, ServerCallContext context)
    {
        return base.MoveCoins(request, context);
    }

    public override Task<Coin> LongestHistoryCoin(None request, ServerCallContext context)
    {
        return base.LongestHistoryCoin(request, context);
    }
}