using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    public static UIManager Instance
    {
        get
        {
            if(instance == null) instance = FindObjectOfType<UIManager>();

            return instance;
        }
    }

    public TextMeshProUGUI ammo;
    public TextMeshProUGUI hp;

    public void AmmoText(int currentAmmo, int maxAmmo)
    {
        ammo.text = currentAmmo + " / " + maxAmmo;
    }

    public void HpText(int _hp)
    {
        hp.text = "HP : " + _hp;
    }

}
