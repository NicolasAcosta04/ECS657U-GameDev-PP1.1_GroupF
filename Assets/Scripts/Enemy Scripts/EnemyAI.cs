using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

/// adapted from https://youtu.be/UjkSFoLxesw
public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    public NavMeshAgent agent;

    public Rigidbody rb;

    public Enum enemyType;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer, Wall;

    private InPlayerCamera InPlayerCamera;

    public Boolean seen;

    public Boolean chasing;

    MeshRenderer renderer;

    //Patrolling
    public Vector3 destination;
    bool destinationSet;
    public float patrolRange;
    private Vector3 startPosition;
    public Boolean lostPlayer = false;
    public Boolean waiting = false;
    public float waitTime = 2.0f;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange, FOVAngle;
    public bool playerInSightRange, playerInAttackRange;

    //Enemy Type Specific
    public bool freeze;

    // Initialisation
    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        rb = agent.GetComponent<Rigidbody>();
        renderer = GetComponent<MeshRenderer>();
        startPosition = transform.position;
        InPlayerCamera = GetComponent<InPlayerCamera>();
        chasing = false;
    }

    private void Start()
    {
        ChangeType(EnemyTypes.Freezers);
        StartCoroutine(FOVRoutine());
    }

    public void ChangeType(Enum type)
    {
        switch (type)
        {
            case EnemyTypes.Chasers:
                enemyType = EnemyTypes.Chasers;
                agent.speed = 2;
                break;

            case EnemyTypes.Freezers:
                enemyType = EnemyTypes.Freezers;
                agent.speed = 3;
                sightRange = 80;
                FOVAngle = 270;
                break;

            default:
                enemyType=EnemyTypes.Chasers;
                agent.speed = 2;
                break;
        }
    }

    //chooses a destination and moves to that location before picking another one
    //if moving to this state from chase (meaning the player has been lost)
    //move to the last assigned destination and wait before picking a new one
    private void Patrolling()
    {
        if (!chasing)
        {
            print("Patrolling");
            while (!destinationSet) SearchDestination();

            if (destinationSet)
            {
                agent.SetDestination(destination);
            }
        }
        else
        {
            print("Lost Player");
            lostPlayer = true;
        }

        Vector3 distanceToDestination = transform.position - destination;

        //Destination reached
        if (distanceToDestination.magnitude < 1f)
        {
            //if enemy reaches last known location of player, wait for a period then start to patrol again
            if (lostPlayer)
            {
                lostPlayer = false;
                waiting = true;
                Invoke(nameof(Waiting), waitTime);
            }
            destinationSet = false;
            chasing = false;
        }
    }

    private void Waiting()
    {
        waiting = false;
    }

    //searches for a random destination on the map that is within the patrol range
    private void SearchDestination()
    {
        print("Searching for Destination");
        float randomX = Random.Range(startPosition.x + patrolRange, startPosition.x - patrolRange);
        float randomZ = Random.Range(startPosition.z + patrolRange, startPosition.z - patrolRange);
        destination = new Vector3(randomX, transform.position.y, randomZ);
        
        if (Physics.Raycast(destination, -transform.up, 2f, whatIsGround) && !(Physics.Raycast(destination, -transform.up, 2f, Wall)))
        {
            destinationSet = true;
        } 
        else
        {
            destinationSet = false;
        }
    }

    //chasing state
    private void Chasing()
    {
        print("Chasing");
        destination = player.position;
        destinationSet = true;
        chasing = true;
        agent.SetDestination(destination);
        if (enemyType.Equals(EnemyTypes.Freezers))
        {
            agent.speed = 6;
        }
        
        transform.LookAt(player);
        
    }

    //attacking state
    private void Attacking()
    {
        print("Attacking");
        agent.SetDestination(transform.position);
        agent.speed = 0;

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

    //Checks if player is in sight range and returns false if a wall is in the way
    private void FOVCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);

        if (rangeChecks.Length > 0)
        {
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

    //displays sight and attack range for debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere (transform.position, sightRange);
    }

    // Update is called once per frame
    void Update()
    {
        //freezes the freezer enemy in place if its seen
        if ((enemyType.Equals(EnemyTypes.Freezers) && seen) || waiting)
        {
            print("Freezing");
            if (!freeze)
            {
                transform.LookAt(player);
            }
            freeze = true;
            agent.speed = 0;
            agent.angularSpeed = 0;
            agent.acceleration = 0;
            agent.velocity = new Vector3(0,0,0);
            agent.isStopped = true;
            rb.drag = 9999;
            rb.angularDrag = 9999;
            print("should be 0 ->" + agent.velocity);
        }
        //resumes movement if not
        else
        {
            freeze = false;
            agent.speed = 3;
            agent.angularSpeed = 120;
            agent.acceleration = 2;
            agent.isStopped = false;
            rb.drag = 0;
            rb.angularDrag = 0.5f;

        }
        //Check for sight and attack range
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        //Sets General Enemy States
        if (!freeze)
        {
            if (!playerInSightRange && !playerInAttackRange) Patrolling();
            if (playerInSightRange && !playerInAttackRange) Chasing();
        }
        if (playerInSightRange && playerInAttackRange) Attacking();


        //Updates variable to match InPlayerCamera
        seen = InPlayerCamera.inCamera;
    }
}

public enum EnemyTypes
{
    Chasers,
    Freezers
}
