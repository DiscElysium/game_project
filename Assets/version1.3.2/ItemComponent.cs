using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 物品基类
public abstract class ItemComponent: MonoBehaviour
{
    public string itemName;
    public Sprite icon;

    public abstract void TriggerEffect(Vector3 position);
}
