using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject meshObject;
    public GameObject effectObject;
    public Rigidbody rigid;


    public float grenadeWaitingTime = 3.0f;
    public float grenadeRedius = 15.0f;
    public float grenadeDestroy = 5.0f;


    void Start()
    {
        StartCoroutine(Explosion());
    }


    private IEnumerator Explosion()
    {
        yield return new WaitForSeconds(grenadeWaitingTime);

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        meshObject.SetActive(false);
        effectObject.SetActive(true);

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, grenadeRedius, Vector3.up, 0, LayerMask.GetMask("Enemy"));
        
        foreach(RaycastHit hitObject in hits)
        {
            hitObject.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }

        Destroy(gameObject, grenadeDestroy);
    }

}
