using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
public class BossAI1 : MonoBehaviour
{
    public NavMeshAgent agent;

    public Animator _anim;

    public Transform player;

    public List<Transform> players;

    public Transform currentplayer;

    public LayerMask whatIsGround, whatIsPlayer;

    public float health;

    public float Damage = 25f;

    Vector3 _playerLastPosition;
    private bool chasing = false;

    public float radius=3f;

    public float maxDistance=3f;

    public float cooldownTime;

    public float nextFireTime;

    public bool canCastPush;

    public enum type {ranged,melee};
    public type _type;
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
        //player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = GetComponent<NavMeshAgent>();
        //_anim = transform.GetChild(0).GetComponent<Animator>();
        _anim = transform.GetComponent<Animator>();
    }
    public void RemovePlayer(Transform x)
    {
        players.Remove(x);
    }

    private void Update()
    {
        
        RaycastHit[] rays = Physics.SphereCastAll(transform.position, radius, transform.forward, maxDistance);
        foreach (RaycastHit rh in rays) { if (rh.transform.tag == "Player" && !players.Contains(rh.transform)) players.Add(rh.transform); }
        if (players.Count>0)
        {
            players = players.OrderBy(x => Vector3.Distance(transform.position, x.position)).ToList();
            player = players[0];
            currentplayer = player;
            //Check for sight and attack range
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
            if(players.Count >= 2)
            {
                RaycastHit[] raysClose = Physics.SphereCastAll(transform.position, 8f, transform.forward);
                int close = 0;
                List<Transform> closePlayersCircle = new List<Transform>(0);
                foreach(RaycastHit rhC in raysClose)
                {
                    if(rhC.transform.tag == "Player")
                    {
                        //Debug.Log(rhC.transform.name + " close player");
                        closePlayersCircle.Add(rhC.transform);
                    }

                }
                cooldownTime += Time.deltaTime;
                if (cooldownTime>=8f)
                {
                    
                        foreach (Transform t in closePlayersCircle)
                        {
                        Debug.Log("majdi");
                        //push
                            Vector3 throwDirection = t.position - transform.position;
                            t.GetComponent<Rigidbody>().AddForce(throwDirection * 9500f * Time.deltaTime);
                        }
                    cooldownTime = 0;
                }
                
            }
        }





        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.GetChild(2).position, -(transform.GetChild(2).position - player.position), out hit) && (hit.transform.tag != "Wall"))
            {
                //is in direct sight

                if (!playerInAttackRange)
                {
                    _anim.SetTrigger("Move");
                    chasing = true;
                    _playerLastPosition = player.position;
                }
                else
                {
                    AttackPlayer();
                    return;
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
        transform.LookAt(new Vector3(player.position.x,transform.position.y, player.position.z));

        if (!alreadyAttacked)
        {
            _anim.SetTrigger("Attack");
        }
    }

    public void AttackAnimationEvent()
    {
        if (_type == type.ranged)
        {

            ///Attack code here
            Rigidbody rb = Instantiate(projectile, transform.GetChild(3).position, Quaternion.identity).GetComponent<Rigidbody>();
            Destroy(rb.gameObject, 3);
            rb.AddForce(-(transform.GetChild(3).position - player.position) * 32f, ForceMode.Impulse);
            //rb.AddForce(-(transform.position-player.position) * 8f, ForceMode.Impulse);
            ///End of attack code

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        else if (_type == type.melee)
        {
            player.GetComponent<HealthManager>().TakeDamage(Damage);
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

