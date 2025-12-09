using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class Controls : MonoBehaviour
{

    private Vector2Int localPos = Vector2Int.zero;

    public float blockSize = 3f;
    public float smoothing = 5f;
    public GameObject Car;
    public Vector2 tiltAngle = Vector2.one * 20f;
    public float tiltSpeed = 1f;
    public float tiltTime = 1f;
    private Vector3 fp;          //First touch position
    private Vector3 lp;          //Last touch position
    private bool isTouch = false;
    private bool dragDo = false;
    [SerializeField] private float dragDistance = 50f;
    int i = 0;
    private float timer;
    [SerializeField] private float tapTime = 1f;

    private Vector2 targetTilt;

    private static Controls Instance { get; set; }
    private Vector3 TargetPosition => new(transform.position.x, localPos.y * blockSize, -localPos.x * blockSize);
   // public static event System.Action<InputType> OnInput = (input) => { };

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
        Instance = this;
    }

  /*  public static InputType AllowedInput { get; set; }
    [System.Flags]
    public enum InputType
    {
        MoveRight = 0b00000001,
        MoveLeft = 0b00000010,
        MoveUp = 0b00000100,
        MoveDown = 0b00001000,

        Move = 0b00001111,

        Tap = 0b00010000,
        DoubleTap = 0b00100000,

        Any = 0b11111111,
        None = 0b00000000,
    }*/


   /* private bool CheckInput(InputType input)
    {
        var value = AllowedInput.HasFlag(input);
        if (value) OnInput(input);
        return value;
    }
*/

    void Update()
    {
        KeyboardInput();
        TouchInput();

        localPos = new Vector2Int(Mathf.Clamp(localPos.x, -1, 1), Mathf.Clamp(localPos.y, -1, 1));

        transform.position = Vector3.Lerp(transform.position, TargetPosition, smoothing * Time.deltaTime);

      

        Vector3 rot = Car.transform.eulerAngles;
        rot.x = Mathf.LerpAngle(rot.x, -targetTilt.y, tiltSpeed * Time.deltaTime);
        rot.z = Mathf.LerpAngle(rot.z, -targetTilt.x, tiltSpeed * Time.deltaTime);
        Car.transform.eulerAngles = rot;

    }

    void KeyboardInput()
    {
        if (Keyboard.current.wKey.wasPressedThisFrame && Move(Vector2Int.up)) StartCoroutine(DoTilt(Vector2.up));
        if (Keyboard.current.aKey.wasPressedThisFrame  && Move(Vector2Int.left)) StartCoroutine(DoTilt(Vector2.left));
        if (Keyboard.current.sKey.wasPressedThisFrame && Move(Vector2Int.down)) StartCoroutine(DoTilt(Vector2.down));
        if (Keyboard.current.dKey.wasPressedThisFrame && Move(Vector2Int.right)) StartCoroutine(DoTilt(Vector2.right));
    }

    private void TouchInput()
    {
        // Use the new system's Touch access instead of legacy Input.GetTouch(0)
        // Check if there is at least one active touch
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            // Get the data for the first active touch
            UnityEngine.InputSystem.EnhancedTouch.Touch touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                fp = touch.screenPosition;
                lp = touch.screenPosition;
                isTouch = true;
                dragDo = false;
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                lp = touch.screenPosition;

                if (isTouch && (Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance))
                {
                    dragDo = true;
                    isTouch = false;

                    if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                    {   // Horizontal movement is greater
                        if ((lp.x > fp.x))
                        {
                            // Right swipe: Directly call Move and StartCoroutine
                            Debug.Log("Right Swipe");
                            if (Move(Vector2Int.right))
                                StartCoroutine(DoTilt(Vector2.right));
                        }
                        else
                        {
                            // Left swipe: Directly call Move and StartCoroutine
                            Debug.Log("Left Swipe");
                            if (Move(Vector2Int.left))
                                StartCoroutine(DoTilt(Vector2.left));
                        }
                    }
                    else
                    {   // Vertical movement is greater
                        if (lp.y > fp.y)
                        {
                            // Up swipe: Directly call Move and StartCoroutine
                            Debug.Log("Up Swipe");
                            if (Move(Vector2Int.up))
                                StartCoroutine(DoTilt(Vector2.up));
                        }
                        else
                        {
                            // Down swipe: Directly call Move and StartCoroutine
                            Debug.Log("Down Swipe");
                            if (Move(Vector2Int.down))
                                StartCoroutine(DoTilt(Vector2.down));
                        }
                    }
                }
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended && !dragDo)
            {
                // Note: The tap/double-tap logic here requires your commented-out
                // 'OnTap()' and 'OnDoubleTap()' methods to be uncommented
                // and your game logic handled directly here.
                i += 1;
                timer = tapTime;

                if (timer > 0)
                {
                    if (i == 2)
                    {
                        Debug.Log("DoubleTap");
                        // Add your OnDoubleTap() logic here if needed
                        i = 0;
                    }
                    else // This handles single tap logic
                    {
                        // Add your OnTap() logic here if needed
                    }
                }
            }
        }
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
