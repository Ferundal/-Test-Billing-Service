namespace Billing.Services;

public class CoinModel
{
    private static long _maxId = 0;
    private long _id;
    private List<UserProfileModel> _owners;
    
    public UserProfileModel Owner
    {
        get => _owners.Last();
        set => _owners.Add(value);
    }

    public long HistoryLength
    {
        get => _owners.Count;
    }
    
    public CoinModel(UserProfileModel emitter)
    {
        _id = _maxId++;
        _owners = new List<UserProfileModel> { emitter };
    }

    public string[] History()
    {
        string[] ownerNames = new string[_owners.Count];
        for (int index = 0; index < _owners.Count; ++index)
        {
            ownerNames[index] = _owners.ElementAt(index).Name;
        }
        return ownerNames;
    }

    public Coin ToCoin()
    {
        var coin = new Coin();
        coin.Id = this._id;
        coin.History = String.Join(" ", this.History());
        return coin;
    }
}