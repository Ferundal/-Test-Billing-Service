﻿using System.Text;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;

namespace Billing.Services;



public class UserProfileModel
{
    public string Name;
    public long Rating;
    public double Proportion;
    private LinkedList<CoinModel> _coins;

    public long Coins => _coins.Count;

    public UserProfileModel()
    {
        Name = "";
        _coins = new LinkedList<CoinModel>();
    }

    public static List<UserProfileModel> LoadFromJson(string jsonPath)
    {
        string jsonUserProfiles = File.ReadAllText(jsonPath, Encoding.Default);
        var users = JsonConvert.DeserializeObject<List<UserProfileModel>>(jsonUserProfiles);
        if (users == null)
        {
            users = new List<UserProfileModel>();
        }

        long raitingSum = 0;
        foreach (var user in users)
        {
            raitingSum += user.Rating;
        }
        foreach (var user in users)
        {
            user.Proportion = (double) user.Rating / (double) raitingSum;
        }
        return users;
    }
    public UserProfile ToUserProfile()
    {
        var userProfile = new UserProfile();
        userProfile.Amount = Coins;
        userProfile.Name = Name;
        return userProfile;
    }

    public void EmitCoins(long coinsAmount)
    {
        while (coinsAmount > 0)
        {
            var coin = new CoinModel(this);
            _coins.AddFirst(coin);
            --coinsAmount;
        }
        if (_coins.Count == 0)
        {
            throw new RuntimeBinderException(_coins.Count.ToString() + "ERROR");
        }
    }

    public bool SendCoins(UserProfileModel receiver, long coinsAmount)
    {
        if (_coins.Count < coinsAmount)
        {
            return false;
        }

        while (coinsAmount-- > 0)
        {
            receiver._coins.AddLast(_coins.First());
            _coins.First().Owner = receiver;
            _coins.RemoveFirst();
        }
        return true;
    }

    public CoinModel LongestCoin()
    {
        if (_coins.Count < 1)
        {
            return null;
        }

        if (_coins.Count == 1)
        {
            return _coins.First.Value;
        }

        CoinModel longestCoin = _coins.First.Value;
        LinkedListNode<CoinModel> linkedListNode = _coins.First;
        do
        {
            linkedListNode = linkedListNode.Next;
            if (linkedListNode.Value.HistoryLength > longestCoin.HistoryLength)
            {
                longestCoin = linkedListNode.Value;
            }
        } while (linkedListNode != _coins.Last);

        return longestCoin;
    }
}