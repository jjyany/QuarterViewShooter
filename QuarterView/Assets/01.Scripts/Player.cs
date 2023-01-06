using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public float runSpeed = 5f;
    public float dodgeSpeed = 10f;
    public float jump = 3f;
    public float turnSpeed = 2.0f;
    public float smoothTurnSpeed = 0.1f;
    public float moveDistance = 300f;
    public float punchiSpeed = 1.4f;
    public float grenadeSpeed = 0.3f;


    public GameObject[] weapons;
    public bool[] hasWeapons;
    public bool[] isWeapons;

    public GameObject[] grenades;
    public int hasgrenade;
    public int grenadePower;
    public int grenadeDistance = 300;
    public GameObject grenadeObject;
    public Transform grenadePos;

    public int ammo;
    public int health;

    public int maxAmmo;
    public int maxHealth;
    public int maxHasGrenade;

    private float hAxis;
    private float vAxis;

    public float currentSpeed;
    public int currentWeaponIndex = -1;
    private float fireDelay;    //다음공격대기시간

    private bool inputWalk;     //이동인풋
    private bool inputJump;     //점프인풋
    private bool inputDodge;    //회피인풋
    private bool inputFire;     //공격인풋
    private bool inputDownGrenade;  //수류탄인풋
    private bool inputReload;   //재장전인풋

    public bool inputSwapWeapon_1;
    public bool inputSwapWeapon_2;
    public bool inputSwapWeapon_3;

    public bool isJump;        //점프중
    public bool isDodge;       //회피중
    public bool isGetItem;     //아이템먹는중
    public bool isSwap = false;        //무기교체중
    public bool isFireReady = true;   //공격대기
    public bool isReload;
    public bool isBorder;      //Ground 충돌
    public bool isGrenade;
    public bool isDamage;
    public bool isHit;

    public bool isDead = false;

    private Animator anim;
    private Rigidbody rigid;
    private SkinnedMeshRenderer skinMeshRenderer;
    private MeshRenderer[] meshRenderers;

    private Vector3 moveVec;
    private Vector3 dodgeVec;
    private Camera followCam;

    private GameObject nearObject;
    public GameObject punch;
    public Weapon currentWeapon;

    public Camera followCamera;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        currentWeapon = GetComponentInChildren<Weapon>();
        skinMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        followCam = Camera.main;
    }

    private void Update()
    {
        GetInput();
        Move();
        Rotate();
        Jump();
        Attack();
        Reload();
        Grenade();
        Dodge();
        Swap();
        Interation();
        UpdateUI();
        Dead();
    }

    private void FixedUpdate()
    {
        FreezeRtation();
        StopToWall();
    }

    private void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        inputWalk = Input.GetButton("Walk");
        inputJump = Input.GetButtonDown("Jump");
        inputDodge = Input.GetButtonDown("Dodge");
        inputFire = Input.GetButton("Fire1");
        inputDownGrenade = Input.GetButtonDown("Fire2");
        inputReload = Input.GetButtonDown("Reload");
        isGetItem = Input.GetButtonDown("Interation");
        inputSwapWeapon_1 = Input.GetButtonDown("Swap1");
        inputSwapWeapon_2 = Input.GetButtonDown("Swap2");
        inputSwapWeapon_3 = Input.GetButtonDown("Swap3");
    }

    private void Move()
    {
        if(isDead)
        {
            return;
        }

        currentSpeed = runSpeed;
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if(isDodge)
        {
            moveVec = dodgeVec;
        }

        if(!isFireReady || isSwap || isReload || isGrenade || inputDownGrenade || inputFire && (!isJump && !isDodge))
        {
            moveVec = Vector3.zero;
        }

        if(!isBorder)
        {
            transform.position += moveVec * currentSpeed * (inputWalk ? 0.3f : 1f) * Time.deltaTime;
        }

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", inputWalk);
    }

    private void Rotate()
    {
        var playerRotate = followCam.transform.eulerAngles.y;

        playerRotate = Mathf.SmoothDampAngle(transform.eulerAngles.y, playerRotate, ref smoothTurnSpeed, smoothTurnSpeed, turnSpeed);

        transform.eulerAngles = Vector3.up * playerRotate;

        //키보드 회전
        transform.LookAt(transform.position + moveVec);

        if (!isDodge && !isJump && !isReload)
        {
            //마우스 회전
            if (inputFire)
            {
                if(isJump || isDodge)
                {
                    return;
                }

                Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, moveDistance))
                {
                    Vector3 nextVec = hit.point - transform.position;
                    nextVec.y = 0;
                    transform.LookAt(transform.position + nextVec);
                }
            }
        }
    }

    private void FreezeRtation()
    {
        rigid.angularVelocity = Vector3.zero;
    }

    private void StopToWall()
    {
        isBorder = Physics.Raycast(transform.position + (Vector3.up), transform.forward, 0.5f, LayerMask.GetMask("Ground"));
    }

    private void Jump()
    {
        if(inputJump && !isJump && !isDodge && !isSwap && !isDamage && !isReload)
        {
            rigid.AddForce(Vector3.up * jump, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    private void Grenade()
    {
        if(hasgrenade == 0)
        {
            return;
        }

        if(inputDownGrenade && !isReload && !isSwap && !isDamage)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, grenadeDistance))
            {
                if(hit.collider.tag == "Player")
                {
                    return;
                }

                isGrenade = true;
                anim.SetTrigger("doGrenade");
                Invoke("NotGrenade", grenadeSpeed);

                Vector3 nextVec = hit.point - grenadePos.position;

                transform.LookAt(transform.position + nextVec);


                GameObject instantGrenade = Instantiate(grenadeObject, grenadePos.position, grenadePos.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                transform.rotation = Quaternion.LookRotation(hit.point - transform.position);
                nextVec.y = grenadePower;
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.right * grenadePower, ForceMode.Impulse);


                if (hasgrenade != 0)
                {
                    hasgrenade--;
                    grenades[hasgrenade].SetActive(false);
                }
            }
        }
    }

    private void NotGrenade()
    {
        isGrenade = false;
    }

    private void Attack()
    {

        fireDelay += Time.deltaTime;
        isFireReady = currentWeaponIndex != -1 ? currentWeapon.AttackSpeed < fireDelay : punchiSpeed < fireDelay;

        if (currentWeapon == null)
        {
            return;
        }

        if(isJump || isDodge)
        {
            return;
        }

        if(inputFire && isFireReady && !isDodge && !isJump && !isSwap && !isReload)
        {
            if(currentWeapon.curAmmo > 0)
            {
                currentWeapon.Use();
            }
            else
            {
                Reload();
            }

            switch (currentWeapon.type)
            {
                case Weapon.Type.Punching:
                    inputFire = Input.GetButtonDown("Fire1");
                    if (inputFire && isFireReady && !isJump && !isDodge && !isDamage)
                    {
                        currentWeapon.Use();
                        anim.SetTrigger("doPunching");
                        fireDelay = 0;
                    }
                    anim.SetBool("isWalk", true);
                    break;
                case Weapon.Type.Melee:
                    currentWeapon.Use();
                    anim.SetTrigger("doSwing");
                    break;

                case Weapon.Type.Range:
                    if(!(currentWeapon.curAmmo == 0))
                    {
                        anim.SetTrigger("doShot");
                    }
                    break;
            }

            fireDelay = 0;
        }
    }

    private void Reload()
    {
        if (currentWeapon == null)
        {
            return;
        }

        if (currentWeapon.type == Weapon.Type.Melee)
        {
            return;
        }

        if (ammo == 0)
        {
            return;
        }

        if (inputReload && !isJump && !isDodge && !isSwap && isFireReady)
        {
            if(isReload || currentWeapon.curAmmo == currentWeapon.maxAmmo)
            {
                return;
            }

            anim.SetTrigger("doReload");
            isReload = true;


            Invoke("ReloadOut", 2.3f);
        }
    }

    private void ReloadOut()
    {
        int reAmmo = ammo < currentWeapon.maxAmmo ? ammo : currentWeapon.maxAmmo;
        currentWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    private void Dodge()
    {
        if (inputDodge && !isJump && moveVec != Vector3.zero && !isDodge && !isSwap)
        {
            dodgeVec = moveVec;
            currentSpeed = dodgeSpeed;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.85f);
        }
    }

    private void DodgeOut()
    {
        isDodge = false;
        currentSpeed = runSpeed;
    }

    private void Swap()
    {
        int weaponIndex = -1;

        if (inputSwapWeapon_1) weaponIndex = 0;
        if (inputSwapWeapon_2) weaponIndex = 1;
        if (inputSwapWeapon_3) weaponIndex = 2;

        if (inputSwapWeapon_1 && (!hasWeapons[0] || currentWeaponIndex == 0))
        {
            if (!hasWeapons[0])
            {
                return;
            }

            isSwap = true;
            isWeapons[0] = false;
            anim.SetTrigger("doSwap");
            currentWeapon.gameObject.SetActive(false);
            currentWeaponIndex = -1;
            currentWeapon = punch.GetComponent<Weapon>();
            Invoke("OutSwap", 0.8f);

            return;
        }

        if (inputSwapWeapon_2 && (!hasWeapons[1] || currentWeaponIndex == 1))
        {
            if (!hasWeapons[1])
            {
                return;
            }

            isSwap = true;
            isWeapons[1] = false;
            anim.SetTrigger("doSwap");
            currentWeapon.gameObject.SetActive(false);
            currentWeaponIndex = -1;
            currentWeapon = punch.GetComponent<Weapon>();
            Invoke("OutSwap", 0.8f);
            return;
        }

        if (inputSwapWeapon_3 && (!hasWeapons[2] || currentWeaponIndex == 2))
        {
            if (!hasWeapons[2])
            {
                return;
            }

            isSwap = true;
            isWeapons[2] = false;
            anim.SetTrigger("doSwap");
            currentWeapon.gameObject.SetActive(false);
            currentWeaponIndex = -1;
            currentWeapon = punch.GetComponent<Weapon>();
            Invoke("OutSwap", 0.8f);
            return;
        }

        if((inputSwapWeapon_1 || inputSwapWeapon_2 || inputSwapWeapon_3) && !isJump && !isDodge)
        {
            for (int i = 0; i < isWeapons.Length; i++)
            {
                isWeapons[i] = false;
            }

            currentWeapon.gameObject.SetActive(false);

            if (!isWeapons[0] && !isWeapons[1] && !isWeapons[2])
            {
                punch.SetActive(true);
            }

            currentWeaponIndex = weaponIndex;
            currentWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            weapons[weaponIndex].gameObject.SetActive(true);

            if (inputSwapWeapon_1) isWeapons[0] = true;
            if (inputSwapWeapon_2) isWeapons[1] = true;
            if (inputSwapWeapon_3) isWeapons[2] = true;

            isSwap = true;

            anim.SetTrigger("doSwap");

            Invoke("OutSwap", 0.8f);
        }
    }

    private void OutSwap()
    {
        isSwap = false;
    }

    private void Interation()
    {
        if(isGetItem && nearObject != null && !isJump)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;
                isWeapons[weaponIndex] = false;

                Destroy(nearObject);
            }
        }
    }

    private void Dead()
    {
        if(health <= 0)
        {
            isDead = true;

            anim.SetLayerWeight(1, 1.0f);

            anim.SetTrigger("doDie");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.Type.ammo:
                    ammo += item.value;
                    if(ammo > maxAmmo)
                    {
                        ammo = maxAmmo;
                    }
                    break;

                case Item.Type.Health:
                    health += item.value;
                    if(health > maxHealth)
                    {
                        health = maxHealth;
                    }
                    break;

                case Item.Type.Grenade:

                    if(hasgrenade == maxHasGrenade)
                    {
                        break;
                    }

                    if(hasgrenade > maxHasGrenade)
                    {
                        hasgrenade = maxHasGrenade;
                    }

                    grenades[hasgrenade].SetActive(true);
                    hasgrenade += item.value;

                    break;
            }

            Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyMelee")
        {
            if (!isDamage)
            {
                MeleeAttack meleeAttack = other.GetComponent<MeleeAttack>();
                health -= meleeAttack.damage;
                StartCoroutine(OnDamage());
            }
        }
        else if(other.tag == "EnemyBullet")
        {
            if(!isDamage)
            {
                Bullet bulletEnemy = other.GetComponent<Bullet>();
                health -= bulletEnemy.damage;
                StartCoroutine(OnDamage());
            }
        }
    }

    private IEnumerator OnDamage()
    {
        anim.SetTrigger("doHit");
        anim.SetLayerWeight(2, 1.0f);

        isDamage = true;
        isHit = true;

        skinMeshRenderer.material.color = Color.red;
        foreach(MeshRenderer mesh in meshRenderers)
        {
            mesh.material.color = Color.red;
        }

        yield return new WaitForSeconds(1.0f);

        skinMeshRenderer.material.color = Color.white;
        foreach(MeshRenderer mesh in meshRenderers)
        {
            mesh.material.color = Color.white;
        }

        isDamage = false;
        isHit = false;
        anim.SetLayerWeight(2, 0.0f);
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Weapon")
        {
            nearObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Weapon")
        {
            nearObject = null;
        }
    }

    private void UpdateUI()
    {
        UIManager.Instance.HpText(health);
        UIManager.Instance.AmmoText(currentWeapon.curAmmo, ammo);
    }
}
