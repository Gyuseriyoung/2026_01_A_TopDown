using UnityEngine;
using TMPro;

public class RecordPanelManager : MonoBehaviour
{
    [Header("최고 기록 UI 연결")]
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [SerializeField] private TextMeshProUGUI bestKillsText;

    private void OnEnable()
    {
        // 기록 패널이 켜질 때마다 최신 기록을 반영합니다.
        DisplayBestRecords();
    }

    private void DisplayBestRecords()
    {
        // ⭐️ GameManager에 이미 로드되어 있는 최신 최고 기록을 그대로 가져옵니다.
        if (GameManager.Instance != null)
        {
            int bestTimeInSeconds = GameManager.Instance.BestTime;
            int bestKills = GameManager.Instance.BestKill;

            // 시간 포맷 연산 (초 -> 분:초)
            int minutes = bestTimeInSeconds / 60;
            int seconds = bestTimeInSeconds % 60;

            // UI 텍스트 업데이트
            if (bestTimeText != null)
                bestTimeText.text = $"최고 생존 시간 : {minutes:00}:{seconds:00}";

            if (bestKillsText != null)
                bestKillsText.text = $"최고 처치 기록 : {bestKills} 마리";
        }
    }
}