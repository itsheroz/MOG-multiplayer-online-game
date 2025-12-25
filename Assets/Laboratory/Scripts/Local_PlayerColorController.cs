/// <summary>
/// A specialized color controller used exclusively for **Offline Mode** or single-player testing.
/// </summary>
/// <remarks>
/// In this local setup, the player is always considered the **owner (IsMine = true)** /// and is assigned a fixed, non-networked ID.
/// </remarks>
public class Local_PlayerColorController : PlayerColorController
{
    // In offline/local mode, this instance always represents the local player.
    public override bool IsMine => true;

    // Assigns a fixed ID (1) since no external network ID is required in offline mode.
    public override int OwnerID => 1;
}