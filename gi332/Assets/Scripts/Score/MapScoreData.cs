using UnityEngine;

[System.Serializable]
public class MapScoreData
{
    public string mapId;
    public bool win;
    public bool noDeath;
    public bool timeChallenge;
    
    public int GetScore()
    {
        int score = 0;
        if (win) score++;
        if (noDeath) score++;
        if (timeChallenge) score++;
        return score;
    }
}
