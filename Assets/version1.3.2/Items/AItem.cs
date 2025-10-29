using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AItem : ItemComponent
{
    public int healAmount = 20;

    public override void TriggerEffect(Vector3 position)
    {
        Debug.Log($"使用AItem点生命值");
    }
}