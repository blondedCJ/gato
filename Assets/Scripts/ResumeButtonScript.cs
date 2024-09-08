using UnityEngine;
using System.Collections;

public class ResumeButtonScriptWithFade : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f; // Duration of the fade transition

    // Method to resume the game (fade out the panel and unpause)
    public void ResumeGame()
    {
        StopAllCoroutines(); // Stop any previous fade transitions
        StartCoroutine(FadeOutAndResume());
    }

    // Coroutine to fade out the panel and resume the game
    private IEnumerator FadeOutAndResume()
    {
        float elapsedTime = 0f;

        // Fade out the panel
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Complete fade-out
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Resume the game
        Time.timeScale = 1;
    }
}
