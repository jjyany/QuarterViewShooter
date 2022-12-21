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
        isPatrol,       //����
        isChase,        //�߰�
        isHit,          //����
        isAttack,       //����
        isDie           //���
    };

    public State state = 0;

    public Transform target;
    public Transform eyePosition;
    public ParticleSystem bloodEffect;

    public int maxHealth;
    public int curHealth;

    public int bulletForce = 5;
    public int grenadeForce = 9;

    public int attackDamage = 10;
    public int grenadeDamage = 50;

    public float walkSpeed = 0.1f;
    public float runSpeed = 0.2f;
    public float attackDistance;
    public float attackRadius = 0.5f;

    public bool isDead = false;

    public SphereCollider meleeArea;
    public Player player;

    private Rigidbody rigid;
    private NavMeshAgent agent;
    private Animator anim;

    private bool hasTarget => player != null && !player.isDead;



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0, 0, 0.5f);
        Gizmos.DrawSphere(eyePosition.position, 2f);
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        meleeArea = GetComponentInChildren<SphereCollider>();

        var attackPivot = meleeArea.transform.position;
        attackPivot.y = transform.position.y;

        attackDistance = Vector3.Distance(transform.position, attackPivot) + attackRadius;
        agent.stoppingDistance = attackDistance;
        agent.speed = walkSpeed;
    }

    private void Start()
    {
        //StartCoroutine(UpdatePatrol());
        state = State.isChase;
    }

    private void Update()
    {
        if(state == State.isDie)
        {
            return;
        }

        if(state == State.isHit)
        {
            agent.speed = walkSpeed * Time.deltaTime;
            //agent.isStopped = true;
        }

        anim.SetFloat("Speed", agent.desiredVelocity.magnitude);

        if (state == State.isChase)
        {
            if (state != State.isDie)
            {
                //agent.speed = walkSpeed;
                agent.speed = runSpeed;
                agent.SetDestination(target.position);
            }
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

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, targetRedius,
                                transform.forward, targetRange, LayerMask.GetMask("Player"));

        var playerliving = GetComponent<Player>();

        if(hits.Length > 0 && playerliving.tag == "Player" && !(state == State.isAttack))
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

        agent.SetDestination(target.position);
        yield return new WaitForSeconds(3.0f);
    }



    private void FreezeVelcity()
    {
        if (state == State.isChase || state == State.isPatrol || state == State.isHit)
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

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;

            Vector3 reactVec = transform.position - other.transform.position;

            OnDamage(reactVec, false);

        }
        else if(other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();

            curHealth -= bullet.damage;

            Vector3 reactVec = other.transform.position - transform.position;

            OnDamage(reactVec, false);
        }

        else if (other.tag == "Punching")
        {
            Weapon weapon = other.GetComponent<Weapon>();

            curHealth -= weapon.damage;

            Vector3 reactVec = transform.position - other.transform.position;

            OnDamage(reactVec, false);
        }
    }

    private void OnDamage(Vector3 _reactVec , bool _isGrenade)
    {
        if (curHealth > 0)
        {
            state = State.isHit;
            anim.SetTrigger("isHit");
            StartCoroutine(OnDamageEffect());
            _reactVec = _reactVec.normalized;

            if(_isGrenade)
            {
                _reactVec = _reactVec.normalized;
                _reactVec += Vector3.up * grenadeDamage;

                rigid.freezeRotation = false;
                rigid.AddForce(_reactVec * grenadeForce, ForceMode.Impulse);
                rigid.AddTorque(_reactVec * grenadeForce, ForceMode.Impulse);
            }
            else
            {
                rigid.AddForce(_reactVec * bulletForce, ForceMode.Impulse);
            }
        }
        else if(curHealth <= 0)
        {
            state = State.isDie;
            agent.enabled = false;
            gameObject.layer = 15;
            anim.SetTrigger("doDie");
            Destroy(gameObject, 10.0f);
        }

        //state = State.isChase;
    }

    private IEnumerator OnDamageEffect()
    {
        bloodEffect.Play();
        yield return new WaitForSeconds(0.2f);
        state = State.isChase;
    }

}