using UnityEngine;

/// <summary>
/// 웨이브 하나의 설정을 담는 ScriptableObject
/// 
/// [생성 방법]
/// Project 창 우클릭 → Create → Game/Wave Data
/// 웨이브 수만큼 에셋 파일 생성 후 WaveManager의 waves 배열에 순서대로 연결하세요
/// </summary>
[CreateAssetMenu(fileName = "WaveData", menuName = "Game/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("웨이브 기본 설정")]
    [Tooltip("이 웨이브에서 스폰할 총 적 수")]
    public int enemyCount = 10;

    [Tooltip("적 스폰 간격 (초)")]
    public float spawnInterval = 1.5f;

    [Tooltip("웨이브 시작 전 대기 시간 (초)")]
    public float startDelay = 2f;

    [Header("적 구성")]
    [Tooltip("이 웨이브에서 스폰할 적 프리팹 목록. 비워두면 WaveManager 기본 프리팹 사용")]
    public GameObject[] enemyPrefabs;

    [Tooltip("각 프리팹의 스폰 가중치. enemyPrefabs와 길이가 같아야 합니다")]
    public int[] spawnWeights;

    [Header("적 스탯 배율 (기본값 1 = 원본 그대로)")]
    public float enemyHpMultiplier = 1f;
    public float enemySpeedMultiplier = 1f;
    public float enemyExpMultiplier = 1f;

    /// <summary>가중치 기반 랜덤 프리팹 선택</summary>
    public GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return null;

        // 가중치 없으면 균등 랜덤
        if (spawnWeights == null || spawnWeights.Length != enemyPrefabs.Length)
            return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        int totalWeight = 0;
        foreach (int w in spawnWeights) totalWeight += w;

        int rand = Random.Range(0, totalWeight);
        int cumulative = 0;
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            cumulative += spawnWeights[i];
            if (rand < cumulative) return enemyPrefabs[i];
        }

        return enemyPrefabs[0];
    }
}