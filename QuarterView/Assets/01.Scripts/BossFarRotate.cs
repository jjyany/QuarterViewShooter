using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFarRotate : MonoBehaviour
{
    public float rotateSpeed = 2f;

    void FixedUpdate()
    {
        transform.Rotate(rotateSpeed * Time.deltaTime, 0, 0);
    }
}
