using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossFarAttack : EnemyBullet
{

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        GameObject target = GameObject.FindWithTag("Player");
        agent.SetDestination(target.transform.position);
    }
}
