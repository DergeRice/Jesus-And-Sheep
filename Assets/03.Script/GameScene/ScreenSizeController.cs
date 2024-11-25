using UnityEngine;

public class ScreenSizeController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        float desiredHeight = Camera.main.orthographicSize;


        Camera.main.orthographicSize = (9 * Screen.height * desiredHeight) / (16 * Screen.width);

    }
}
