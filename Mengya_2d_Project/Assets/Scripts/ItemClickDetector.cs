using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemClickDetector : MonoBehaviour
{
    private ItemData currentItemData; // ��ǰ�������Ʒ����
    private PlayerMovement player;

    void Awake()
    {
        // ������ң�ͨ��Player��ǩ������ԭ���߼�һ�£�
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<PlayerMovement>();
        }
        else
        {
            Debug.LogWarning("δ�ҵ�Player���壬�޷����о����⣡");
        }
    }

    void Start()
    {
        // ��ȡ��ǰ��Ʒ���ϵ�ItemData���
        currentItemData = GetComponent<ItemData>();

        // ��ȫУ��
        if (currentItemData == null)
        {
            Debug.LogWarning("��ǰ��Ʒδ����ItemData����������ӣ�");
        }
    }

    void Update()
    {
        // ������������
        if (Input.GetMouseButtonDown(0))
        {
            if (ItemDetailManager.Instance != null && ItemDetailManager.Instance.IsPanelOpen()) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            CheckItemClick();
        }
    }

    /// <summary>
    /// ����Ƿ�������ǰ��Ʒ�����޸�z�ḳֵ����
    /// </summary>
    private void CheckItemClick()
    {
        if (player == null || currentItemData == null)
        {
            return;
        }

        // 2. ���߼�⣨���ԭ���߼����ж��Ƿ�������ǰ��Ʒ��
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            // 3. ���������ġ������������Ʒ��ֱ�߾���
            float distanceToPlayer = Vector3.Distance(player.GetPlayerPosition(), transform.position);

            // 4. �жϾ����Ƿ���ʰȡ�뾶��
            if (distanceToPlayer <= player.pickUpRadius)
            {
                // ���������������������
                if (ItemInvestigationManager.Instance != null)
                {
                    ItemInvestigationManager.Instance.SetCurrentItem(currentItemData);
                }
                var level6Mgr = FindObjectOfType<Level6PandaManager>();
                if (level6Mgr != null)
                {
                    level6Mgr.SetCurrentItem(currentItemData);
                }
                ItemDetailManager.Instance.ShowDetailPanel(currentItemData);
            }
            else
            {
                // �����뾶��������ʾ����ѡ���������֪����
                Debug.LogWarning($"�뿿����Ʒ����ǰ���룺{distanceToPlayer:F2}����Ҫ��{player.pickUpRadius}");
                // ��ѡ������UI��ʾ��������Ļ�м���ʾ�������Զ���޷�ʰȡ����
            }
        }
     
    }

    /// <summary>
    /// ������Ʒ�������
    /// </summary>
    private void ShowItemDetail()
    {
        if (currentItemData == null || ItemDetailManager.Instance == null)
        {
            Debug.LogWarning("�޷���ʾ������壺��Ʒ���ݻ���������������ڣ�");
            return;
        }

        // �����������������ʾ��岢������Ʒ����
        ItemDetailManager.Instance.ShowDetailPanel(currentItemData);
    }
}
