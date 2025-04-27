using Unisave.Authentication.Middleware;
using Unisave.Facades;
using Unisave.Facets;

/// <summary>
/// An example facet for handling player data
/// </summary>
public class UserDataFacet : Facet
{
    /// <summary>
    /// Returns the logged-in player's entity
    /// </summary>
    public UserEntity DownloadLoggedInPlayer()
    {
        return Auth.GetPlayer<UserEntity>();
    }

    /// <summary>
    /// Increments the star count for the player
    /// and returns the updated player entity
    /// </summary>
    public UserEntity SaveUserSetting(float weight, float height, float bmi, float sex)
    {
        var player = Auth.GetPlayer<UserEntity>();

        player.weight = weight;
        player.height = height;
        player.bmi = bmi;
        player.sex = sex;
        player.Save();

        return player;
    }
}