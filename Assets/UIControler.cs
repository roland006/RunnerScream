using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    using Game;

    public class UIController : MonoBehaviour
    {
        [SerializeField] private SpawnerObstacles spawnerObstacles;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject pauseButton;
        [SerializeField] private GameObject gameOverPanel;
        public static GameObject PauseButton => Instance.pauseButton; // => используется только когда get, нет settera

        //public static GameObject GetPauseButton() { return Instance.pauseButton; } - тоже самое

        [SerializeField] private RectTransform healthBar;
        [SerializeField] private float healthBarElementSize = 20f;

        private static UIController Instance { get; set; }

        [SerializeField] private Text costText;
        [SerializeField] private Text moneyText;
        [SerializeField] private float hardMoney = 100;
        [SerializeField] private Button continueButton;

        [SerializeField] private GameObject finalGameOverPanel;
        [SerializeField] private Text totalScoreText;
        [SerializeField] private GameObject NewRecordText;
        [SerializeField] private GameObject LastRecordText;
        [SerializeField] private Text LastRecordAmount;
       
        private float cost;

        private float record;

        private void Awake()
        {
            Instance = this;
        }

        public void SetPaused(bool value)
        {
            pausePanel.SetActive(value);
            Time.timeScale = value ? 0f : 1f;
        }

        public void LoadGarage()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public static void SetHealth(int health)
        {
            var size = Instance.healthBar.sizeDelta;
            size.x = Instance.healthBarElementSize * health;
            Instance.healthBar.sizeDelta = size;
        }        
     

        public void OnGameOver(bool noTimer = false)
        {
            Time.timeScale = 0f;
            PauseButton.SetActive(false);
            
            gameOverPanel.SetActive(true);
            

            cost = 1 << PlayerHealth.DeathsCount;

            costText.text = cost.ToString();
            moneyText.text = hardMoney.ToString();
            continueButton.interactable = hardMoney >= cost;
            //continueByAdButton.interactable = Application.internetReachability != NetworkReachability.NotReachable; задел на ревайв за рекламу
        }

        public void ShowResults()
        {
            gameOverPanel.SetActive(false);
            finalGameOverPanel.SetActive(true);
            var currentScore =  spawnerObstacles.Score;
            totalScoreText.text =  Mathf.FloorToInt(currentScore).ToString();

            if (record < currentScore)
            {
                record = currentScore;
                NewRecordText.SetActive(true);
            }
            else
            {
                LastRecordText.SetActive(true);
                LastRecordAmount.text = record.ToString();
            }
        }

        public void Continue()
        {
            hardMoney -= cost;
            PlayerHealth.Revive();
            Time.timeScale = 1f;
            PauseButton.SetActive(true);
            gameOverPanel.SetActive(false);
        }
    }
}