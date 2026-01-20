// LeaderboardManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LeaderboardManager : MonoBehaviour
{
    private static LeaderboardManager instance;
    public static LeaderboardManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LeaderboardManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("LeaderboardManager");
                    instance = obj.AddComponent<LeaderboardManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    private LeaderboardData leaderboardData;
    private string savePath;

    [Header("Настройки")]
    public int maxEntries = 10;
    public string defaultPlayerName = "Игрок";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        savePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");
        LoadLeaderboard();
    }

    public void AddScore(int score, string playerName = null)
    {
        if (string.IsNullOrEmpty(playerName))
            playerName = defaultPlayerName;
            
        ScoreEntry newEntry = new ScoreEntry(playerName, score);
        leaderboardData.AddScore(newEntry);
        
        // Ограничиваем количество записей
        if (leaderboardData.scores.Count > maxEntries)
        {
            leaderboardData.scores.RemoveRange(maxEntries, leaderboardData.scores.Count - maxEntries);
        }
        
        SaveLeaderboard();
    }

    public List<ScoreEntry> GetTopScores()
    {
        return leaderboardData.GetTopScores(maxEntries);
    }

    public void ClearLeaderboard()
    {
        leaderboardData = new LeaderboardData();
        SaveLeaderboard();
    }

    private void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(leaderboardData, true);
        File.WriteAllText(savePath, json);
    }

    private void LoadLeaderboard()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            leaderboardData = JsonUtility.FromJson<LeaderboardData>(json);
        }
        else
        {
            leaderboardData = new LeaderboardData();
            // Добавляем тестовые данные
            AddTestData();
        }
    }

    private void AddTestData()
    {
        AddScore(1500, "Алексей");
        AddScore(1200, "Мария");
        AddScore(900, "Иван");
        AddScore(800, "Екатерина");
        AddScore(600, "Дмитрий");
    }
}