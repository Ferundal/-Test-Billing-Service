namespace Billing.Services;

public class User
{
    private long Amount;
    private string Name;
    private long Rating;

    public UserProfile ToUserProfile()
    {
        var userProfile = new UserProfile();
        userProfile.Amount = Amount;
        userProfile.Name = Name;
        return userProfile;
    }
}