using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followTarget : MonoBehaviour
{
    public Transform target;
    private Transform temptarget;
    public GameObject tpsUI;
    public GameObject repairSystem;

    void Start()
    {
        if (target == null)
        {

        }

        temptarget = target;

        // positionOffset = transform.position - target.position; 
    }

    void Update()
    {
        bool active = target != null;

        if (active)
        {
        transform.position = target.position;
        transform.rotation = target.rotation;
        }

        if (Input.GetKeyDown(KeyCode.Period) && active)
        {
            target = null;
            Debug.Log("dismount");
            tpsUI.SetActive(true);
            repairSystem.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Period) && !active)
        {
            target = temptarget;
            Debug.Log("giddyup");
            tpsUI.SetActive(false);
        }
    }

}
