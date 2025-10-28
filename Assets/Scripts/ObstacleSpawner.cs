using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

public class SpawerObstacles : MonoBehaviour
{
    [Header("Difficulty Settings")]
    [SerializeField] private List<DifficultyLevel> difficultyLevels;

    [Header("Movement Settings")]
    [SerializeField] private Vector3 movementDirection = Vector3.back;
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float accelerationPerSecond = 0.5f;
    [SerializeField] private float maxSpeedLimit = 15f;

    [Header("Life Time Settings")]
    [SerializeField] private float obstacleLifetime = 10f;

    [Header("Score Settings")]
    [SerializeField] private float baseScoreRate = 10f;

    private float currentWorldSpeed;
    private float currentScoreRate;
    private float score;

    [Header("UI References")]
    [SerializeField] private TextMeshPro scoreText;
    [SerializeField] private TextMeshPro speedText;

    private float lastSpawnTime;
    private float nextSpawnDelay = 0f;
    private List<MovingObstacle2> spawnedObstacles = new List<MovingObstacle2>();
    
    // Difficulty system variables
    private int currentDifficultyIndex = 0;
    private float currentDifficultyTimer = 0f;
    private List<int> recentSpawnedIndices = new List<int>();
    private const int MAX_HISTORY_SIZE = 3;

    [System.Serializable]
    public class DifficultyLevel
    {
        public string name;
        public List<GameObject> obstacles;
        public float duration = 30f; // How long this difficulty lasts
    }

    void Start()
    {
        lastSpawnTime = Time.time;
        currentWorldSpeed = startSpeed;
        currentScoreRate = baseScoreRate;

        // Validate difficulty levels
        if (difficultyLevels == null || difficultyLevels.Count == 0)
        {
            Debug.LogError("No difficulty levels configured!");
            return;
        }

        StartCoroutine(SpawnObstacle());
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        // Update difficulty timer
        currentDifficultyTimer += deltaTime;
        UpdateDifficultyLevel();

        // === Accelerate world ===
        currentWorldSpeed = Mathf.Min(currentWorldSpeed + accelerationPerSecond * deltaTime, maxSpeedLimit);
        WorldSpeedManager.CurrentSpeed = currentWorldSpeed;
        score += currentScoreRate * deltaTime;
        currentScoreRate += accelerationPerSecond * deltaTime;

        UpdateUI();

        foreach (var obstacle in spawnedObstacles)
            if (obstacle != null)
                obstacle.SetSpeed(currentWorldSpeed);

        spawnedObstacles.RemoveAll(obj => obj == null);
    }

    void UpdateDifficultyLevel()
    {
        if (difficultyLevels.Count == 0) return;

        float currentDuration = difficultyLevels[currentDifficultyIndex].duration;
        
        // Check if it's time to switch to next difficulty
        if (currentDifficultyTimer >= currentDuration)
        {
            currentDifficultyTimer = 0f;
            currentDifficultyIndex++;

            if (currentDifficultyIndex >= difficultyLevels.Count)
            {
                currentDifficultyIndex = 0;
            }                       
            
            recentSpawnedIndices.Clear(); // Clear history when changing difficulty
            
            Debug.Log($"Switched to difficulty: {difficultyLevels[currentDifficultyIndex].name}");
        }
    }

    IEnumerator SpawnObstacle()
    {
        GameObject selectedPrefab = GetRandomObstacleForCurrentDifficulty();
        
        if (selectedPrefab != null)
        {
            Vector3 spawnPosition = transform.position;

            GameObject newObstacle = Instantiate(selectedPrefab, spawnPosition, transform.rotation);

            float obstacleLength = 1f;
            ObstacleData data = newObstacle.GetComponent<ObstacleData>();
            if (data != null)
                obstacleLength = data.obstacleLength;

            MovingObstacle2 obstacleController = newObstacle.GetComponent<MovingObstacle2>();
            if (obstacleController == null)
                obstacleController = newObstacle.AddComponent<MovingObstacle2>();

            obstacleController.Initialize(movementDirection, currentWorldSpeed, obstacleLifetime);
            spawnedObstacles.Add(obstacleController);

            nextSpawnDelay = obstacleLength / currentWorldSpeed;
        }
        else
        {
            // Fallback delay if no obstacle was spawned
            nextSpawnDelay = 1f;
            Debug.LogWarning("No obstacle available to spawn!");
        }

        yield return new WaitForSecondsRealtime(nextSpawnDelay);
        StartCoroutine(SpawnObstacle());
    }

    GameObject GetRandomObstacleForCurrentDifficulty()
    {
        if (difficultyLevels.Count == 0) 
        {
            Debug.LogWarning("No difficulty levels configured!");
            return null;
        }

        var currentDifficulty = difficultyLevels[currentDifficultyIndex];
        
        if (currentDifficulty.obstacles == null || currentDifficulty.obstacles.Count == 0)
        {
            Debug.LogWarning($"No obstacles in difficulty level: {currentDifficulty.name}");
            return null;
        }

        // If only one obstacle available, return it
        if (currentDifficulty.obstacles.Count == 1)
            return currentDifficulty.obstacles[0];

        // Get available indices (excluding recently used ones)
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < currentDifficulty.obstacles.Count; i++)
        {
            if (!recentSpawnedIndices.Contains(i))
                availableIndices.Add(i);
        }

        // If all obstacles were recently used, clear history and use all
        if (availableIndices.Count == 0)
        {
            availableIndices = Enumerable.Range(0, currentDifficulty.obstacles.Count).ToList();
            recentSpawnedIndices.Clear();
        }

        // Select random index from available ones
        int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
        
        // Add to recent history
        recentSpawnedIndices.Add(randomIndex);
        
        // Maintain history size
        if (recentSpawnedIndices.Count > MAX_HISTORY_SIZE)
            recentSpawnedIndices.RemoveAt(0);

        return currentDifficulty.obstacles[randomIndex];
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {Mathf.FloorToInt(score)}";
        if (speedText != null)
            speedText.text = $"Speed: {currentWorldSpeed:F1} u/s\nDifficulty: {difficultyLevels[currentDifficultyIndex].name}";
    }

    // Helper method to get current difficulty info
    public string GetCurrentDifficultyInfo()
    {
        if (difficultyLevels.Count == 0) return "No difficulties";
        
        var current = difficultyLevels[currentDifficultyIndex];
        float timeLeft = current.duration - currentDifficultyTimer;
        return $"{current.name} ({timeLeft:F1}s left)";
    }
}
// === Moving obstacle class (unchanged) ===
public class MovingObstacle2 : MonoBehaviour
{
    private Vector3 direction;
    private float currentSpeed;

    void Awake()
    {
    }

    public void Initialize(Vector3 moveDirection, float moveSpeed, float lifeTime)
    {
        direction = moveDirection.normalized;
        currentSpeed = moveSpeed;
        Destroy(gameObject, lifeTime);
    }

    public void SetSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }

    void Update()
    {
        transform.position += direction * currentSpeed * Time.deltaTime;
    }

    public float GetSpeed()
    {
        return currentSpeed;
    }
}