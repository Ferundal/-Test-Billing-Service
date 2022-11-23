using Grpc.Core;
using Billing;

namespace Billing.Services;

public class GreeterService : Billing.BillingBase
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task ListUsers(None request, IServerStreamWriter<UserProfile> responseStream, ServerCallContext context)
    {
        return base.ListUsers(request, responseStream, context);
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