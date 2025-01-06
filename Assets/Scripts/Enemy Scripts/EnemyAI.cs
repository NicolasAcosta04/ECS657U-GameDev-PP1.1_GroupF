using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

/// adapted from https://youtu.be/UjkSFoLxesw
public class EnemyAI : MonoBehaviour
{
    // Declare fields
    private NavMeshAgent agent;
    private Rigidbody rb;
    private GameObject playerObject;
    private Transform player;
    private LayerMask whatIsGround, whatIsPlayer, Wall;
    private SoundFXManager soundFXManager;

    // Enemy Type
    public EnemyTypes enemyType;

    // Freezer Attributes
    [Header("Freezer Attributes")]
    private InPlayerCamera InPlayerCamera;
    private bool seen;
    private bool chasing;
    private bool freeze;

    //Patrolling
    [Header("Patrol Attributes")]
    [SerializeField] private Vector3 destination;
    [SerializeField] private float patrolRange;
    [SerializeField] private float waitTime = 2.0f;
    private bool destinationSet;
    private Vector3 startPosition;
    private bool lostPlayer = false;
    private bool waiting = false;


    //Attacking
    [Header("Attack attributes")]
    [SerializeField] private float timeBetweenAttacks;
    [SerializeField] private float attackDamage;
    [SerializeField] private float damageDuration;
    private bool attacking = false;
    private DamageEffect damageScript;
    private bool alreadyAttacked;

    //State Attributes
    [Header("State Attributes")]
    [SerializeField] private float sightRange;
    [SerializeField] private float attackRange = 0.2f;
    [SerializeField] private float FOVAngle;
    private bool playerInSightRange, playerInAttackRange;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] detectionSFX;
    private AudioSource currentDetection;


    //Enemy Type Specific


    // Initialisation
    private void Awake()
    {
        playerObject = GameObject.FindWithTag("Player");
        player = playerObject.transform;
        agent = GetComponent<NavMeshAgent>();
        rb = agent.GetComponent<Rigidbody>();
        InPlayerCamera = GetComponent<InPlayerCamera>();
        damageScript = GetComponent<DamageEffect>();
        whatIsGround = LayerMask.GetMask("whatIsGround");
        whatIsPlayer = LayerMask.GetMask("whatIsPlayer");
        Wall = LayerMask.GetMask("Wall");
        soundFXManager = FindAnyObjectByType<SoundFXManager>();
        startPosition = transform.position;
        chasing = false;
    }

    private void Start()
    {
        ChangeType(enemyType);
        StartCoroutine(FOVRoutine());
    }

    public void ChangeType(EnemyTypes type)
    {
        switch (type)
        {
            case EnemyTypes.Chasers:
                enemyType = EnemyTypes.Chasers;
                agent.speed = 1;
                attackDamage = 3;
                timeBetweenAttacks = 2;
                damageDuration = 0;
                break;

            case EnemyTypes.Freezers:
                enemyType = EnemyTypes.Freezers;
                agent.speed = 3;
                sightRange = 80;
                FOVAngle = 270;
                attackDamage = 15;
                timeBetweenAttacks = 3;
                damageDuration = 5;
                break;

            default:
                enemyType = EnemyTypes.Chasers;
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
            //print("Patrolling");
            while (!destinationSet) SearchDestination();

            if (destinationSet)
            {
                agent.SetDestination(destination);
            }
        }
        else
        {
            //print("Lost Player");
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

    //searches for a random destination on the map that is within the patrol range of the player
    private void SearchDestination()
    {
        //print("Searching for Destination");
        float randomX = Random.Range(player.position.x + patrolRange, player.position.x - patrolRange);
        float randomZ = Random.Range(player.position.z + patrolRange, player.position.z - patrolRange);
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
        if (!chasing)
        {
            if (currentDetection == null)
            {
                currentDetection = soundFXManager.PlayRandomSoundFXClip(detectionSFX, transform, 0.8f);
            }
            
        }
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
        attacking = true;
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            damageScript.damageAmount = attackDamage;
            damageScript.damageDuration = damageDuration;
            damageScript.ApplyEffect(playerObject);
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
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    // Update is called once per frame
    void Update()
    {
        //freezes the freezer enemy in place if its seen
        if ((enemyType.Equals(EnemyTypes.Freezers) && seen) || waiting || attacking)
        {
            //print("Freezing");
            if (!freeze)
            {
                transform.LookAt(player);
            }
            freeze = true;
            agent.speed = 0;
            agent.angularSpeed = 0;
            agent.acceleration = 0;
            agent.velocity = new Vector3(0, 0, 0);
            agent.isStopped = true;
            rb.drag = 9999;
            rb.angularDrag = 9999;
            //print("should be 0 ->" + agent.velocity);
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
        attacking = false;
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