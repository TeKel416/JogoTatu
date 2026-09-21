using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CountdownTimer : MonoBehaviour
{
    [Tooltip("Tempo inicial em segundos. 180 = 3 minutos.")]
    public float startTime = 300f;

    public TextMeshProUGUI timerText;

    private float timeRemaining;
    private bool timerRunning;

    private void Start()
    {
        timeRemaining = startTime;
        timerRunning = true;
        UpdateDisplay();
    }

    private void Update()
    {
        if (!timerRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerRunning = false;
            OnTimerEnd();
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

      
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnTimerEnd()
    {
        Debug.Log("Tempo esgotado!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // FUNCAO PLACEHOLDER PRO J1
    public void PauseTimer()
    {
        timerRunning = false;
        timerText.text = "PARABÉNS! VOCÊ VENCEU!";

        Invoke("GoToMenu", 5f); // FUNCAO PLACEHOLDER PRO J1
    }

    // FUNCAO PLACEHOLDER PRO J1
    void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
