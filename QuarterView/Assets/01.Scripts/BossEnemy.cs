using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BossEnemy : Enemy
{
    private Vector3 lookVec; //플레이어 위치
    private Vector3 jumpAttackVec; //예측한 위치

    private bool isLook = true;
    private bool isRunning;

    void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();

        StartCoroutine(Think());
    }

    void Update()
    {
        if(isLook)
        {
            //플레이어를 보고있다면 플레이어의 뒷쪽을 예측
            float hor = Input.GetAxisRaw("Horizontal");
            float ver = Input.GetAxisRaw("Vertical");

            lookVec = new Vector3(hor, 0, ver) * 1f;
            transform.LookAt(target.transform.position + lookVec);
        }
        else
        {
            agent.SetDestination(jumpAttackVec);
        }
    }

    private IEnumerator Think()
    {
        yield return new WaitForSeconds(0.2f);

        int randomAction = Random.Range(0, 5);

        switch (randomAction)
        {
            case 0:
                StartCoroutine(BossRunningAttack());
                break;
            case 1:
                StartCoroutine(BossFarAttack());
                break;
            case 2:
                StartCoroutine(BossJumpAttack());
                break;
            case 3:
                StartCoroutine(BossFarAttack());
                break;
            case 4:
                StartCoroutine(BossJumpAttack());
                break;
        }
    }

    private IEnumerator BossFarAttack()
    {
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.98f);
        Instantiate(enemyBullet, bulletPos.position, bulletPos.rotation);
        
        StartCoroutine(Think());
    }

    private IEnumerator BossJumpAttack()
    {
        anim.SetTrigger("doJump");
        jumpAttackVec = target.transform.position + lookVec;
        isLook = false;
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.enabled = false;
        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;
        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;
        isLook = true;
        capsuleCollider.enabled = true;
        StartCoroutine(Think());
    }

    private IEnumerator BossRunningAttack()
    {
        isRunning = true;

        if(isRunning)
        {
            anim.SetBool("isRunning", true);
            yield return new WaitForSeconds(5.5f);
            isRunning = false;
        }


        anim.SetBool("isRunning", false);
        anim.SetTrigger("doRunning");
        //yield return new WaitForSeconds(5.5f);
        StartCoroutine(Think());
    }
}
