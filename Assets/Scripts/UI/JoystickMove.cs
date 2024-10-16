using UnityEngine;

public class JoystickMove : MonoBehaviour
{
    public Joystick movementJoystick;
    public float playerSpeed;
    private Rigidbody rb;
    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(movementJoystick.Direction.x, 0, movementJoystick.Direction.y);
        direction = mainCamera.transform.TransformDirection(direction);
        direction.y = 0;

        if (direction.magnitude >= 0.1f)
        {
            //rb.velocity = direction.normalized * playerSpeed;
            rb.AddForce(direction.normalized * playerSpeed);
            transform.rotation = Quaternion.LookRotation(direction.normalized);
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }
}