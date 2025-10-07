using UnityEngine;

public class SpawnerLight : MonoBehaviour
{
    Header("Spawn Settings")] [SerializeField]
    private GameObject obstaclePrefab; // Префаб препятствия

    [SerializeField] private float spawnInterval = 2f; // Периодичность спавна в секундах

    [Header("Movement Settings")]
    [SerializeField]
    private Vector3 movementDirection = Vector3.back; // Направление движения препятствий

    [SerializeField] private float Speed = 3f; // Минимальная скорость


    [Header("Life Time Settings")]
    [SerializeField]
    private float obstacleLifetime = 10f; // Время жизни препятствия


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
        if (obstaclePrefab == null)
        {
            Debug.LogWarning("Obstacle prefab is not assigned!");
            return;
        }

        // Определяем позицию спавна
        Vector3 spawnPosition = transform.position;

        // Создаем препятствие
        GameObject newObstacle = Instantiate(obstaclePrefab, spawnPosition, transform.rotation);

        // Добавляем компонент для управления движением и временем жизни
        MovingObstacle obstacleController = newObstacle.GetComponent<MovingObstacle>();
        if (obstacleController == null)
        {
            obstacleController = newObstacle.AddComponent<MovingObstacle>();
        }

        obstacleController.Initialize(movementDirection, Speed, obstacleLifetime);

        // Добавляем в список
        spawnedObstacles.Add(newObstacle);
    }

    // Визуализация в редакторе
}

// Компонент для управления движением препятствий
public class MovingObstacle : MonoBehaviour
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
