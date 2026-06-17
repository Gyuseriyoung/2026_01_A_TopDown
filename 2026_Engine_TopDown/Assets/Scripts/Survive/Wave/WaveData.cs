using UnityEngine;

/// <summary>
/// 웨이브 하나의 설정을 담는 ScriptableObject
///
/// [에셋 생성]
/// Project 창 우클릭 → Create → Game/Wave Data
/// 웨이브 수만큼 파일 생성 (Wave1.asset, Wave2.asset ...)
/// WaveManager Inspector의 Waves 배열에 순서대로 드래그
///
/// [spawnWeights 사용 예시]
///   enemyPrefabs  = [Slime, Goblin, Orc]
///   spawnWeights  = [5,     3,      1  ]  → 슬라임 5/9, 고블린 3/9, 오크 1/9 확률
///   비워두면 균등 확률로 선택합니다
/// </summary>
[CreateAssetMenu(fileName = "WaveData", menuName = "Game/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("웨이브 기본 설정")]
    [Tooltip("이 웨이브에서 스폰할 총 적 수")]
    public int enemyCount = 10;

    [Tooltip("적 스폰 간격 (초)")]
    public float spawnInterval = 1.5f;

    [Tooltip("'Wave N' 메시지 출력 후 스폰 시작까지 대기 시간 (초)")]
    public float startDelay = 2f;

    [Header("적 구성")]
    [Tooltip("이 웨이브에서 스폰할 적 프리팹 목록.\n비워두면 WaveManager의 defaultEnemyPrefabs 사용")]
    public GameObject[] enemyPrefabs;

    [Tooltip("각 프리팹의 스폰 가중치 (enemyPrefabs와 길이가 같아야 합니다).\n비워두면 균등 확률")]
    public int[] spawnWeights;

    [Header("적 스탯 배율 (1 = 기본값 그대로)")]
    [Tooltip("스폰된 적의 최대 HP에 곱합니다")]
    public float enemyHpMultiplier = 1f;
    [Tooltip("스폰된 적의 이동속도에 곱합니다")]
    public float enemySpeedMultiplier = 1f;
    [Tooltip("스폰된 적의 경험치 드랍량에 곱합니다")]
    public float enemyExpMultiplier = 1f;

    // ── 내부 유틸 ─────────────────────────────────────────

    /// <summary>가중치 기반 랜덤 프리팹 선택. null 반환 시 WaveManager가 기본 프리팹 사용</summary>
    public GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return null;

        if (spawnWeights == null || spawnWeights.Length != enemyPrefabs.Length)
            return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        int total = 0;
        foreach (int w in spawnWeights) total += w;

        int rand = Random.Range(0, total);
        int cumul = 0;
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            cumul += spawnWeights[i];
            if (rand < cumul) return enemyPrefabs[i];
        }
        return enemyPrefabs[0];
    }
}