using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class ShopManager : MonoBehaviour
{
    public List<CodelessIAPButton> iAPButtons = new List<CodelessIAPButton>();

    public void Start()
    {
        iAPButtons[0].onPurchaseComplete.AddListener(PurchaseFirstItem);
        iAPButtons[1].onPurchaseComplete.AddListener(Purchase1200Item);
        iAPButtons[2].onPurchaseComplete.AddListener(Purchase2900Item);
        iAPButtons[3].onPurchaseComplete.AddListener(Purchase6900Item);
        iAPButtons[4].onPurchaseComplete.AddListener(Purchase12000Item);
    }

    public void PurchaseFirstItem(Product product)
    {
        GameManager.instance.heartManager.GainHeart(20);
        NetworkManager.instance.GoldChange(2000);
        FindFirstObjectByType<LobbySceneUIManager>().DestroyFirstChaseItem();
    }

    public void Purchase1200Item(Product product)
    {
        GameManager.instance.heartManager.GainHeart(5);
        NetworkManager.instance.GoldChange(500);
    }


    public void Purchase2900Item(Product product)
    {
        GameManager.instance.heartManager.GainHeart(20);
        NetworkManager.instance.GoldChange(2000);
    }


    public void Purchase6900Item(Product product)
    {
        GameManager.instance.heartManager.GainHeart(50);
        NetworkManager.instance.GoldChange(5000);
    }


    public void Purchase12000Item(Product product)
    {
        GameManager.instance.heartManager.GainHeart(100);
        NetworkManager.instance.GoldChange(10000);
    }



}
