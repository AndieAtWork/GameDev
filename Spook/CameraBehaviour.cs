using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform player;
    public static float rotationSpeed;

    public static CameraBehaviour Instance { get; private set; }

    private void Awake()
    {
        // The Maze is a singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Player is set with it's atttributes
        player = PlayerBehaviour.Instance.transform;
        rotationSpeed = PlayerBehaviour.rotationSpeed;
    }

    void LateUpdate()
    {
        // Camera follows player in position
        transform.position = new Vector3(
            player.position.x,
            player.position.y,
            transform.position.z
        );

        // Player rotation affects the camera
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }

    void Update()
    {
    }
}
