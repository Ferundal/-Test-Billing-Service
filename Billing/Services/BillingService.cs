using System.Text;
using System.Text.Json.Nodes;
using Grpc.Core;
using Billing;
using Newtonsoft.Json;

namespace Billing.Services;

public class BillingService : Billing.BillingBase
{
    private readonly ILogger<BillingService> _logger;

    private List<UserProfileModel> _users;
    private long _raitingSum;

    public BillingService(ILogger<BillingService> logger)
    {
        _logger = logger;
        _users = UserProfileModel.LoadFromJson("userProfiles.json");

        foreach (var user in _users)
        {
            _raitingSum += user.Rating;
        }
    }
    
    public override async Task ListUsers(None request, IServerStreamWriter<UserProfile> responseStream, ServerCallContext context)
    {
        foreach (var user in _users)
        {
            await responseStream.WriteAsync(user.ToUserProfile());
        }
    }

    public override Task<Response> CoinsEmission(EmissionAmount request, ServerCallContext context)
    {
        Response response = new Response();
        if (request.Amount < _users.Count)
        {
            response.Status = Response.Types.Status.Failed;
            response.Comment = $"Emission Amount less then Users Amount ({_users.Count})";
            return Task.FromResult(response);
        }

        double coinWeight = 1 / (double) request.Amount;
        var usersToSpreadCoins = new List<UserProfileModel>(_users);
        foreach (var user in usersToSpreadCoins)
        {
            user.EmitCoins(1);
            if (user.Proportion < coinWeight)
            {
                usersToSpreadCoins.Remove(user);
            }
        }

        var spreadedCoinsAmounts = new List<int>(usersToSpreadCoins.Count);
        for(int index = 0; index < usersToSpreadCoins.Count; ++index)
        {
            spreadedCoinsAmounts[index] = 1;
        }
        for (var coinsToSpread = request.Amount - _users.Count; coinsToSpread > 0; --coinsToSpread)
        {
            if (usersToSpreadCoins.Count < 2)
            {
                usersToSpreadCoins.First().EmitCoins(coinsToSpread);
                break;
            }

            int coinReciverIndex = 0;
            for (int index = 1; index < usersToSpreadCoins.Count - 1; ++index)
            {
                if (spreadedCoinsAmounts[index] * coinWeight / usersToSpreadCoins.ElementAt(index).Proportion <
                    spreadedCoinsAmounts[coinReciverIndex] * coinWeight / usersToSpreadCoins.ElementAt(coinReciverIndex).Proportion)
                {
                    coinReciverIndex = index;
                }
            }
            usersToSpreadCoins.ElementAt(coinReciverIndex).EmitCoins(1);
            ++spreadedCoinsAmounts[coinReciverIndex];
            if (usersToSpreadCoins.ElementAt(coinReciverIndex).Proportion >
                spreadedCoinsAmounts[coinReciverIndex] * coinWeight)
            {
                usersToSpreadCoins.RemoveAt(coinReciverIndex);
                spreadedCoinsAmounts.RemoveAt(coinReciverIndex);
            }
        }
        response.Status = Response.Types.Status.Ok;
        return Task.FromResult(response);
    }

    
    
    public override Task<Response> MoveCoins(MoveCoinsTransaction request, ServerCallContext context)
    {
        Response response = new Response();
        UserProfileModel srcUser = FindUser(request.SrcUser);
        if (srcUser == null)
        {
            response.Status = Response.Types.Status.Failed;
            response.Comment = $"Source user \"{_users.Count}\" not found";
            return Task.FromResult(response);
        }

        if (srcUser.Coins < request.Amount)
        {
            response.Status = Response.Types.Status.Failed;
            response.Comment = $"Not enough coins at source user. Needed: {request.Amount}, Available: {srcUser.Coins}";
            return Task.FromResult(response);
        }
        UserProfileModel dstUser = FindUser(request.DstUser);
        if (dstUser == null)
        {
            response.Status = Response.Types.Status.Failed;
            response.Comment = $"Destination user \"{_users.Count}\" not found";
            return Task.FromResult(response);
        }
        srcUser.SendCoins(dstUser, request.Amount);
        response.Status = Response.Types.Status.Ok;
        return Task.FromResult(response);
    }

    public override Task<Coin> LongestHistoryCoin(None request, ServerCallContext context)
    {
        CoinModel longestCoin = null;
        foreach (var user in _users)
        {
            var userLongestCoin = user.LongestCoin();
            if (userLongestCoin != null)
            {
                if (longestCoin == null)
                {
                    longestCoin = userLongestCoin;
                }
                else
                {
                    if (userLongestCoin.HistoryLength > longestCoin.HistoryLength)
                    {
                        longestCoin = userLongestCoin;
                    }
                }
            }
        }
        if (longestCoin == null)
        {
            return Task.FromResult<Coin>(new Coin());
        }
        return Task.FromResult<Coin>(longestCoin.ToCoin());
    }

    private UserProfileModel FindUser(string userName)
    {
        foreach (var user in _users)
        {
            if (user.Name.Equals(userName))
            {
                return user;
            }
        }

        return null;
    }
    
}