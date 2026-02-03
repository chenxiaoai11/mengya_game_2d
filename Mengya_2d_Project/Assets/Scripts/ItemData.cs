using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ItemData : MonoBehaviour
{
    [Header("物品基础信息")]
    public int itemId; // 唯一ID
    public string itemName; // 物品名称
    [TextArea(3, 5)]
    public string itemDescription; // 物品描述
    [Header("UI显示相关")]
    public Sprite itemIcon; // 背包显示图标
    [Header("关卡相关（核心：标记物品所属关卡）")]
    public int belongToLevel; // 物品所属关卡（1~8，对应背包Slot_0~Slot_7）]
    public GameObject prefabReference;
}
