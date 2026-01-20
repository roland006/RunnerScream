// LeaderboardUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Ссылки UI")]
    public GameObject leaderboardPanel;
    public Transform entriesContainer;
    public GameObject entryPrefab;
    public Button closeButton;
    public Button refreshButton;
    public Text titleText;
    
    [Header("Настройки")]
    public string leaderboardTitle = "ТАБЛИЦА ЛИДЕРОВ";
    public Color firstPlaceColor = Color.yellow;
    public Color secondPlaceColor = Color.gray;
    public Color thirdPlaceColor = new Color(0.8f, 0.45f, 0.2f); // Бронзовый
    public Color defaultColor = Color.white;
    
    private void Start()
    {
        if (titleText != null)
            titleText.text = leaderboardTitle;
            
        if (closeButton != null)
            closeButton.onClick.AddListener(() => leaderboardPanel.SetActive(false));
            
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshLeaderboard);
            
        leaderboardPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        // Очищаем контейнер
        foreach (Transform child in entriesContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Получаем топ игроков
        List<ScoreEntry> topScores = LeaderboardManager.Instance.GetTopScores();
        
        if (topScores.Count == 0)
        {
            GameObject emptyEntry = Instantiate(entryPrefab, entriesContainer);
            Text entryText = emptyEntry.GetComponentInChildren<Text>();
            if (entryText != null)
                entryText.text = "Записей пока нет";
            return;
        }
        
        // Создаем записи
        for (int i = 0; i < topScores.Count; i++)
        {
            ScoreEntry entry = topScores[i];
            GameObject entryObj = Instantiate(entryPrefab, entriesContainer);
            
            // Настраиваем цвет в зависимости от места
            Image entryImage = entryObj.GetComponent<Image>();
            if (entryImage != null)
            {
                switch (i)
                {
                    case 0: entryImage.color = firstPlaceColor; break;
                    case 1: entryImage.color = secondPlaceColor; break;
                    case 2: entryImage.color = thirdPlaceColor; break;
                    default: entryImage.color = defaultColor; break;
                }
            }
            
            // Устанавливаем текст
            Text entryText = entryObj.GetComponentInChildren<Text>();
            if (entryText != null)
            {
                string dateString = entry.date.ToString("dd.MM.yyyy");
                entryText.text = $"{entry.rank}. {entry.playerName}: {entry.score} очков ({dateString})";
            }
        }
    }
}