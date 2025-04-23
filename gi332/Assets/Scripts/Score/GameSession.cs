using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[System.Serializable]
public class ScoreSaveData
{
    public List<MapScoreData> scores;
}

public class GameSession : MonoBehaviour
{
    public static GameSession Instance;
    public List<MapScoreData> scores = new();
    public string currentMapId;
    
    private string SavePath => Application.persistentDataPath + "/scores.json";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScores();
        } 
        else Destroy(gameObject);
    }

    public void SaveScore(MapScoreData data)
    {
        var existing = scores.Find(x => x.mapId == data.mapId);
        if (existing != null) scores.Remove(existing);
        scores.Add(data);
        SaveScores();
    }

    public MapScoreData GetScoreForMap(string mapId)
    {
        return scores.Find(x => x.mapId == mapId);
    }
    
    private void SaveScores()
    {
        ScoreSaveData saveData = new() { scores = scores };
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json, Encoding.UTF8);
    }

    private void LoadScores()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath, Encoding.UTF8);
            ScoreSaveData loadedData = JsonUtility.FromJson<ScoreSaveData>(json);
            scores = loadedData.scores;
        }
    }
}
