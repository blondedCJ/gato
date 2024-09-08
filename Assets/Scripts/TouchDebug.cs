using UnityEngine;
using UnityEngine.UI;

public class TouchDebug : MonoBehaviour
{
    public Text debugText;

    void Update()
    {
        // Display the number of touches
        debugText.text = "Touch Count: " + Input.touchCount;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            debugText.text += "\nTouch " + i + ": Pos=" + t.position + " Phase=" + t.phase;
        }
    }
}
