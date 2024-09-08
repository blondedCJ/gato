using UnityEngine;
using System.Collections;

public class PanelManagerWithFade : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f; // Duration of the fade transition
    private bool isPanelActive = false;

    void Start()
    {
        // Ensure the panel starts hidden and non-interactable
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        isPanelActive = false;
    }

    // Method to show the panel and pause the game (for the "Hamburger" button)
    public void ShowPanelAndPause()
    {
        if (!isPanelActive) // Only trigger if the panel is not already active
        {
            StopAllCoroutines(); // Ensure no coroutines are overlapping
            StartCoroutine(FadeInAndPause());
        }
    }

    // Method to hide the panel and resume the game (for the "Resume" button)
    public void HidePanelAndResume()
    {
        if (isPanelActive) // Only trigger if the panel is active
        {
            StopAllCoroutines(); // Ensure no coroutines are overlapping
            StartCoroutine(FadeOutAndResume());
        }
    }

    // Coroutine to fade in the panel and pause the game
    private IEnumerator FadeInAndPause()
    {
        float elapsedTime = 0f;

        // Make the panel interactive as we start fading in
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Ensure the panel is fully visible at the end
        canvasGroup.alpha = 1;

        Time.timeScale = 0; // Pause the game
        isPanelActive = true; // Update the panel state
    }

    // Coroutine to fade out the panel and resume the game
    private IEnumerator FadeOutAndResume()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Make the panel non-interactive and invisible
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Time.timeScale = 1; // Resume the game
        isPanelActive = false; // Update the panel state
    }
}
