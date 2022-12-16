using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class RandomPoint
{
    public static Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance, int areaMask)
    {
        //(�ݰ�(������) 1�� ���� ������ ������ ��ġ�� * �Ÿ�) + �ڽ�
        //�� �ڽ��� ��ġ���� �����ϰ� ������ ���� �Ÿ����� ���� ��
        var randomPos = Random.insideUnitSphere * distance + center;

        //raycastHit�� ����� ���
        NavMeshHit hit;

        //Back�� NavMesh�� ������ �������� ��ġ�� �����Ѵ�(��ġ��, �޾ƿ� Hit����, �ݰ�, areaMask)
        NavMesh.SamplePosition(randomPos, out hit, distance, areaMask);

        //�� SamplePosition���� �޾ƿ� (������)��ġ���� ��ȯ�Ѵ�.
        return hit.position;
    }
}
