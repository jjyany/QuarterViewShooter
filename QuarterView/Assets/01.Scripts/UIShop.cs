using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : MonoBehaviour
{
    public RectTransform uiGroup;

    public Animator anim;

    public GameObject buttonUI;
    public GameObject[] itemObject; //�����Ͽ� ������ ������ �迭
    public int[] itemPrice;         //�������� ���ݵ�
    public Transform[] itemPos;   //������ �������� ��ġ

    private Player enterplayer; //������ ���������� �÷��̾� ����


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

        //���κ������� ���� ���źҰ�
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
