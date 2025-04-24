using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BuyableItem : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private OptionsSO options;
    [SerializeField] private bool useOptionsValues;

    [Space(10)]
    [SerializeField] private BuyableSO buyable;
    [SerializeField] private bool onlyBuyOnce;

    [Header("Components")]
    [SerializeField] private TextMeshPro itemNameBox;
    [SerializeField] private TextMeshPro itemDescriptionBox;
    [SerializeField] private TextMeshProUGUI priceText;

    public int Cost => buyable.GetCost(useDifficulty: useOptionsValues, difficulty: options.Difficulty);
    public Buyable Type => buyable.Type;

    private Animator anim;

    private void Awake()
    {
        TryGetComponent(out anim);
    }

    private void OnEnable()
    {
        itemDescriptionBox.gameObject.SetActive(false);
        itemNameBox.text = buyable.name;
        itemDescriptionBox.text = buyable.Description;
        priceText.text = Cost.ToString();
    }

    public void Buy()
    {
        if (onlyBuyOnce) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            anim.SetBool("Toggled", true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            anim.SetBool("Toggled", false);
        }
    }

    public enum Buyable { Dash, AttackUp, FullRecovery}
}
