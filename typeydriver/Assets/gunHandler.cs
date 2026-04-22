using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunHandler : MonoBehaviour
{
    
    public followTarget followTargetScript;
    public Transform gunTip;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (followTargetScript == null || followTargetScript.target != null)
        {
            return;
        }
        else if (followTargetScript == null || followTargetScript.target == null)
        {
            // Third person over-the-shoulder camera
            if (Input.GetMouseButtonDown(0)){
                Debug.Log("pew");
            
                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f)) {
                if (hit.rigidbody != null) {
                    Debug.Log("Hit: " + hit.transform.name);
                    hit.rigidbody.AddForce(Vector3.up * 1000f);      
                }
            }
            
            }
        }
    }
}
