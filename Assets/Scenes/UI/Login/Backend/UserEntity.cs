using System;
using Unisave;
using Unisave.Entities;

[EntityCollectionName("Users")]
public class UserEntity : Entity
{
    /// <summary>
    /// Email of the player
    /// </summary>
    // [DontLeaveServer]
    //  \_ Should ideally also be hidden so that you don't leak the email
    //     to other players. But it's left visible for this demo project.
    public string email;

    /// <summary>
    /// Hashed password of the player
    /// </summary>
    [DontLeaveServer]
    public string password;

    /// <summary>
    /// When was the last time the player has logged in
    /// </summary>
    public DateTime lastLoginAt = DateTime.UtcNow;

    //Data
    public int totalSteps = 0;
    public float totalDistance = 0;
    public float averageSpeed = 0;
    public int totalPrevent = 0;
}