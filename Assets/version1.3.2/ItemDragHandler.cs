using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemComponent item; // 物品组件
    public BackpackSlot slot; // 物品当前所在的格子
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;
    private Image itemImage; // 物品的Image组件
    private RectTransform rectTransform;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        itemImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        // 确保初始位置正确
        PositionInSlotCenter();
    }

    void Start()
    {
        // 再次确保位置正确
        PositionInSlotCenter();
    }

    // 将物品定位到格子中心
    private void PositionInSlotCenter()
    {
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(50, 50); // 设置固定大小
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 只在背包打开时允许拖拽物品
        CircularBackpack backpack = FindObjectOfType<CircularBackpack>();
        if (backpack != null && !backpack.IsBackpackOpen)
        {
            return;
        }

        originalPosition = transform.position;
        originalParent = transform.parent;

        // 临时设置为Canvas的子对象确保物品显示在前景
        transform.SetParent(transform.root);
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 只在背包打开时允许拖拽物品
        CircularBackpack backpack = FindObjectOfType<CircularBackpack>();
        if (backpack != null && !backpack.IsBackpackOpen)
        {
            return;
        }
        if (backpack != null && !backpack.IsBackpackOpen)
        {
            return;
        }

        originalPosition = transform.position;
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        //transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 只在背包打开时允许拖拽物品
        CircularBackpack backpack = FindObjectOfType<CircularBackpack>();
        if (backpack != null && !backpack.IsBackpackOpen)
        {
            return;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 检测释放位置的UI元素
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        BackpackSlot droppedSlot = null;
        foreach (RaycastResult result in results)
        {
            droppedSlot = result.gameObject.GetComponent<BackpackSlot>();
            if (droppedSlot != null)
            {
                break;
            }
        }

        if (droppedSlot != null)
        {
            // 执行交换逻辑
            if (droppedSlot != slot)
            {
                // 交换物品
                BackpackSlot originalSlot = slot;
                ItemComponent tempItem = droppedSlot.item;
                if (tempItem != null)
                {
                    droppedSlot.SetItem(item);
                    originalSlot.SetItem(tempItem);
                }
                droppedSlot.RefreshIcon();
                originalSlot.RefreshIcon();
            }
        }
    }

        // 添加辅助方法
    private void PositionItemInSlotCenter(ItemComponent item, Transform slotTransform)
    {
        if (item != null)
        {
            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.anchorMin = new Vector2(0.5f, 0.5f);
                itemRect.anchorMax = new Vector2(0.5f, 0.5f);
                itemRect.pivot = new Vector2(0.5f, 0.5f);
                itemRect.anchoredPosition = Vector2.zero;
                itemRect.localRotation = Quaternion.identity;
                itemRect.localScale = Vector3.one;
                itemRect.sizeDelta = new Vector2(50, 50);
            }
            else
            {
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
                item.transform.localScale = Vector3.one;
            }
        }
    }


    // 查找物品所在的格子
    private BackpackSlot FindSlotForItem(ItemComponent searchItem)
    {
        CircularBackpack backpack = FindObjectOfType<CircularBackpack>();
        if (backpack != null)
        {
            foreach (BackpackSlot backpackSlot in backpack.slots)
            {
                if (backpackSlot.item == searchItem)
                {
                    return backpackSlot;
                }
            }
        }
        return null;
    }

    private void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);

        // 重置到格子中心
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(50, 50); // 设置固定大小
        }
        else
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }
    }
}