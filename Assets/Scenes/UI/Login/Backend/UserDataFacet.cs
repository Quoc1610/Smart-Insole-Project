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

    public UserEntity SaveUserDashBoard(int total_steps, float total_distance, float avg_speed, float avg_step_length)
    {
        var player = Auth.GetPlayer<UserEntity>();
        float sessionTime = 0;
        if (player.totalDistance != 0 && player.averageSpeed != 0) sessionTime = (player.totalDistance * 1000) / player.averageSpeed;
        float time = (total_distance * 1000) / avg_speed;
        
        player.totalDistance+= total_distance;

        
        player.averageSpeed+= (player.averageSpeed * sessionTime + avg_speed*time) / (sessionTime+time);
        player.averageStepLength = (player.averageStepLength * player.totalSteps + avg_step_length * total_steps) / (player.totalSteps + total_steps);

        player.totalSteps += total_steps;
        player.Save();

        return player;
    }
}