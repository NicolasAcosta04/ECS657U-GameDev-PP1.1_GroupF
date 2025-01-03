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
        if (chasing) print("Lost Player");

        Vector3 distanceToDestination = transform.position - destination;

        //Destination reached
        if (distanceToDestination.magnitude < 1f)
        {
            destinationSet = false;
            chasing = false;
        }
    }

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

    private void Chasing()
    {
        print("Chasing");
        destination = player.position;
        destinationSet = true;
        chasing = true;
        agent.SetDestination(destination);
        agent.speed = 5;
        transform.LookAt(player);
        
    }

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
        if (enemyType.Equals(EnemyTypes.Freezers) && seen)
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
