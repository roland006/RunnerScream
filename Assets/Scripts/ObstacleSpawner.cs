using System.Collections.Generic;
using UnityEngine;

public class SpawerObstacles : MonoBehaviour
{
   
    public List<GameObject> obstaclePrefabs; // Список префабов препятствий

    public float spawnInterval = 2f; // Периодичность спавна в секундах
    public bool useWeightedRandom = false; // Использовать весовую систему

    public Vector3 movementDirection = Vector3.back; // Направление движения препятствий
    public float minSpeed = 3f; // Минимальная скорость
    public float maxSpeed = 7f; // Максимальная скорость

    public float obstacleLifetime = 10f; // Время жизни препятствия


    private float lastSpawnTime;
    private List<GameObject> spawnedObstacles = new List<GameObject>();

    void Start()
    {
        lastSpawnTime = Time.time;

        // Спавним первое препятствие сразу
        SpawnObstacle();
    }

    void Update()
    {
        // Проверяем, пришло ли время спавнить новое препятствие
        if (Time.time - lastSpawnTime >= spawnInterval)
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
        {
            obstacleController = newObstacle.AddComponent<MovingObstacle2>();
        }

        obstacleController.Initialize(movementDirection, randomSpeed, obstacleLifetime);

        // Добавляем в список
        spawnedObstacles.Add(newObstacle);
    }

    GameObject GetRandomPrefab()
    {
        if (obstaclePrefabs.Count == 1)
            return obstaclePrefabs[0];
        int randomIndex = Random.Range(0, obstaclePrefabs.Count);
        return obstaclePrefabs[randomIndex];
    }

    // Компонент для управления движением препятствий
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
            // Движение препятствия
            transform.position += direction * speed * Time.deltaTime;

            // Проверка времени жизни
            if (Time.time - spawnTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}