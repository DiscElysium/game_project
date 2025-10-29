using UnityEngine;

public interface ISlotSubscriber
{
    void TriggerChildSlotEffects(Vector3 position);
    Vector3 GetPosition();
}