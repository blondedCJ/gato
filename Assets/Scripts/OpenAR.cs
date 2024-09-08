using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class OpenAR : MonoBehaviour
{

    public void LoadScene()
    {
        Debug.Log("Attempting to load AR scene...");
        try
        {
            SceneManager.LoadScene("AR Scene", LoadSceneMode.Single);
            Debug.Log("Scene loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error loading scene: " + ex.Message);
        }
    }

}
