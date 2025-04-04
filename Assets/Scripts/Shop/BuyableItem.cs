using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuyableItem : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int cost;
    [SerializeField] private Buyable type;

    [Header("Components")]
    [SerializeField] private GameObject itemDescriptionBox;
    [SerializeField] private TextMeshProUGUI priceText;

    public int Cost => cost;
    public Buyable Type => type;

    private void OnEnable()
    {
        itemDescriptionBox.SetActive(false);
        priceText.text = cost.ToString();
    }

    public void Buy()
    {
        print("Bought");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            itemDescriptionBox.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            itemDescriptionBox.SetActive(false);
        }
    }

    public enum Buyable { Dash, AttackUp, FullRecovery}
}
