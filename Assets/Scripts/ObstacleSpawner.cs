using System.Collections.Generic;
using UnityEngine;

public class SpawerObstacles : MonoBehaviour
{
   
    [Header("Spawn Settings")] 
    [SerializeField] private List<GameObject> obstaclePrefabs;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool useWeightedRandom = false;

    [Header("Movement Settings")] 
    [SerializeField] private Vector3 movementDirection = Vector3.back;
    [SerializeField] private float minSpeed = 3f;
    [SerializeField] private float maxSpeed = 7f;
    [SerializeField] private float accelerationPerSecond = 0.5f; // ускорение всех объектов
    [SerializeField] private float maxSpeedLimit = 15f; // ограничение максимальной скорости

    [Header("Life Time Settings")] 
    [SerializeField] private float obstacleLifetime = 10f;

    [Header("Score Settings")]
    [SerializeField] private float baseScoreRate = 10f; // очки в секунду при старте
    private float currentScoreRate;
    private float score;

    private float lastSpawnTime;
    private float nextSpawnDelay = 0f; // задержка после длины объекта
    private List<MovingObstacle2> spawnedObstacles = new List<MovingObstacle2>();

    void Start()
    {
        lastSpawnTime = Time.time;
        currentScoreRate = baseScoreRate;

        SpawnObstacle(); // первое препятствие сразу
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        // === Ускоряем все объекты ===
        foreach (var obstacle in spawnedObstacles)
        {
            if (obstacle != null)
                obstacle.ApplyAcceleration(accelerationPerSecond * deltaTime, maxSpeedLimit);
        }

        // === Обновляем очки ===
        score += currentScoreRate * deltaTime;
        currentScoreRate += accelerationPerSecond * deltaTime; // растет пропорционально ускорению

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

        // Выбираем случайный префаб
        GameObject selectedPrefab = GetRandomPrefab();

        // Определяем позицию спавна
        Vector3 spawnPosition = transform.position;


        // Создаем препятствие
        GameObject newObstacle = Instantiate(selectedPrefab, spawnPosition, transform.rotation);

        // Генерируем случайную скорость
        float randomSpeed = Random.Range(minSpeed, maxSpeed);

        // Добавляем компонент для управления движением и временем жизни
        MovingObstacle2 obstacleController = newObstacle.GetComponent<MovingObstacle2>();
        if (obstacleController == null)
            obstacleController = newObstacle.AddComponent<MovingObstacle2>();

        float obstacleLength = 1f; // значение по умолчанию
        ObstacleData data = newObstacle.GetComponent<ObstacleData>();
        if (data != null)
            obstacleLength = data.obstacleLength;

        obstacleController.Initialize(movementDirection, randomSpeed, obstacleLifetime);
        spawnedObstacles.Add(obstacleController);

        // === Вычисляем задержку для следующего спавна ===
        nextSpawnDelay = obstacleLength / randomSpeed;
    }

    GameObject GetRandomPrefab()
    {
        if (obstaclePrefabs.Count == 1)
            return obstaclePrefabs[0];
        int randomIndex = Random.Range(0, obstaclePrefabs.Count);
        return obstaclePrefabs[randomIndex];
    }

    // Класс управления движением
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

        void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
            if (Time.time - spawnTime >= lifetime)
                Destroy(gameObject);
        }

        public void ApplyAcceleration(float deltaSpeed, float maxSpeed)
        {
            speed = Mathf.Min(speed + deltaSpeed, maxSpeed);
        }
    }
}