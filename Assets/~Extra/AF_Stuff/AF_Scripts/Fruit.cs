using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Fruit : MonoBehaviour
{
    [SerializeField] private GameObject fruitSlicedPrefab;
    [SerializeField] private  float minStartForce = 10f;
    [SerializeField] private  float maxStartForce = 15f;
    [SerializeField] private Color[] colorOptions;

    [Space(15)]
    [SerializeField] private VoidEventSO fruitSlicedEventSO;

    [HideInInspector] public bool sliced;
    Rigidbody2D rb;
    SpriteRenderer[] sRends;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(transform.up * Random.Range(minStartForce, maxStartForce), ForceMode2D.Impulse);

        sRends = GetComponentsInChildren<SpriteRenderer>();
        Color chosenColor = colorOptions[Random.Range(0, colorOptions.Length)];
        for (int i = 0; i < sRends.Length; i++)
        {
            sRends[i].color = chosenColor;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent(out Blade _))
        {
            Vector3 direction = (col.transform.position - transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);
            
            Quaternion newRot = new(transform.rotation.x, transform.rotation.y, rotation.z, transform.rotation.w);

            GameObject slicedFruit = Instantiate(fruitSlicedPrefab, transform.position, newRot);
            Destroy(slicedFruit, 3f);

            SpriteRenderer[] splitSprites = slicedFruit.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < splitSprites.Length; i++)
            {
                splitSprites[i].color = sRends[0].color;
            }

            fruitSlicedEventSO.RaiseEvent();
            sliced = true;
            Destroy(gameObject);
        }
    }
}
