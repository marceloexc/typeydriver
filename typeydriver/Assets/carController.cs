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

    public Rigidbody rb;
    public followTarget followTargetScript;
    public bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // followTargetScript should be assigned in the Inspector to the character's followTarget component
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // lower center for stability
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (followTargetScript == null || followTargetScript.target == null || !canMove) return; // Don't control car if not mounted or movement disabled
        Move();
        Turn();
        ApplyDrift();
    }

    void Move()
    {
        float vertical = Input.GetAxis("Vertical"); // W/S or Up/Down
        Vector3 forwardForce = transform.forward * vertical * acceleration;

        // Limit max speed
        if (rb.velocity.magnitude < maxSpeed)
            rb.AddForce(forwardForce, ForceMode.Acceleration);
    }

    void Turn()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right

        if (rb.velocity.magnitude > 0.1f) // only turn if moving
        {
            float turn = horizontal * turnSpeed * Time.fixedDeltaTime;
            transform.Rotate(0, turn, 0);
        }
    }

    void ApplyDrift()
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.velocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(rb.velocity, transform.right) * driftFactor;

        rb.velocity = forwardVelocity + rightVelocity;
    }
}