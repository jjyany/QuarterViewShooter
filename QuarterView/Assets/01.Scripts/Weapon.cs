using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Weapon : MonoBehaviour
{
    public enum Type
    {
        Punching,
        Melee,
        Range
    };
    public Type type;

    public int damage;
    public int maxAmmo;
    public int curAmmo;

    public float AttackSpeed;
    public float bulletSpeed;
    public float minBulletShellSpeed;
    public float maxBulletShellSpeed;

    public BoxCollider meleeArea; //근접공격 범위
    public BoxCollider punchArea; //근접공격 범위

    public TrailRenderer meleetrailEffect;
    public TrailRenderer punchtrailEffect;

    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletShellPos;
    public GameObject bulletShell;

    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    public void Use()
    {
        switch (player.currentWeaponIndex)
        {
            case -1:
                StopCoroutine(Punchi());
                StartCoroutine(Punchi());
                break;

            case 0:
                    StopCoroutine(Swing());
                    StartCoroutine(Swing());
                break;

            case 1:
                    curAmmo--;
                    StartCoroutine(Shot());
                break;

            case 2:
                    curAmmo--;
                    StartCoroutine(Shot());
                break;
        }
    }

    private IEnumerator Punchi()
    {
        yield return new WaitForSeconds(0.1f);
        punchArea.enabled = true;
        punchtrailEffect.enabled = true;

        yield return new WaitForSeconds(0.15f);
        punchArea.enabled = false;
        punchtrailEffect.enabled = false;
    }

    private IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.45f);
        meleeArea.enabled = true;
        meleetrailEffect.enabled = true;

        yield return new WaitForSeconds(0.35f);
        meleeArea.enabled = false;
        meleetrailEffect.enabled = false;
    }

    private IEnumerator Shot()
    {
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * bulletSpeed;

        yield return null;

        GameObject instantBulletShell = Instantiate(bulletShell, bulletShellPos.position, bulletShellPos.rotation);
        Rigidbody bulletShellRigid = instantBulletShell.GetComponent<Rigidbody>();
        Vector3 shellVec = bulletShellPos.right * Random.Range(minBulletShellSpeed, maxBulletShellSpeed);
        bulletShellRigid.AddForce(shellVec, ForceMode.Impulse);
        bulletShellRigid.AddTorque(Vector3.up * Random.Range(minBulletShellSpeed, maxBulletShellSpeed));
    }


}
