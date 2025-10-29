using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularBackpack : MonoBehaviour
{
    public GameObject backpackUI; // UI
    public List<BackpackSlot> slots = new List<BackpackSlot>(); // 格子列表
    public List<BackpackSlot> triggerStartSlot = new List<BackpackSlot>(); // 触发起始格子
    private bool isBackpackOpen = false;
    private bool isEditMode = false; // 新增：编辑模式状态
    private Coroutine triggerCoroutine;

    // 新增：编辑模式UI元素
    public GameObject editModePanel; // 编辑模式专用UI面板
    public UnityEngine.UI.Button saveEditButton; // 保存编辑按钮

    // 获取背包状态
    public bool IsBackpackOpen { get { return isBackpackOpen; } }
    public bool IsEditMode { get { return isEditMode; } } // 新增：编辑模式属性

    void Start()
    {
        // 初始状态为关闭
        isBackpackOpen = false;
        isEditMode = false;
        backpackUI.SetActive(false); // 确保UI初始关闭
        editModePanel?.SetActive(false); // 确保编辑面板初始关闭

        HideAllItemIcons(); // 隐藏物品图标
        HideAllSlots(); // 隐藏格子
        SetupSlotConnections();

        // 注册保存按钮事件
        if (saveEditButton != null)
        {
            saveEditButton.onClick.AddListener(SaveSlotPositions);
        }
    }

    void Update()
    {
        // Tab切换背包
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleBackpack();
        }

        // 新增：Q键切换编辑模式（仅在背包打开时有效）
        if (isBackpackOpen && Input.GetKeyDown(KeyCode.Q))
        {
            ToggleEditMode();
        }

        // 关闭时触发
        if (!isBackpackOpen && Input.GetMouseButtonDown(0))
        {
            Debug.Log("开始触发");
            TriggerSlotZero();
        }

        // 松开时停止
        if (Input.GetMouseButtonUp(0))
        {
            StopItemTriggerSequence();
        }
    }

    // 新增：切换编辑模式
    void ToggleEditMode()
    {
        isEditMode = !isEditMode;

        if (isEditMode)
        {
            EnterEditMode();
        }
        else
        {
            ExitEditMode();
        }
    }

    // 新增：进入编辑模式
    void EnterEditMode()
    {
        Debug.Log("进入格子编辑模式");

        // 显示编辑面板
        if (editModePanel != null)
        {
            editModePanel.SetActive(true);
        }

        // 隐藏物品图标
        HideAllItemIcons();

        // 显示所有格子背景
        ShowAllSlots();

        // 启用所有格子的拖拽功能
        foreach (BackpackSlot slot in slots)
        {
            if (slot != null)
            {
                slot.EnableEditMode();
            }
        }
    }

    // 新增：退出编辑模式
    void ExitEditMode()
    {
        Debug.Log("退出格子编辑模式");

        // 隐藏编辑面板
        if (editModePanel != null)
        {
            editModePanel.SetActive(false);
        }

        // 禁用所有格子的拖拽功能
        foreach (BackpackSlot slot in slots)
        {
            if (slot != null)
            {
                slot.DisableEditMode();
            }
        }

        // 根据背包状态显示/隐藏物品
        if (isBackpackOpen)
        {
            ShowAllItemIcons();
            ShowAllSlots();
        }
        else
        {
            HideAllItemIcons();
            HideAllSlots();
        }
    }

    // 新增：保存格子位置
    void SaveSlotPositions()
    {
        Debug.Log("保存格子位置");
        // 这里可以添加保存格子位置到PlayerPrefs或配置文件的逻辑
        foreach (BackpackSlot slot in slots)
        {
            if (slot != null)
            {
                slot.SaveCurrentPosition();
            }
        }

        // 退出编辑模式
        isEditMode = false;
        ExitEditMode();
    }

    void ToggleBackpack()
    {
        // 如果正在编辑模式，先退出编辑模式
        if (isEditMode)
        {
            isEditMode = false;
            ExitEditMode();
        }

        isBackpackOpen = !isBackpackOpen;
        backpackUI.SetActive(isBackpackOpen);

        if (isBackpackOpen)
        {
            // 显示物品图标
            ShowAllItemIcons();
            ShowAllSlots(); // 显示格子
        }
        else
        {
            // 隐藏物品图标
            HideAllItemIcons();
            HideAllSlots(); // 隐藏格子
        }
    }

    // 其他现有方法保持不变...
    void InitializeItemPositions()
    {
        foreach (BackpackSlot slot in slots)
        {
            if (slot != null && slot.item != null)
            {
                PositionItemInSlotCenter(slot);
            }
        }
    }

    void SetupSlotConnections()
    {
        if (slots.Count > 0 && slots[0] != null)
        {
            foreach (BackpackSlot startSlot in triggerStartSlot)
            {
                if (startSlot != null)
                {
                    slots[0].ConnectTo(startSlot);
                    Debug.Log($"已连接0号格子到起始位置: {startSlot.gameObject.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning("没有0号格子或0号格子为空，无法连接");
        }
    }

    void PositionItemInSlotCenter(BackpackSlot slot)
    {
        if (slot.item != null)
        {
            slot.item.transform.SetParent(slot.transform);
            RectTransform itemRect = slot.item.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.anchorMin = new Vector2(0.5f, 0.5f);
                itemRect.anchorMax = new Vector2(0.5f, 0.5f);
                itemRect.pivot = new Vector2(0.5f, 0.5f);
                itemRect.anchoredPosition = Vector2.zero;
                itemRect.localRotation = Quaternion.identity;
                itemRect.localScale = Vector3.one;
            }
            else
            {
                slot.item.transform.localPosition = Vector3.zero;
                slot.item.transform.localRotation = Quaternion.identity;
                slot.item.transform.localScale = Vector3.one;
            }
        }
    }

    void TriggerSlotZero()
    {
        StopItemTriggerSequence();
        if (slots.Count > 0 && slots[0] != null)
        {
            slots[0].TriggerWithChildren();
        }
        else
        {
            Debug.LogWarning("没有0号格子或0号格子为空，无法触发");
        }
    }

    void ShowAllItemIcons()
    {
        foreach (BackpackSlot slot in slots)
        {
            if (slot.item != null && slot.itemImage != null)
            {
                slot.itemImage.sprite = slot.item.icon;
                slot.itemImage.enabled = true;
            }
        }
    }

    void HideAllItemIcons()
    {
        foreach (BackpackSlot slot in slots)
        {
            if (slot.itemImage != null)
            {
                slot.itemImage.enabled = false;
            }
        }
    }

    void ShowAllSlots()
    {
        foreach (BackpackSlot slot in slots)
        {
            if (slot.slotBackground != null)
            {
                slot.slotBackground.enabled = true;
            }
        }
    }

    void HideAllSlots()
    {
        foreach (BackpackSlot slot in slots)
        {
            if (slot.slotBackground != null)
            {
                slot.slotBackground.enabled = false;
            }
        }
    }

    void StopItemTriggerSequence()
    {
        if (triggerCoroutine != null)
        {
            StopCoroutine(triggerCoroutine);
            triggerCoroutine = null;
        }

        foreach (BackpackSlot slot in slots)
        {
            slot.StopTrigger();
        }
    }

    public void ResetAllSlotPositions()
    {
        foreach (BackpackSlot slot in slots)
        {
            if (slot != null)
            {
                slot.ResetPosition();
            }
        }
        Debug.Log("所有格子位置已重置");
    }

    public void AddItemToSlot(ItemComponent item, int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Count)
        {
            slots[slotIndex].SetItem(item);
            if (!isBackpackOpen)
            {
                slots[slotIndex].itemImage.enabled = false;
            }
        }
    }

    public void ConnectSlots(BackpackSlot slot1, BackpackSlot slot2)
    {
        if (slot1 != null && slot2 != null)
        {
            slot1.ConnectTo(slot2);
        }
    }
}