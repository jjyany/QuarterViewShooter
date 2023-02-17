using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : MonoBehaviour
{
    public RectTransform uiGroup;

    public Animator anim;

    public GameObject buttonUI;
    public GameObject[] itemObject; //구매하여 생성될 아이템 배열
    public int[] itemPrice;         //아이템의 가격들
    public Transform[] itemPos;   //생성될 아이템의 위치

    private Player enterplayer; //상점에 들어왔을때의 플레이어 정보


    public void ShopEnter(Player player)
    {
        enterplayer = player;
        uiGroup.anchoredPosition = Vector3.zero;
    }

    public void ShopExit()
    {
        anim.SetTrigger("doHello");
        uiGroup.anchoredPosition = Vector3.down * 1000;
    }

    public void ShopBuyItem(int index)
    {
        int price = itemPrice[index];

        //코인부족으로 인한 구매불가
        if(price > enterplayer.coin)
        {
            return;
        }

        enterplayer.coin -= price;

        Instantiate(itemObject[index], itemPos[index].position, itemPos[index].rotation);
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            buttonUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            buttonUI.SetActive(false);
        }
    }
}
