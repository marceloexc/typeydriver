using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthHandler : MonoBehaviour
{
    public float hitPoints;
    public bool isPlayer;

    private GameObject parentObj;
    private bool isDead = false;

    void Start()
    {
        parentObj = transform.parent != null ? transform.parent.gameObject : gameObject;
    }

    void Update()
    {
        if (!isDead && hitPoints <= 0)
        {
            isDead = true;
            Destroy(parentObj, 1f);
        }
    }
}
