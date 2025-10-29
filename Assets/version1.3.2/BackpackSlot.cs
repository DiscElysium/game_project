using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackpackSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemComponent item;
    public Image itemImage;
    public Image slotBackground;
    public List<BackpackSlot> connectedSlots = new List<BackpackSlot>();

    // 编辑模式相关
    private bool isEditMode = false;
    private Vector2 originalPosition;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas; // 新增：父级Canvas
    private Vector2 dragOffset; // 新增：拖拽偏移量

    private Coroutine triggerCoroutine;
    private List<ISlotSubscriber> subscribers = new List<ISlotSubscriber>();
    public Transform playerTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 获取父级Canvas
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            parentCanvas = FindObjectOfType<Canvas>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 保存原始位置
        originalPosition = rectTransform.anchoredPosition;

        // 确保锚点设置正确
        SetupRectTransform();

        if (itemImage != null)
        {
            PositionIconInCenter();
        }

        if (item != null)
        {
            PositionItemInSlot();
        }
    }

    // 新增：设置RectTransform属性
    private void SetupRectTransform()
    {
        if (rectTransform != null)
        {
            // 统一设置锚点和轴心点
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    public void EnableEditMode()
    {
        isEditMode = true;
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = true;

        if (itemImage != null)
        {
            itemImage.enabled = false;
        }
    }

    public void DisableEditMode()
    {
        isEditMode = false;
        canvasGroup.alpha = 1f;

        CircularBackpack backpack = FindObjectOfType<CircularBackpack>();
        if (backpack != null && backpack.IsBackpackOpen && !backpack.IsEditMode)
        {
            if (itemImage != null && item != null)
            {
                itemImage.enabled = true;
            }
        }
    }

    public void SaveCurrentPosition()
    {
        originalPosition = rectTransform.anchoredPosition;
        Debug.Log($"格子 {gameObject.name} 位置已保存: {originalPosition}");
    }

    // 修复后的拖拽方法
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isEditMode) return;

        // 计算鼠标位置与格子中心的偏移量
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localCursor))
        {
            dragOffset = rectTransform.anchoredPosition - localCursor;
        }
        else
        {
            dragOffset = Vector2.zero;
        }

        transform.SetAsLastSibling();
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isEditMode) return;

        // 修复坐标转换
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localCursor))
        {
            rectTransform.anchoredPosition = localCursor + dragOffset;

            // 限制在父级RectTransform范围内（可选）
            ClampToParentBounds();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isEditMode) return;

        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = true;
    }

    // 新增：限制在父级范围内
    private void ClampToParentBounds()
    {
        if (rectTransform.parent is RectTransform parentRect)
        {
            Vector2 parentSize = parentRect.rect.size;
            Vector2 anchoredPos = rectTransform.anchoredPosition;
            Vector2 slotSize = rectTransform.rect.size;

            // 计算边界限制
            float minX = -parentSize.x * 0.5f + slotSize.x * 0.5f;
            float maxX = parentSize.x * 0.5f - slotSize.x * 0.5f;
            float minY = -parentSize.y * 0.5f + slotSize.y * 0.5f;
            float maxY = parentSize.y * 0.5f - slotSize.y * 0.5f;

            anchoredPos.x = Mathf.Clamp(anchoredPos.x, minX, maxX);
            anchoredPos.y = Mathf.Clamp(anchoredPos.y, minY, maxY);

            rectTransform.anchoredPosition = anchoredPos;
        }
    }

    // 新增：重置格子位置到原始位置
    public void ResetPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
    }

    // 其他方法保持不变...
    public void SetItem(ItemComponent newItem)
    {
        item = newItem;
        if (item != null)
        {
            item.transform.SetParent(transform);
            itemImage = item.GetComponent<Image>();

            if (itemImage != null)
            {
                itemImage.sprite = item.icon;
                itemImage.enabled = true;
                PositionIconInCenter();

                RectTransform iconRect = itemImage.GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    iconRect.sizeDelta = new Vector2(50, 50);
                }
            }

            ItemDragHandler dragHandler = item.GetComponent<ItemDragHandler>();
            if (dragHandler == null)
            {
                dragHandler = item.gameObject.AddComponent<ItemDragHandler>();
            }
            dragHandler.item = item;
            dragHandler.slot = this;

            PositionItemInSlot();
        }
        else
        {
            ClearSlot();
        }
    }

    private void PositionItemInSlot()
    {
        if (item != null)
        {
            item.transform.SetParent(transform);
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

    private void PositionIconInCenter()
    {
        if (itemImage != null)
        {
            RectTransform rectTransform = itemImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(50, 50);
            }
        }
    }

    public void ClearSlot()
    {
        item = null;
        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }
    }

    public void RefreshIcon()
    {
        if (itemImage != null)
        {
            if (item != null)
            {
                itemImage.sprite = item.icon;
                itemImage.enabled = true;
                PositionIconInCenter();
            }
            else
            {
                itemImage.sprite = null;
                itemImage.enabled = false;
            }
        }
    }

    public void ConnectTo(BackpackSlot otherSlot)
    {
        if (otherSlot != null && !connectedSlots.Contains(otherSlot))
        {
            connectedSlots.Add(otherSlot);
        }
    }

    public void Subscribe(ISlotSubscriber subscriber)
    {
        if (subscriber != null && !subscribers.Contains(subscriber))
        {
            subscribers.Add(subscriber);
        }
    }

    public void Unsubscribe(ISlotSubscriber subscriber)
    {
        if (subscriber != null && subscribers.Contains(subscriber))
        {
            subscribers.Remove(subscriber);
        }
    }

    public void TriggerWithChildren()
    {
        StopTrigger();
        triggerCoroutine = StartCoroutine(TriggerTreeCoroutine());
    }

    public void StopTrigger()
    {
        if (triggerCoroutine != null)
        {
            StopCoroutine(triggerCoroutine);
            triggerCoroutine = null;
        }
    }

    private IEnumerator TriggerTreeCoroutine()
    {
        yield return new WaitForSeconds(0.25f);
        yield return TriggerSubscribersCoroutine();
    }

    private IEnumerator TriggerSubscribersCoroutine()
    {
        Vector3 triggerPosition = playerTransform != null ? playerTransform.position : Vector3.zero;

        foreach (ISlotSubscriber subscriber in subscribers.ToArray())
        {
            if (subscriber != null)
            {
                subscriber.TriggerChildSlotEffects(triggerPosition);
            }
            else
            {
                subscribers.Remove(subscriber);
            }
        }

        yield return new WaitForSeconds(0.25f);

        foreach (BackpackSlot childSlot in connectedSlots)
        {
            if (childSlot != null)
            {
                childSlot.TriggerWithChildren();
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    public void TriggerChildEffectsFromBullet(Vector3 bulletPosition)
    {
        if (connectedSlots.Count > 0)
        {
            foreach (BackpackSlot childSlot in connectedSlots)
            {
                if (childSlot != null && childSlot.item != null)
                {
                    childSlot.item.TriggerEffect(bulletPosition);
                }
            }
        }
    }
}