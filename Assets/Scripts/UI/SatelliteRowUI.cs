using UnityEngine;
using TMPro;

public class SatelliteRowUI : MonoBehaviour
{
    // 1. 定义UI字段（6个）
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI altitudeText;
    public TextMeshProUGUI semiMajorAxisText; // 半长轴
    public TextMeshProUGUI eccentricityText;  // 偏心率
    public TextMeshProUGUI inclinationText;   // 倾斜率
    public TextMeshProUGUI raanText;            // 升交点赤经 (RAAN)
    public TextMeshProUGUI perigeeArgText;     // 近地点幅角 (argument of perigee)
    public TextMeshProUGUI trueAnomalyText;    // 真近点角 (true anomaly)

    private NBody boundBody;
    private OrbitalParameters orbitalParams; // 保存轨道参数

    // 2. Bind方法现在接收 NBody 和 OrbitalParameters
    public void Bind(NBody body, OrbitalParameters parameters)
    {
        boundBody = body;
        orbitalParams = parameters;
        UpdateDisplay(); // 绑定时立即更新一次
    }

    public void Unbind()
    {
        boundBody = null;
    }

    // 3. 更新显示逻辑
    public void UpdateDisplay()
    {
        if (boundBody == null)
        {
            // 如果物体被销毁，清空所有文本
            nameText.text = "-";
            altitudeText.text = "-";
            semiMajorAxisText.text = "-";
            eccentricityText.text = "-";
            inclinationText.text = "-";
            raanText.text = "-";
            return;
        }

        // 填充所有文本字段
        nameText.text = boundBody.name;
        
        // 使用项目中的单位 (1 unit = 10 km)
        altitudeText.text = $"{(float)boundBody.altitude * 10f:F1} km";

        // 检查轨道参数是否有效
        if (orbitalParams.isValid)
        {
            semiMajorAxisText.text = $"{orbitalParams.semiMajorAxis * 10f:F1} km";
            eccentricityText.text = $"{orbitalParams.eccentricity:F3}"; // F3 = 3位小数
            inclinationText.text = $"{orbitalParams.inclination:F2}°"; // F2 = 2位小数
            raanText.text = $"{orbitalParams.RAAN:F2}°";
            if (perigeeArgText != null)
                perigeeArgText.text = $"{orbitalParams.argumentOfPerigee:F1}°";
            if (trueAnomalyText != null)
                trueAnomalyText.text = $"{orbitalParams.trueAnomaly:F1}°";
        }
        else
        {
            // 如果轨道无效 (例如速度为0)，显示 "N/A"
            semiMajorAxisText.text = "N/A";
            eccentricityText.text = "N/A";
            inclinationText.text = "N/A";
            raanText.text = "N/A";
            if (perigeeArgText != null)
                perigeeArgText.text = "N/A";
            if (trueAnomalyText != null)
                trueAnomalyText.text = "N/A";
        }
    }
}