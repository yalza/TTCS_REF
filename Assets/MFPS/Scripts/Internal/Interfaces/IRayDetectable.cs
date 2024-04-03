/// <summary>
/// Use to mark objects that can be detected from the local player camera ray
/// </summary>
public interface IRayDetectable
{
    /// <summary>
    /// Called when the local player is looking at this item at certain near distance
    /// </summary>
    void OnRayDetectedByPlayer();

    /// <summary>
    /// Called when the local player WAS looking at this item but then look away.
    /// </summary>
    void OnUnDetectedByPlayer();
}