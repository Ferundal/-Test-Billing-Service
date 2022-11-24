namespace Billing.Services;

public class User
{
    public long Amount;
    public string Name;
    public long Rating;
    

    public UserProfile ToUserProfile()
    {
        var userProfile = new UserProfile();
        userProfile.Amount = Amount;
        userProfile.Name = Name;
        string [] words = { "Jhon", "Bob", "Raule" };
        userProfile.Name = string.Join(",", words[1..^1]);
        return userProfile;
        
    }
}