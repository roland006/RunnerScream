using UnityEngine;

public class Controls : MonoBehaviour
{

    private Vector2Int localPos = Vector2Int.zero;

    public float blockSize = 3f;
    public float smoothing = 5f;
    private static Controls Instance { get; set; }
    private Vector3 TargetPosition => new(transform.position.x, localPos.y * blockSize, -localPos.x * blockSize);

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && Move(Vector2Int.up)) ;
        if (Input.GetKeyDown(KeyCode.A) && Move(Vector2Int.left)) ;
        if (Input.GetKeyDown(KeyCode.S) && Move(Vector2Int.down)) ;
        if (Input.GetKeyDown(KeyCode.D) && Move(Vector2Int.right)) ;

        localPos = new Vector2Int(Mathf.Clamp(localPos.x, -1, 1), Mathf.Clamp(localPos.y, -1, 1));

        transform.position = Vector3.Lerp(transform.position, TargetPosition, smoothing * Time.deltaTime);
    }

    private bool Move(Vector2Int vector)
    {
        localPos += vector;
        return true;
    }
}
