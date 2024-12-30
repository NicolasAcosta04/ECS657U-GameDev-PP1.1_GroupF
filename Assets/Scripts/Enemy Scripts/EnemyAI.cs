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

    public LayerMask whatIsGround, whatIsPlayer, Wall;

    //Patrolling
    public Vector3 destination;
    bool destinationSet;
    public float patrolRange;
    private Vector3 startPosition;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange, FOVAngle;
    public bool playerInSightRange, playerInAttackRange;

    // Initialisation
    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
    }

    private void Start()
    {
        StartCoroutine(FOVRoutine());
    }

    private void Patrolling()
    {
        while (!destinationSet) SearchDestination();

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
        float randomX = Random.Range(startPosition.x + patrolRange, startPosition.x - patrolRange);
        float randomZ = Random.Range(startPosition.z + patrolRange, startPosition.z - patrolRange);
        destination = new Vector3(randomX, transform.position.y, randomZ);
        
        if (Physics.Raycast(destination, -transform.up, 2f, whatIsGround) && !(Physics.Raycast(destination, -transform.up, 2f, Wall)))
        {
            Debug.Log(destination);
            destinationSet = true;
        } 
        else
        {
            Debug.Log(destinationSet);
            destinationSet = false;
        }
    }

    private void Chasing()
    {
        transform.LookAt(player);
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

    //Helps performance slightly by checking sight range 5 times a second rather than every frame
    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FOVCheck();
        }
    }

    private void FOVCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);

        if (rangeChecks.Length > 0)
        {
            Debug.Log(rangeChecks);
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < FOVAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, Wall))
                {
                    playerInSightRange = true;
                }
                else
                {
                    playerInSightRange = false;
                }
            }
            else
            {
                playerInSightRange = false;
            }
        }
        else if (playerInSightRange)
        {
            playerInSightRange = false;
        }
    }

    //displays sight and attack range
    /* private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere (transform.position, sightRange);
    } */

    // Update is called once per frame
    void Update()
    {
        //Check for sight and attack range
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) Chasing();
        if (playerInSightRange && playerInAttackRange) Attacking();
    }
}
