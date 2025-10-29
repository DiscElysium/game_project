using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerSubscriber : MonoBehaviour, ISlotSubscriber
{
    [Header("格子关联")]
    public BackpackSlot originSlot; // 发射该子弹的格子
    // Start is called before the first frame update
    void Start()
    {
        SubscribeToSlot0();
    }

    // Update is called once per frame
    private void SubscribeToSlot0()
    {
        CircularBackpack backpack = FindObjectOfType<CircularBackpack>();
        if (backpack != null && backpack.slots.Count > 0)
        {
            backpack.slots[0].Subscribe(this);
            Debug.Log("玩家成功订阅第0个格子");
        }
    }

    public void TriggerChildSlotEffects(Vector3 position)
    {
        if (originSlot != null && originSlot.connectedSlots.Count > 0)
        {
            foreach (BackpackSlot childSlot in originSlot.connectedSlots)
            {
                if (childSlot != null && childSlot.item != null)
                {
                    childSlot.item.TriggerEffect(position);
                    Debug.Log($"子弹触发子物品效果: {childSlot.item.gameObject.name}");
                }
            }
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

}
