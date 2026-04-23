using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleRigidbodyCar : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration = 20f;       // Forward/backward speed
    public float turnSpeed = 50f;          // Rotation speed
    public float maxSpeed = 25f;           // Top speed

    [Header("Drift Settings")]
    [Range(0f, 1f)]
    public float driftFactor = 0.95f;      // 1 = no drift, lower = more slide

    [Header("Startup")]
    public bool canMove = false;
    public bool autoDriveEnabled = false;

    public Rigidbody rb;
    public followTarget followTargetScript;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // followTargetScript should be assigned in the Inspector to the character's followTarget component
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // lower center for stability
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (followTargetScript == null || followTargetScript.target == null || !canMove) return; // prevent car control if not mounted or movement disabled

        bool manualInput = HasMovementInput();

        if (autoDriveEnabled && !manualInput)
        {
            AutoMoveForward();
            return;
        }

        if (manualInput)
            autoDriveEnabled = false;

        Move();
        Turn();
        ApplyDrift();
    }

    void AutoMoveForward()
    {
        Vector3 forwardForce = transform.forward * acceleration;
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVelocity.magnitude < maxSpeed)
            rb.AddForce(forwardForce, ForceMode.Acceleration);

        ApplyDrift();
    }

    void Move()
    {
        float vertical = Input.GetAxis("Vertical"); // W/S or Up/Down
        Vector3 forwardForce = transform.forward * vertical * acceleration;
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit max speed on horizontal movement only
        if (flatVelocity.magnitude < maxSpeed)
            rb.AddForce(forwardForce, ForceMode.Acceleration);
    }

    void Turn()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVelocity.magnitude > 0.1f) // only turn if moving horizontally
        {
            float turn = horizontal * turnSpeed * Time.fixedDeltaTime;
            transform.Rotate(0, turn, 0);
        }
    }

    void ApplyDrift()
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.velocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(rb.velocity, transform.right) * driftFactor;
        Vector3 upVelocity = Vector3.up * Vector3.Dot(rb.velocity, Vector3.up);

        rb.velocity = forwardVelocity + rightVelocity + upVelocity;
    }

    bool HasMovementInput()
    {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f;
    }

    public void EnableAutoDrive()
    {
        canMove = true;
        autoDriveEnabled = true;
    }
}