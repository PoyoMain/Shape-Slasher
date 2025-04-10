using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitTrigger : MonoBehaviour
{
    [SerializeField] private VoidEventSO fruitFellEventSO;
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Fruit fruit))
        {
            if (fruit.sliced) return;

            fruitFellEventSO.RaiseEvent();
        }
    }
}
