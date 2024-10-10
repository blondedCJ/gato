using UnityEngine;
using UnityEngine.UI;

public class PetStatus : MonoBehaviour
{
    public float hunger = 100f;
    public float thirst = 100f;
    public float affection = 100f;

    public float hungerDecayRate = 0.5f;   // Hunger decreases by 0.5 units per second
    public float thirstDecayRate = 0.7f;   // Thirst decreases by 0.7 units per second
    public float affectionDecayRate = 0.3f; // Affection decreases by 0.3 units per second

    // Serialized fields for UI elements
    [SerializeField] public Slider hungerBar;
    [SerializeField] public Slider thirstBar;
    [SerializeField] public Slider affectionBar;

    // Serialized field for the reset button
    [SerializeField] private Button resetButton;

    private void Start()
    {
        LoadStatus();
        UpdateUI();

        string lastPlayedTime = PlayerPrefs.GetString("LastPlayedTime", System.DateTime.Now.ToString());
        System.DateTime lastDateTime = System.DateTime.Parse(lastPlayedTime);
        System.TimeSpan timeDifference = System.DateTime.Now - lastDateTime;

        UpdateStatusAfterTimePassed((float)timeDifference.TotalSeconds);

        resetButton.onClick.AddListener(ResetStatus);
    }

    private void Update()
    {
        UpdateStatus();
        UpdateUI();
    }

    private void UpdateStatus()
    {
        hunger = Mathf.Max(0, hunger - hungerDecayRate * Time.deltaTime);
        thirst = Mathf.Max(0, thirst - thirstDecayRate * Time.deltaTime);
        affection = Mathf.Max(0, affection - affectionDecayRate * Time.deltaTime);
    }

    public void IncreaseAffection(float amount)
    {
        affection = Mathf.Clamp(affection + amount, 0f, 100f); // Ensure affection is within bounds
    }

    private void UpdateUI()
    {
        hungerBar.value = hunger / 100f;
        thirstBar.value = thirst / 100f;
        affectionBar.value = affection / 100f;
    }

    public void FeedPet(float amount)
    {
        hunger = Mathf.Min(100, hunger + amount);
        UpdateUI();
        SaveStatus();
    }

    public void GiveWater(float amount)
    {
        thirst = Mathf.Min(100, thirst + amount);
        UpdateUI();
        SaveStatus();
    }

    public void PlayWithPet(float amount)
    {
        affection = Mathf.Min(100, affection + amount);
        UpdateUI();
        SaveStatus();
    }

    public void IncreaseHungerBy(float amount)
    {
        hunger = Mathf.Min(100, hunger + amount);
        Debug.Log($"Hunger increased: {hunger}"); // Debug log
        UpdateUI(); // Update the UI
        SaveStatus(); // Save the updated status
    }

    private void SaveStatus()
    {
        PlayerPrefs.SetFloat("Hunger", hunger);
        PlayerPrefs.SetFloat("Thirst", thirst);
        PlayerPrefs.SetFloat("Affection", affection);
        PlayerPrefs.SetString("LastPlayedTime", System.DateTime.Now.ToString());
    }

    private void LoadStatus()
    {
        hunger = PlayerPrefs.GetFloat("Hunger", 100f);
        thirst = PlayerPrefs.GetFloat("Thirst", 100f);
        affection = PlayerPrefs.GetFloat("Affection", 100f);
    }

    private void OnApplicationQuit()
    {
        SaveStatus();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveStatus();
        }
    }

    private void UpdateStatusAfterTimePassed(float secondsPassed)
    {
        hunger = Mathf.Max(0, hunger - hungerDecayRate * secondsPassed);
        thirst = Mathf.Max(0, thirst - thirstDecayRate * secondsPassed);
        affection = Mathf.Max(0, affection - affectionDecayRate * secondsPassed);
    }

    public void ResetStatus()
    {
        hunger = 100f;
        thirst = 100f;
        affection = 100f;
        UpdateUI();
        SaveStatus();
    }
}
