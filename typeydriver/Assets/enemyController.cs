using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class enemyController : MonoBehaviour
{
    public Transform player;
    public Animator anim;
    [SerializeField] public TextMeshPro textMesh;

    public bool attacking = false;
    public bool chasing = false;

    public float moveSpeed = 3f;
    public float stoppingDistance = 2f;
    public float attackRange = 1f;
    public float rotationSpeed = 5f; // controls how fast enemy turns

    private static readonly char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    void Start()
    {
        AssignRandomLetter();
    }


    void Update()
    {
        // calculate distance to player
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < stoppingDistance)
        {
            // look at player
            Vector3 direction = (player.position - transform.position);
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }


            if (distance <= attackRange)
            {
                Debug.Log(attacking);
                attacking = true;
                chasing = false;
                anim.SetBool("Attacking", attacking);
                anim.SetBool("Chasing", chasing);
            }
            else
            {
                Debug.Log("moving");
                attacking = false;
                chasing = true;
                anim.SetBool("Attacking", attacking);
                anim.SetBool("Chasing", chasing);
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            }
        }
            else
        {
            Debug.Log("where dey at doe");
            attacking = false;
            chasing = false;
            anim.SetBool("Attacking", attacking);
            anim.SetBool("Chasing", chasing);
        }
    }

        void AssignRandomLetter()
    {
        int index = Random.Range(0, letters.Length);
        textMesh.text = letters[index].ToString();
    }
}
