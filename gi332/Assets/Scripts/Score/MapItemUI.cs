using UnityEngine;
using TMPro;

public class MapItemUI : MonoBehaviour
{
    public string mapId;
    public TMP_Text scoreText;

    private void OnEnable()
    {
        UpdateScoreDisplay();
    }

    public void UpdateScoreDisplay()
    {
        var data = GameSession.Instance?.GetScoreForMap(mapId);
        if (data != null)
            scoreText.text = $"{data.GetScore()}/3";
        else
            scoreText.text = "0/3";
    }
}