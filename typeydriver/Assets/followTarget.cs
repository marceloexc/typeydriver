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

    public float interactDistance = 5f; // NEW
    public GameObject interactIcon;     // NEW (assign in inspector)

    void Start()
    {
        temptarget = target;

        if (interactIcon != null)
        {
            interactIcon.SetActive(false); // hide at start
        }
    }

    void Update()
    {
        bool active = target != null;

        // Follow car if mounted
        if (active)
        {
            player.transform.position = target.position;
            player.transform.rotation = target.rotation;
        }

        // Distance check (only matters when NOT in car)
        bool inRange = false;

        if (!active && temptarget != null)
        {
            float distance = Vector3.Distance(player.transform.position, temptarget.position);
            inRange = distance <= interactDistance;

            // Show/hide UI icon
            if (interactIcon != null)
            {
                interactIcon.SetActive(inRange);
            }
        }
        else if (interactIcon != null)
        {
            interactIcon.SetActive(false);
        }

        Debug.Log("inputty" + InputLock.IsTyping);
        if (InputLock.IsTyping)
        return;

        // EXIT CAR (no distance restriction needed)
        if (Input.GetKeyDown(KeyCode.E) && active)
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
        // ENTER CAR (only if in range)
        else if (Input.GetKeyDown(KeyCode.E) && !active && inRange)
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
