using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawerObstacles : MonoBehaviour
{
    [Header("Spawn Settings")] 
    [SerializeField] private List<GameObject> obstaclePrefabs;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Movement Settings")] 
    [SerializeField] private Vector3 movementDirection = Vector3.back;
    [SerializeField] private float startSpeed = 5f;          // стартовая скорость мира
    [SerializeField] private float accelerationPerSecond = 0.5f; // ускорение
    [SerializeField] private float maxSpeedLimit = 15f;      // максимум скорости

    [Header("Life Time Settings")] 
    [SerializeField] private float obstacleLifetime = 10f;

    [Header("Score Settings")] 
    [SerializeField] private float baseScoreRate = 10f; // очки в секунду при старте

    private float currentWorldSpeed;
    private float currentScoreRate;
    private float score;

    [Header("UI References")] 
    [SerializeField] private TextMeshPro scoreText; 
    [SerializeField] private TextMeshPro speedText; 
    [SerializeField] private bool autoFindUI = true; 

    private float lastSpawnTime;
    private float nextSpawnDelay = 0f;
    private List<MovingObstacle2> spawnedObstacles = new List<MovingObstacle2>();

    void Start()
    {
        lastSpawnTime = Time.time;
        currentWorldSpeed = startSpeed;
        currentScoreRate = baseScoreRate;

        SpawnObstacle(); // первое препятствие сразу
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        // === Ускоряем "мир" ===
        currentWorldSpeed = Mathf.Min(currentWorldSpeed + accelerationPerSecond * deltaTime, maxSpeedLimit);

        // === Обновляем очки ===
        score += currentScoreRate * deltaTime;
        currentScoreRate += accelerationPerSecond * deltaTime; // растет вместе с ускорением

        // === Обновляем UI ===
        UpdateUI();

        // === Обновляем движение препятствий ===
        foreach (var obstacle in spawnedObstacles)
        {
            if (obstacle != null)
                obstacle.SetSpeed(currentWorldSpeed);
        }

        // === Проверяем, можно ли спавнить новое препятствие ===
        if (Time.time - lastSpawnTime >= spawnInterval + nextSpawnDelay)
        {
            SpawnObstacle();
            lastSpawnTime = Time.time;
        }

        // Удаляем уничтоженные объекты из списка
        spawnedObstacles.RemoveAll(obj => obj == null);
    }

    void SpawnObstacle()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Count == 0)
        {
            Debug.LogWarning("No obstacle prefabs assigned!");
            return;
        }

        GameObject selectedPrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
        Vector3 spawnPosition = transform.position;

        GameObject newObstacle = Instantiate(selectedPrefab, spawnPosition, transform.rotation);

        // Берем длину препятствия
        float obstacleLength = 1f;
        ObstacleData data = newObstacle.GetComponent<ObstacleData>();
        if (data != null)
            obstacleLength = data.obstacleLength;

        // Добавляем контроллер движения
        MovingObstacle2 obstacleController = newObstacle.GetComponent<MovingObstacle2>();
        if (obstacleController == null)
            obstacleController = newObstacle.AddComponent<MovingObstacle2>();

        obstacleController.Initialize(movementDirection, currentWorldSpeed, obstacleLifetime);
        spawnedObstacles.Add(obstacleController);

        // === Рассчитываем задержку между спавнами по длине ===
        nextSpawnDelay = obstacleLength / currentWorldSpeed;
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {Mathf.FloorToInt(score)}";
        if (speedText != null)
            speedText.text = $"Speed: {currentWorldSpeed:F1} u/s";
    }

    // === Класс движения препятствий ===
    public class MovingObstacle2 : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float lifetime;
        private float spawnTime;

        public void Initialize(Vector3 moveDirection, float moveSpeed, float lifeTime)
        {
            direction = moveDirection.normalized;
            speed = moveSpeed;
            lifetime = lifeTime;
            spawnTime = Time.time;
        }

        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }

        void Update()
        {
            transform.position += direction * speed * Time.deltaTime;

            if (Time.time - spawnTime >= lifetime)
                Destroy(gameObject);
        }
    }
}
