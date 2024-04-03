namespace MFPS.Internal.Interfaces
{

    /// <summary>
    /// Handle the UI window that appear after the finish of a match
    /// By default it collect the local player data and show it to the screen
    /// </summary>
    public interface IMFPSResumeScreen
    {
        void CollectData();
        void Show();
    }
}