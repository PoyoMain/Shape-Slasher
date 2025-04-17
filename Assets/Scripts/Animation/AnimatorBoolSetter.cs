using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorBoolSetter : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        TryGetComponent(out anim);
    }

    public void SetBoolTrue(string paramName)
    {
        anim.SetBool(paramName, true);
    }

    public void SetBoolFalse(string paramName)
    {
        anim.SetBool(paramName, false);
    }
}
