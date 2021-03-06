namespace VRSF.MoveAround.Teleport
{
    /// <summary>
    /// Contains all the basic methods for the Teleport Systems.
    /// </summary>
    public interface ITeleportSystem
    {
        void TeleportUser(ITeleportFilter teleportFilter);
    }
}