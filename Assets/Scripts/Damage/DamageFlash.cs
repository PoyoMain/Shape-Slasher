using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashTime = 0.25f;

    public float FlashTime => flashTime;

    private SpriteRenderer[] spriteRenderers;
    private Material[] materials;

    private Coroutine damageFlashCoroutine;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        Init();
    }

    private void Init()
    {
        materials = new Material[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            materials[i] = spriteRenderers[i].material; 
        }
    }

    public void CallDamageFlash()
    {
        if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
        damageFlashCoroutine = StartCoroutine(DamageFlasher());
    }

    private IEnumerator DamageFlasher()
    {
        SetFlashColor();
        float elapsedTime = 0f;

        while(elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;

            float currentFlashAmount = Mathf.Lerp(1f, 0f, elapsedTime / flashTime);
            SetFlashAmount(currentFlashAmount);

            yield return null;
        }
    }

    private void SetFlashColor()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetColor("_FlashColor", flashColor);
        }
    }

    private void SetFlashAmount(float amount)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat("_FlashAmount", amount);
        }
    }

    public void SetAlphaToOne()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            Color tempColor = spriteRenderers[i].color;
            tempColor.a = 1;
            spriteRenderers[i].color = tempColor;
        }
    }
}
