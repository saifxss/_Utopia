using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;

    public Animator _anim;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    public float health;

    Vector3 _playerLastPosition;
    private bool chasing = false;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        _anim = transform.GetChild(0).GetComponent<Animator>();
    }

    private void Update()
    {

        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.GetChild(2).position, -(transform.GetChild(2).position - player.position), out hit) && (hit.transform.tag != "Wall"))
            {
                //is in direct sight

                if (!playerInAttackRange)
                {
                    chasing = true;
                    _playerLastPosition = player.position;
                }
                else
                {
                    AttackPlayer();
                    //return;
                }
            }
        }

        if (chasing)
        {
            //not in direct sight but was previously seeing the player
            ChasePlayer();
        }
        else
        {
            //not in direct sight and lost the player
            Patroling();
        }
    }

    private void Patroling()
    {
        Debug.Log("Patroling");
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        _anim.SetTrigger("Move");
        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        if (Vector3.Distance(transform.position, _playerLastPosition) <= attackRange)
            chasing = false;
        else
            agent.SetDestination(_playerLastPosition);
    }

    private void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);
        chasing = false;
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            _anim.SetTrigger("Attack");
            ///Attack code here
            Rigidbody rb = Instantiate(projectile, transform.GetChild(3).position, Quaternion.identity).GetComponent<Rigidbody>();
            Destroy(rb.gameObject, 3);
            rb.AddForce(-(transform.GetChild(3).position - player.position) * 32f, ForceMode.Impulse);
            //rb.AddForce(-(transform.position-player.position) * 8f, ForceMode.Impulse);
            ///End of attack code

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }
    private void DestroyEnemy()
    {
        _anim.SetTrigger("Dead");
        //Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}

