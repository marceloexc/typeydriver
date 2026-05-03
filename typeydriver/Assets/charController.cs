using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class charController : MonoBehaviour
{

public float speed;
public Animator anim;
public followTarget followTargetScript;
public Rigidbody rb;
public Camera playerCamera;

// Jump parameters
public float jumpForce = 5f;
public float groundCheckDistance = 0.2f;

// Dash parameters
public float dashSpeed = 20f;
public float dashDuration = 0.2f;
public float dashCooldown = 1f;

private bool isGrounded;
private float dashCooldownTimer = 0f;
private bool isDashing = false;
private float dashTimer = 0f;
private bool wantJump = false;
private bool wantDash = false;
private Collider characterCollider;
private float originalDrag;

void Start ()
{
    rb = GetComponent<Rigidbody> ();
    characterCollider = GetComponent<Collider>();
    originalDrag = rb.drag;
    if (playerCamera == null)
    {
        playerCamera = Camera.main;
    }
}

void Update()
{
    // Check for input in Update (not FixedUpdate)
    if (Input.GetKeyDown(KeyCode.Space))
        wantJump = true;
    
    if (Input.GetKeyDown(KeyCode.LeftShift))
        wantDash = true;
}

void FixedUpdate ()
{
    if (followTargetScript == null || followTargetScript.target != null) return; // Don't control char if mounted
    Debug.Log("Freedom of Movement");
    
    // Update dash cooldown
    if (dashCooldownTimer > 0)
        dashCooldownTimer -= Time.fixedDeltaTime;
    
    // Update dash duration
    if (isDashing)
    {
        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0)
        {
            isDashing = false;
            // Only reset horizontal velocity, preserve vertical velocity
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            // Reset drag when dash ends
            rb.drag = originalDrag;
        }
    }
    
    IsGrounded();
    Move();
    Jump();
    Dash();
    
    UpdateFallAnimation();
}

void Move()
{
    float horizontal = Input.GetAxis("Horizontal");
    float vertical = Input.GetAxis("Vertical");
    
    Vector3 moveVec = Vector3.zero;
    
    if (playerCamera != null)
    {
        // Get camera-relative directions (ignore vertical rotation)
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        
        // Zero out the Y component to keep movement on the ground plane
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // Move relative to camera direction
        moveVec = (cameraForward * vertical + cameraRight * horizontal);
    }
    else
    {
        moveVec = new Vector3(horizontal, 0, vertical);
    }
    
    // Don't apply normal movement while dashing
    if (!isDashing)
    {
        rb.transform.position += moveVec * speed * Time.fixedDeltaTime;
    }
     
    anim.SetFloat("Speed", moveVec.magnitude);
}

void Jump()
{
    if (wantJump && isGrounded)
    {
        Debug.Log("Jumping");
        anim.SetTrigger("Jump");
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }
    
    // Cancel dash drag on jump for gliding effect + boost
    if (wantJump && isDashing)
    {
        rb.drag = originalDrag;
        
        // Apply boost in the direction player is looking
        Vector3 boostDirection = playerCamera.transform.forward;
        boostDirection.y = 0;
        boostDirection.Normalize();
        rb.velocity += boostDirection * 7f;
        
        Debug.Log("Dash-Jump Cancel");
    }
    
    wantJump = false;  // Consume jump input regardless of whether jump happened
}

void Dash()
{
    // Start dash on input
    if (wantDash && dashCooldownTimer <= 0 && !isDashing)
    {
        isDashing = true;
        anim.SetTrigger("Dash");
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        wantDash = false;
        Debug.Log("Dashing");
        
        // Apply dash impulse once
        Vector3 dashDirection = playerCamera.transform.forward;
        dashDirection.y = 0;
        dashDirection.Normalize();
        rb.velocity = new Vector3(dashDirection.x * dashSpeed, rb.velocity.y, dashDirection.z * dashSpeed);
        
        // Increase drag during dash for control
        rb.drag = 4f;
        Debug.Log(originalDrag);
    }
    
    // Velocity is reset when dash ends (handled in FixedUpdate)
}

void UpdateFallAnimation()
{
    // Set fall animation to true if airborne, false if grounded
    if (!isGrounded)
    {
        if (!isDashing)
        {
        anim.SetBool("Fall", true);
        return;
        }
    }
    else
    {
        anim.SetBool("Fall", false);
    }
}

void IsGrounded()
{
    // Use OverlapSphere slightly below the character to detect ground
    Vector3 checkPosition = rb.transform.position + Vector3.down * (groundCheckDistance + 0.1f);
    Collider[] hitColliders = Physics.OverlapSphere(checkPosition, 0.1f);
    
    isGrounded = false;
    foreach (Collider col in hitColliders)
    {
        if (col != characterCollider)
        {
            isGrounded = true;
            break;
        }
    }
    
    Debug.Log("isGrounded: " + isGrounded);
}

}