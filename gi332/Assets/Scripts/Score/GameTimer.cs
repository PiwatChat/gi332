using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public float timeLimit = 60f;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private bool isRunning = true;
    
    private float elapsedTime = 0f;

    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        float remaining = Mathf.Max(0, timeLimit - elapsedTime);
        timerText.text = $"Time: {elapsedTime:F1}s";
    }

    public float GetElapsedTime() => elapsedTime;
}
