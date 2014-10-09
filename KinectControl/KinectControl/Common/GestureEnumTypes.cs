
namespace KinectControl.Common
{
    /// <summary>
    /// the gesture part result
    /// </summary>
    public enum GesturePartResult
    {
        /// <summary>
        /// Gesture part fail
        /// </summary>
        Fail,

        /// <summary>
        /// Gesture part suceed
        /// </summary>
        Succeed,

        /// <summary>
        /// Gesture part result undetermined
        /// </summary>
        Pausing
    }

    /// <summary>
    /// The gesture type
    /// </summary>
    public enum GestureType
    {
        None,
        WaveLeft,
        SwipeUp,
        SwipeDown,
        SwipeLeft,
        SwipeRight,
        Menu,
        ZoomIn,
        ZoomOut,
        JoinedHands,
    }
}
