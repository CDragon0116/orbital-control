using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SatelliteListPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject rowPrefab; // prefab with SatelliteRowUI attached
    public RectTransform content; // Scroll View -> Content
    public Button closeButton;
    public TMP_Text titleText;
    public TMP_InputField filterInput;

    [Header("Settings")]
    public float refreshInterval = 0.2f;
    public int maxRows = 200;

    private GravityManager gravityManager;
    private SimContext ctx;
    private bool isVisible = false;
    private float nextRefresh = 0f;

    private readonly List<SatelliteRowUI> activeRows = new();
    private readonly Stack<SatelliteRowUI> rowPool = new();

    void Awake()
    {
        //if (closeButton != null)
        //    closeButton.onClick.AddListener(Hide);

        if (titleText != null)
            titleText.text = "Satellite List";
    }

    public void Initialize(SimContext ctx)
    {
        this.ctx = ctx;
        this.gravityManager = ctx?.GravityManager;
        Hide();
    }

    void Update()
    {
        if (!isVisible) return;
        if (gravityManager == null || gravityManager.Bodies == null) return;

        if (Time.time >= nextRefresh)
        {
            RefreshList();
            nextRefresh = Time.time + refreshInterval;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        isVisible = true;
        nextRefresh = 0f;
    }

    public void Hide()
    {
        isVisible = false;
        gameObject.SetActive(false);
    }

    public void Toggle()
    {
        if (isVisible) Hide(); else Show();
    }

    private void RefreshList()
    {
        string filter = filterInput != null ? filterInput.text?.Trim() : "";

        // 1. 获取中心天体 (计算轨道需要)
        if (gravityManager == null) return;
        NBody centralBody = gravityManager.CentralBody;
        if (centralBody == null) return; // 如果没有中心天体，无法计算

        // 2. 筛选出所有要显示的卫星
        var bodies = gravityManager.Bodies
            .Where(b => b != null && !b.isCentralBody && b.CompareTag("Planet"))
            .OrderBy(b => b.name)
            .ToList();

        // 3. (可选) 应用过滤器
        if (!string.IsNullOrEmpty(filter))
            bodies = bodies.Where(b => b.name.ToLower().Contains(filter.ToLower())).ToList();

        // 4. (可选) 限制最大行数
        if (bodies.Count > maxRows)
            bodies = bodies.Take(maxRows).ToList();

        int i = 0;
        // 5. 【核心修改】遍历所有卫星
        foreach (var body in bodies)
        {
            // 6. 【核心修改】为每一颗卫星计算轨道六根数
            // 我们需要 centralBody.mass，这是一个 float
            OrbitalParameters parameters = OrbitalCalculations.CalculateOrbitalParameters(
                centralBody.mass, 
                centralBody.transform.position, 
                body.transform, 
                body.velocity
            );

            // 7. 获取或创建UI行
            SatelliteRowUI row = GetOrCreateRow(i);
            
            // 8. 【核心修改】将 NBody 和新算出的 parameters 一起绑定
            row.Bind(body, parameters);
            row.gameObject.SetActive(true);
//             // --- 添加以下代码 ---
// // 获取这一行的 Image 组件
// Image rowImage = row.GetComponent<Image>();
// if (rowImage != null)
// {
//     // i 是当前行的索引。偶数行一种颜色，奇数行另一种
//     if (i % 2 == 0)
//         rowImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f); // 较亮的暗色 (50%透明度)
//     else
//         rowImage.color = new Color(0.05f, 0.05f, 0.05f, 0.5f); // 较暗的暗色 (50%透明度)
// }
// // --- 添加结束 ---
            i++;
        }

        // 9. 停用多余的行 (这部分代码保持不变)
        for (int j = i; j < activeRows.Count; j++)
        {
            activeRows[j].Unbind();
            activeRows[j].gameObject.SetActive(false);
            rowPool.Push(activeRows[j]);
        }
        if (activeRows.Count > i)
            activeRows.RemoveRange(i, activeRows.Count - i);
    }

    private SatelliteRowUI GetOrCreateRow(int index)
    {
        if (index < activeRows.Count)
            return activeRows[index];

        SatelliteRowUI row;
        if (rowPool.Count > 0)
        {
            row = rowPool.Pop();
        }
        else
        {
            var go = Instantiate(rowPrefab, content);
            row = go.GetComponent<SatelliteRowUI>();
            if (row == null)
                Debug.LogError("Row prefab is missing SatelliteRowUI component.");
        }
        row.transform.SetSiblingIndex(index);
        activeRows.Add(row);
        return row;
    }
}