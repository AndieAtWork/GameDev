using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public int xPosition;
    public int yPosition;
    public int currentRoom;

    public static float rotationSpeed = 140f;
    public static float moveSpeedForward = 7.5f;
    public static float moveSpeedBackward = 5f;

    // For casting movement
    public float radius = 0.2f; // The radius of the player
    public float castDistance = 0.0f; // The distance allowed
    public LayerMask wallLayer;

    private SpriteRenderer _renderer;

    public bool canBeWarped = true; // When you just crossed a gate, you can't cross again unless you move into its space again
    public bool forward = true;

    public static PlayerBehaviour Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.transform.position = new Vector2(xPosition, yPosition);
    }

    // Update is called once per frame
    void Update()
    {
        // Rotation
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
        // Rotation
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }

        // Moving forward
        if (Input.GetKey(KeyCode.W))
        {
            forward = true; // Affects rotation when crossing doors
            Vector2 direction = (Vector2)transform.up * 1; // The movement is set as a raycast to see if it will hit an Obstacle
            RaycastHit2D hit = Physics2D.CircleCast(
                transform.position,
                radius,
                direction,
                radius + castDistance,
                wallLayer
            );
            // If there is no collision, the player moves
            if (hit.collider == null)
            {
                transform.position += transform.up * moveSpeedForward * Time.deltaTime;
                if (!canBeWarped)
                {
                    // When you cross an gate, it is set to false until your next effective movement
                    Debug.Log("can be warped!");
                    canBeWarped = true;
                }
            }
        }

        // You can only move backwards if you're not moving forward
        if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
        {
            forward = false; // Affects rotation when crossing doors
            Vector2 direction = (Vector2)transform.up * -1;
            RaycastHit2D hit = Physics2D.CircleCast(
                transform.position,
                radius,
                direction,
                radius + castDistance,
                wallLayer
            );
            // If there is no collision, the player moves
            if (hit.collider == null)
            {
                transform.position -= transform.up * moveSpeedBackward * Time.deltaTime;
                if (!canBeWarped)
                {
                    Debug.Log("can be warped!");
                    canBeWarped = true;
                }
            }
        }
    }
}
