using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followTarget : MonoBehaviour
{
    public Transform target;
    private Transform temptarget;
    public GameObject tpsUI;
    public GameObject repairSystem;
    public GameObject player;

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
        player.transform.position = target.position;
        player.transform.rotation = target.rotation;
        }

        if (Input.GetKeyDown(KeyCode.Period) && active)
        {
            target = null;
            Debug.Log("dismount");
            tpsUI.SetActive(true);
            repairSystem.SetActive(false);
            player.SetActive(true);
            TooltipManager.Instance.ShowTooltip(
            "exit_car",
            "Obtaining Letters",
            "Use RMB to blast bots, who'll drop the letter on their screen.",
            5f
            );
        }
        else if (Input.GetKeyDown(KeyCode.Period) && !active)
        {
            target = temptarget;
            Debug.Log("giddyup");
            tpsUI.SetActive(false);
            player.SetActive(false);
            TooltipManager.Instance.ShowTooltip(
            "reenter_car",
            "Using Letters",
            "Open the fixme system using TAB, then type in the name of the damaged part.",
            5f
            );
        }
    }

}
