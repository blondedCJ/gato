using UnityEngine;
using System.Collections;

public class PanelManagerWithFade : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f; // Duration of the fade transition
    public CanvasGroup darkOverlay; // Reference to the dark overlay CanvasGroup
    private bool isPanelActive = false;

    void Start()
    {
        // Ensure the panel starts hidden and non-interactable
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        isPanelActive = false;

        // Ensure the dark overlay starts fully transparent
        if (darkOverlay != null)
        {
            darkOverlay.alpha = 0;
            darkOverlay.interactable = false;
            darkOverlay.blocksRaycasts = false;
        }
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

    // Coroutine to fade in the panel and dark overlay, then pause the game
    private IEnumerator FadeInAndPause()
    {
        float elapsedTime = 0f;

        // Make the panel interactive as we start fading in
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Fade in the panel and dark overlay
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            canvasGroup.alpha = alpha;

            // Fade in the dark overlay if it's assigned
            if (darkOverlay != null)
            {
                darkOverlay.alpha = Mathf.Lerp(0, 0.8f, elapsedTime / fadeDuration); // Adjust the 0.5f to control how dark the overlay gets
            }

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Ensure the panel is fully visible at the end
        canvasGroup.alpha = 1;

        // Ensure the dark overlay is at the correct opacity
        if (darkOverlay != null)
        {
            darkOverlay.alpha = 0.8f; // Final opacity of the dark overlay
            darkOverlay.interactable = true;
            darkOverlay.blocksRaycasts = true;
        }

        Time.timeScale = 0; // Pause the game
        isPanelActive = true; // Update the panel state
    }

    // Coroutine to fade out the panel and dark overlay, then resume the game
    private IEnumerator FadeOutAndResume()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            canvasGroup.alpha = alpha;

            // Fade out the dark overlay if it's assigned
            if (darkOverlay != null)
            {
                darkOverlay.alpha = Mathf.Lerp(0.5f, 0, elapsedTime / fadeDuration);
            }

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Make the panel non-interactive and invisible
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Make the dark overlay non-interactive and invisible
        if (darkOverlay != null)
        {
            darkOverlay.alpha = 0;
            darkOverlay.interactable = false;
            darkOverlay.blocksRaycasts = false;
        }

        Time.timeScale = 1; // Resume the game
        isPanelActive = false; // Update the panel state
    }
}
