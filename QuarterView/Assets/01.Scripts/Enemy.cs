using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SearchService;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    public enum State
    {
        isPatrol,       //????
        isChase,        //?߰?
        isHit,          //????
        isAttack,       //????
        isDie           //????
    };

    public enum Type
    {
        meleeAttack,
        runAttack,
        farAttack
    }

    public State state = 0;
    public Type type = 0;

    public GameObject target;
    public ParticleSystem bloodEffect;

    public int maxHealth;
    public int curHealth;

    public int bulletForce = 5;
    public int grenadeForce = 9;

    public int grenadeDamage = 50;

    public float walkSpeed;
    public float runSpeed;
    public float attackDistance;
    public float attackRadius = 0.1f;
    public float hitTime = 0.55f;

    public float turnSmoothVelocity;
    [Range(0.1f, 2.0f)]
    public float turnSmoothSpeed;
    [Range(0, 2.0f)]
    public float attackStay;



    public bool isDead = false;

    public SphereCollider meleeArea;
    public GameObject enemyBullet;
    public Transform bulletPos;

    private Rigidbody rigid;
    private NavMeshAgent agent;
    private Animator anim;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0, 0, 0.5f);
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        meleeArea = GetComponentInChildren<SphereCollider>();

        //var attackPivot = meleeArea.transform.position;
        //attackPivot.y = transform.position.y;

        //attackDistance = Vector3.Distance(transform.position, attackPivot) + attackRadius;
        agent.stoppingDistance = attackRadius;
        agent.speed = walkSpeed;
    }

    private void Start()
    {
        StartCoroutine(UpdatePatrol());
        state = State.isChase;
    }

    private void Update()
    {
        if(isDead || state == State.isHit)
        {
            return;
        }

        if (state == State.isChase)
        {
            if (target != null)
            {
                anim.SetFloat("Speed", agent.desiredVelocity.magnitude);

                if (agent.remainingDistance <= 0.1f)
                {
                    var lookRotation = Quaternion.LookRotation(target.transform.position - transform.position);

                    var turnAngleY = lookRotation.eulerAngles.y;

                    turnAngleY = Mathf.SmoothDampAngle(transform.eulerAngles.y, turnAngleY, ref turnSmoothVelocity, turnSmoothSpeed);
                    transform.eulerAngles = Vector3.up * turnAngleY;
                }


                if (state != State.isDie)
                {
                    switch (type)
                    {
                        case Type.meleeAttack:
                            agent.speed = walkSpeed;
                            break;
                        case Type.runAttack:
                            agent.speed = runSpeed;
                            break;
                        case Type.farAttack:
                            agent.speed = walkSpeed;
                            break;
                    }
                    agent.SetDestination(target.transform.position);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        FreezeVelcity();

        StartCoroutine(Targeting());
    }

    private IEnumerator Targeting()
    {
        if (!isDead)
        {
            float targetRedius = 0;
            float targetRange = 0;

            switch(type)
            {
                case Type.meleeAttack:
                    targetRedius = 0.3f;
                    targetRange = 0.6f;
                    break;
                case Type.runAttack:
                    targetRedius = 0.25f;
                    targetRange = 0.6f;
                    break;
                case Type.farAttack:
                    targetRedius = 0.5f;
                    targetRange = 15f;
                    break;
            }

            RaycastHit[] hits = Physics.SphereCastAll(transform.position, targetRedius,
                                    transform.forward, targetRange, LayerMask.GetMask("Player"));
            if (hits.Length > 0 && !(state == State.isAttack))
            {
                if(state == State.isHit)
                {
                    yield return new WaitForSeconds(1.0f);
                }
                else
                {
                    StartCoroutine(Attack());
                }
            }
        }
    }

    private IEnumerator Attack()
    {
        state = State.isAttack;

        if (state == State.isAttack)
        {
            switch(type)
            {
                case Type.meleeAttack:
                    if (isDead)
                    {
                        break;
                    }
                    anim.SetTrigger("isMelee");
                    agent.isStopped = true;
                    yield return new WaitForSeconds(0.98f);
                    meleeArea.enabled = true;
                    yield return new WaitForSeconds(0.8f);
                    meleeArea.enabled = false;
                    break;

                case Type.runAttack:
                    if (isDead)
                    {
                        break;
                    }
                    anim.SetTrigger("isAttack");
                    agent.isStopped = true;
                    yield return new WaitForSeconds(0.58f);
                    meleeArea.enabled = true;
                    yield return new WaitForSeconds(0.2f);
                    meleeArea.enabled = false;
                    break;

                case Type.farAttack:
                    if (isDead)
                    {
                        break;
                    }
                    yield return new WaitForSeconds(0.58f);
                    anim.SetTrigger("isAttack");
                    agent.isStopped = true;
                    yield return new WaitForSeconds(1.0f);
                    GameObject instantBullet = Instantiate(enemyBullet, bulletPos.position, transform.rotation);
                    Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                    rigidBullet.velocity = transform.forward * 10f;

                    yield return new WaitForSeconds(1.5f);
                    break;
            }
        }

        if(!isDead)
        {
            state = State.isChase;
            agent.isStopped = false;
        }
    }

    private IEnumerator UpdatePatrol()
    {

        yield return new WaitForSeconds(2.0f);
        target = GameObject.FindWithTag("Player");
        agent.SetDestination(target.transform.position);
    }



    private void FreezeVelcity()
    {
        if (state == State.isChase || state == State.isPatrol)
        {
            rigid.angularVelocity = Vector3.zero;
        }

        if(state == State.isHit || state == State.isAttack || state == State.isDie)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    public void HitByGrenade(Vector3 _explosionPos)
    {
        curHealth -= grenadeDamage;

        Vector3 reactVec = transform.position - _explosionPos;
        StartCoroutine(OnDamage(reactVec, true));

    }

    private void OnTriggerEnter(Collider other)
    {
        if (state != State.isDie && other)
        {
            Vector3 reactVec = transform.position - other.transform.position;

            if (other.tag == "Melee")
            {
                Weapon weapon = other.GetComponent<Weapon>();
                curHealth -= weapon.damage;
                StartCoroutine(OnDamage(reactVec, false));
            }
            else if (other.tag == "Bullet")
            {
                Bullet bullet = other.GetComponent<Bullet>();
                curHealth -= bullet.damage;
                StartCoroutine(OnDamage(reactVec, false));
            }

            else if (other.tag == "Punching")
            {
                Weapon weapon = other.GetComponent<Weapon>();
                curHealth -= weapon.damage;
                StartCoroutine(OnDamage(reactVec, false));
            }
        }
    }

    private IEnumerator OnDamage(Vector3 _reactVec , bool _isGrenade)
    {
        if (curHealth > 0)
        {
            state = State.isHit;
            if (state == State.isHit)
            {
                Debug.Log("OnDamage Execution");
                agent.isStopped = true;
                anim.SetTrigger("isHit");
                bloodEffect.Play();
                _reactVec = _reactVec.normalized;
                agent.isStopped = false;
                yield return new WaitForSeconds(hitTime);
                state = State.isChase;
            }

            if (_isGrenade)
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
            isDead = true;
            agent.enabled = false;
            gameObject.layer = 15;
            anim.SetTrigger("doDie");
            Destroy(gameObject, 10.0f);
        }


    }
}
