using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyController : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 3f;
    public float stoppingDistance = 2f;
    public float attackRange = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        // Calculate distance to player
        float distance = Vector3.Distance(transform.position, player.position);

        // Move towards player if beyond stopping distance
        if (distance < stoppingDistance)
        {
                if (distance <= attackRange)
            {
                // attack logic will go here eventually ig
                Debug.Log("attacking");
            }
            else
            {
                Debug.Log("moving");
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
            }

        }
        else if (distance > stoppingDistance)
        {
           return; 
        }
    }
}
