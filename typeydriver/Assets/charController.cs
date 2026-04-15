using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class charController : MonoBehaviour
{

public float speed;
public followTarget followTargetScript;
public Rigidbody rb;
public Camera playerCamera;

void Start ()
{
    rb = GetComponent<Rigidbody> ();
    if (playerCamera == null)
    {
        playerCamera = Camera.main;
    }
}

void FixedUpdate ()
{
    if (followTargetScript == null || followTargetScript.target != null) return; // Don't control char if mounted
    Debug.Log("Freedom of Movement");
    Move();
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
    
    rb.transform.position += moveVec * speed;
}
}