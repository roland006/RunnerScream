using System.Collections;
using UnityEngine;

public class Controls : MonoBehaviour
{

    private Vector2Int localPos = Vector2Int.zero;

    public float blockSize = 3f;
    public float smoothing = 5f;
    public GameObject Car;
    public Vector2 tiltAngle = Vector2.one * 20f;
    public float tiltSpeed = 1f;
    public float tiltTime = 1f;

    private Vector2 targetTilt;

    private static Controls Instance { get; set; }
    private Vector3 TargetPosition => new(transform.position.x, localPos.y * blockSize, -localPos.x * blockSize);

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && Move(Vector2Int.up) ) StartCoroutine(DoTilt(Vector2.up));
        if (Input.GetKeyDown(KeyCode.A) && Move(Vector2Int.left)) StartCoroutine(DoTilt(Vector2.left));
        if (Input.GetKeyDown(KeyCode.S) && Move(Vector2Int.down)) StartCoroutine(DoTilt(Vector2.down)) ;
        if (Input.GetKeyDown(KeyCode.D) && Move(Vector2Int.right)) StartCoroutine(DoTilt(Vector2.right)) ;

        localPos = new Vector2Int(Mathf.Clamp(localPos.x, -1, 1), Mathf.Clamp(localPos.y, -1, 1));

        transform.position = Vector3.Lerp(transform.position, TargetPosition, smoothing * Time.deltaTime);

      

        Vector3 rot = Car.transform.eulerAngles;
        rot.x = Mathf.LerpAngle(rot.x, -targetTilt.y, tiltSpeed * Time.deltaTime);
        rot.z = Mathf.LerpAngle(rot.z, -targetTilt.x, tiltSpeed * Time.deltaTime);
        Car.transform.eulerAngles = rot;

    }

    private bool Move(Vector2Int vector)
    {
        localPos += vector;
        return true;
    }

    private IEnumerator DoTilt(Vector2 side)
    {
        targetTilt = tiltAngle * side;

        yield return new WaitForSeconds(tiltTime);

        targetTilt = Vector2.zero;
    }
}
