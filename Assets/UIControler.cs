using UnityEngine;
using UnityEngine.SceneManagement;

public class UIControler : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

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
}