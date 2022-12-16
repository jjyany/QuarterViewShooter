using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SearchService;

public class Enemy : MonoBehaviour
{
    public enum State
    {
        isPatrol,
        isChase,
        isAttack,
        isDie
    };

    public State state = 0;

    public Transform target;

    public int maxHealth;
    public int curHealth;

    public int grenadeDamage = 50;
    public int bulletForce = 5;
    public int grenadeForce = 9;

    public float walkSpeed = 2.0f;
    public float runSpeed = 2.5f;
    public float attackDistance;
    public float attackRadius = 1f;

    public SphereCollider meleeArea;

    private SkinnedMeshRenderer mat;
    private Rigidbody rigid;
    private NavMeshAgent agent;
    private Animator anim;


    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        mat = GetComponentInChildren<SkinnedMeshRenderer>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        meleeArea = GetComponentInChildren<SphereCollider>();

        var attackPivot = meleeArea.transform.position;
        attackPivot.y = transform.position.y;

        attackDistance = Vector3.Distance(transform.position, attackPivot) + attackRadius;
        agent.stoppingDistance = attackDistance;
        agent.speed = runSpeed;

        Invoke("ChaseStart", 2);
    }

    private void Start()
    {
        StartCoroutine(UpdatePatrol());
    }

    private void Update()
    {

        if(state == State.isPatrol)
        {

        }
    }

    private void FixedUpdate()
    {
        FreezeVelcity();
    }

    private void Targeting()
    {
        float targetRedius = 1f;
        float targetRange = 2f;

        RaycastHit[] hit = Physics.SphereCastAll(transform.position, targetRedius,
                                transform.forward, targetRange, LayerMask.GetMask("Player"));

        var playerliving = GetComponent<Player>();

        if(hit.Length > 0 && playerliving.tag == "Player" && !(state == State.isAttack))
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        state = State.isAttack;

        if (state == State.isAttack)
        {
            anim.SetTrigger("isAttack");

            yield return new WaitForSeconds(0.28f);
            meleeArea.enabled = true;
            yield return new WaitForSeconds(0.5f);
            meleeArea.enabled = false;
            yield return new WaitForSeconds(0.8f);

            state = State.isChase;
        }
    }

    private IEnumerator UpdatePatrol()
    {
        while(!(state == State.isDie))
        {
            if(target != null)
            {
                target = null;
            }

            if(state != State.isPatrol)
            {
                state = State.isPatrol;
                agent.speed = walkSpeed;
            }

            if(agent.remainingDistance <= 1f)
            {
                var partolTargetPosition = RandomPoint.GetRandomPointOnNavMesh(transform.position, 5f, NavMesh.AllAreas);
                agent.SetDestination(partolTargetPosition);
            }

            Targeting();
            break;
        }
        yield return new WaitForSeconds(1.0f);
    }

    private void ChaseStart()
    {
        if (state == State.isChase)
        {
            agent.speed = runSpeed;
            anim.SetFloat("Speed", agent.desiredVelocity.magnitude);
        }
    }

    private void FreezeVelcity()
    {
        if(state == State.isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    public void HitByGrenade(Vector3 _explosionPos)
    {
        curHealth -= grenadeDamage;

        Vector3 reactVec = transform.position - _explosionPos;
        OnDamage(reactVec, true);
        StartCoroutine(OnDamageColor());

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;

            Vector3 reactVec = transform.position - other.transform.position;

            OnDamage(reactVec, false);
            StartCoroutine(OnDamageColor());

        }
        else if(other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();

            curHealth -= bullet.damage;

            Vector3 reactVec = other.transform.position - transform.position;

            OnDamage(reactVec, false);
            StartCoroutine(OnDamageColor());
        }

        else if (other.tag == "Punching")
        {
            Weapon weapon = other.GetComponent<Weapon>();

            curHealth -= weapon.damage;

            Vector3 reactVec = transform.position - other.transform.position;

            OnDamage(reactVec, false);
            StartCoroutine(OnDamageColor());
        }
    }

    private void OnDamage(Vector3 _reactVec , bool _isGrenade)
    {
        if(curHealth > 0)
        {
            mat.material.color = Color.white;
            _reactVec = _reactVec.normalized;

            if(_isGrenade)
            {
                rigid.freezeRotation = false;
                rigid.AddForce(_reactVec * grenadeForce, ForceMode.Impulse);
                rigid.AddTorque(_reactVec * grenadeForce, ForceMode.Impulse);
            }
            else
            {
                rigid.AddForce(_reactVec * bulletForce, ForceMode.Impulse);
            }
        }
        else
        {
            gameObject.layer = 15;
            anim.SetTrigger("doDie");
            state = State.isDie;
            agent.enabled = false;
            Destroy(gameObject, 3.0f);
        }
    }

    private IEnumerator OnDamageColor()
    {
        mat.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        mat.material.color = Color.white;

        if (curHealth == 0)
        {
            mat.material.color = Color.gray;
        }
    }

}
