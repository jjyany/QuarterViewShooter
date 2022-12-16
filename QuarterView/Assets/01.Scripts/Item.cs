using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type
    {
        ammo = 0,
        Health,
        Grenade,
        Weapon
    };
    public Type type;

    public int value;
}
