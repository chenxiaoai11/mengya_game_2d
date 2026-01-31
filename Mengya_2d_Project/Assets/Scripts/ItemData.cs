using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class ItemData : MonoBehaviour
    {
        [Header("物品基础信息")]
    public int itemId;
    public string itemName; // 物品名称
    [TextArea(3, 5)] // 让输入框变成多行文本框，更方便输入描述
    public string itemDescription; // 物品描述
    public Sprite itemIcon;
}
