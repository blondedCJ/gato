using UnityEngine;

public class OrientationController : MonoBehaviour
{
    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft; // or ScreenOrientation.LandscapeRight
    }
}
