using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// 플레이어 주변 청크를 동적으로 생성/삭제하는 무한 타일맵
/// 
/// [사용법]
/// 1. 빈 GameObject에 이 스크립트 추가
/// 2. Inspector에서 Tilemap, TileBase 배열, Player Transform 연결
/// 3. chunkSize, renderDistance 조정
/// </summary>
public class InfiniteTilemapGenerator : MonoBehaviour
{
    [Header("타일맵 설정")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase[] groundTiles;   // 사용할 타일들 (랜덤 배치)

    [Header("청크 설정")]
    [SerializeField] private int chunkSize = 16;       // 청크 하나의 크기 (16x16)
    [SerializeField] private int renderDistance = 2;   // 플레이어 주변 몇 청크까지 생성

    [Header("플레이어")]
    [SerializeField] private Transform player;

    // 현재 생성된 청크 목록
    private Dictionary<Vector2Int, bool> loadedChunks = new Dictionary<Vector2Int, bool>();
    private Vector2Int lastPlayerChunk;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        lastPlayerChunk = GetChunkCoord(player.position);
        UpdateChunks();
    }

    private void Update()
    {
        Vector2Int currentChunk = GetChunkCoord(player.position);

        // 변경: 매 프레임 체크 (빠른 이동 대응)
        lastPlayerChunk = currentChunk;
        UpdateChunks();
    }

    // ── 청크 좌표 계산 ───────────────────────────────────────
    private Vector2Int GetChunkCoord(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / chunkSize),
            Mathf.FloorToInt(worldPos.y / chunkSize)
        );
    }

    // ── 주변 청크 생성 / 멀리 있는 청크 삭제 ────────────────
    private void UpdateChunks()
    {
        HashSet<Vector2Int> neededChunks = new HashSet<Vector2Int>();

        // 필요한 청크 목록 계산
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                Vector2Int coord = new Vector2Int(
                    lastPlayerChunk.x + x,
                    lastPlayerChunk.y + y
                );
                neededChunks.Add(coord);

                // 아직 생성 안 된 청크면 생성
                if (!loadedChunks.ContainsKey(coord))
                {
                    GenerateChunk(coord);
                    loadedChunks[coord] = true;
                }
            }
        }

        // 범위 밖 청크 삭제 (메모리 절약)
        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var chunk in loadedChunks.Keys)
        {
            Vector2Int diff = chunk - lastPlayerChunk;
            if (Mathf.Abs(diff.x) > renderDistance + 1 || Mathf.Abs(diff.y) > renderDistance + 1)
                toRemove.Add(chunk);
        }

        foreach (var coord in toRemove)
        {
            RemoveChunk(coord);
            loadedChunks.Remove(coord);
        }
    }

    // ── 청크 타일 생성 ───────────────────────────────────────
    private void GenerateChunk(Vector2Int chunkCoord)
    {
        int startX = chunkCoord.x * chunkSize;
        int startY = chunkCoord.y * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePos = new Vector3Int(startX + x, startY + y, 0);

                // 같은 씨드로 같은 위치엔 항상 같은 타일 → 자연스러운 무한맵
                TileBase tile = GetDeterministicTile(tilePos.x, tilePos.y);
                tilemap.SetTile(tilePos, tile);
            }
        }
    }

    // ── 청크 타일 삭제 ───────────────────────────────────────
    private void RemoveChunk(Vector2Int chunkCoord)
    {
        int startX = chunkCoord.x * chunkSize;
        int startY = chunkCoord.y * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePos = new Vector3Int(startX + x, startY + y, 0);
                tilemap.SetTile(tilePos, null);
            }
        }
    }

    // ── 결정론적 타일 선택 (같은 좌표 = 항상 같은 타일) ─────
    private TileBase GetDeterministicTile(int x, int y)
    {
        float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
        int index = Mathf.FloorToInt(noise * groundTiles.Length);
        index = Mathf.Clamp(index, 0, groundTiles.Length - 1);
        return groundTiles[index];
    }
}