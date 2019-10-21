using System;
using UnityEngine;

public class FxRVideoProjectionMode : MonoBehaviour
{
    public PROJECTION_MODE Projection;

    [Serializable]
    public enum PROJECTION_MODE
    {
        VIDEO_PROJECTION_2D = 0,
        VIDEO_PROJECTION_360 = 1,
        VIDEO_PROJECTION_360S = 2, // 360 stereo
        VIDEO_PROJECTION_180 = 3,
        VIDEO_PROJECTION_180LR = 4, // 180 left to right
        VIDEO_PROJECTION_180TB = 5, // 180 top to bottom
        VIDEO_PROJECTION_3D = 6 // 3D side by side

    }
}
