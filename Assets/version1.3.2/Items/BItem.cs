using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BItem : ItemComponent
{
    public int healAmount = 20;

    public override void TriggerEffect(Vector3 position)
    {
        Debug.Log($" π”√BItem");
    }
}