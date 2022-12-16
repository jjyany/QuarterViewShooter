using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class RandomPoint
{
    public static Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance, int areaMask)
    {
        //(반경(반지름) 1을 가진 원안의 임의의 위치값 * 거리) + 자신
        //즉 자신의 위치에서 랜덤하게 생성된 원에 거리값을 더한 값
        var randomPos = Random.insideUnitSphere * distance + center;

        //raycastHit과 비슷한 기능
        NavMeshHit hit;

        //Back된 NavMesh의 정보를 바탕으로 위치를 지정한다(위치값, 받아온 Hit정보, 반경, areaMask)
        NavMesh.SamplePosition(randomPos, out hit, distance, areaMask);

        //위 SamplePosition에서 받아온 (랜덤한)위치값을 반환한다.
        return hit.position;
    }
}
