using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// adapted from https://youtu.be/UjkSFoLxesw
public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    //Patrolling
    public Vector3 destination;
    bool destinationSet;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    // Initialisation
    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Patrolling()
    {
        if (!destinationSet) SearchDestination();

        if (destinationSet)
        {
            agent.SetDestination(destination);
        }

        Vector3 distanceToDestination = transform.position - destination;

        //Destination reached
        if (distanceToDestination.magnitude < 1f)
        {
            destinationSet = false;
        }
    }

    private void SearchDestination()
    {
        GameObject[] rooms = GameObject.FindGameObjectsWithTag("Room");
        Transform randomRoom = rooms[Random.Range(0,rooms.Length)].transform;
        destination = new Vector3(randomRoom.position.x, transform.position.y, randomRoom.position.z);
        
        if (Physics.Raycast(destination, -transform.up, 2f, whatIsGround))
        {
            destinationSet = true;
        }
    }

    private void Chasing()
    {
        agent.SetDestination(player.position);
    }

    private void Attacking()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            //Add attack here

            //
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) Chasing();
        if (playerInSightRange && playerInAttackRange) Attacking();
    }
}
