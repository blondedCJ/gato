using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

public class XRSceneManager : MonoBehaviour
{
    private void OnEnable()
    {
        // Subscribe to the scene loaded and unloaded events
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from the scene loaded and unloaded events
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "AR Scene") // Replace with your AR scene name
        {
            InitializeXR();
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "AR Scene") // Replace with your AR scene name
        {
            StopXR();
        }
    }

    private void InitializeXR()
    {
        // Initialize the XR loader
        XRGeneralSettings.Instance.Manager.InitializeLoaderSync();

        // Check if the loader was successfully initialized
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Failed to initialize XR Loader.");
            return;
        }

        // Start the XR subsystems
        XRGeneralSettings.Instance.Manager.StartSubsystems();
    }

    private void StopXR()
    {
        // Stop the XR subsystems
        XRGeneralSettings.Instance.Manager.StopSubsystems();

        // Deinitialize the XR loader
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
    }
}
