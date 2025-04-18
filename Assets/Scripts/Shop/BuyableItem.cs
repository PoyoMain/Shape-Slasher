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
    [Header("Stats")]
    [SerializeField] private DifficultyInt cost;
    [SerializeField] private int testCost;
    [SerializeField] private Buyable type;
    [SerializeField] private bool onlyBuyOnce;

    [Header("Components")]
    [SerializeField] private GameObject itemDescriptionBox;
    [SerializeField] private TextMeshProUGUI priceText;

    public int Cost => useOptionsValues ? cost.ReturnValue(options.Difficulty) : testCost;
    public Buyable Type => type;

    private Animator anim;

    private void Awake()
    {
        TryGetComponent(out anim);
    }

    private void OnEnable()
    {
        itemDescriptionBox.SetActive(false);
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
