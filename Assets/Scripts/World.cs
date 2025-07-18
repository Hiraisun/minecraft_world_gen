using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class World : MonoBehaviour
{
    public const int ChunkSize = 16;
    const int renderRadius = 5;

    private int seed;

    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private Transform playerTransform;

    Dictionary<Vector2Int, Chunk> activeChunks = new Dictionary<Vector2Int, Chunk>();

    Vector2Int lastChunkPosition;
    Vector2Int currentChunkPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        // シード値をランダムに設定
        seed = Random.Range(0, 10000);

        lastChunkPosition = GetChunkPosition();
        currentChunkPosition = lastChunkPosition;

        // 初期チャンクを生成（距離順にソート）
        List<Vector2Int> chunksToGenerate = new List<Vector2Int>();
        for (int x = -renderRadius; x <= renderRadius; x++)
        {
            for (int z = -renderRadius; z <= renderRadius; z++)
            {
                Vector2Int chunkPos = lastChunkPosition + new Vector2Int(x, z);
                float distance = Vector2.Distance(chunkPos, lastChunkPosition);
                if (distance <= renderRadius)
                {
                    chunksToGenerate.Add(chunkPos);
                }
            }
        }

        // 距離でソート（プレイヤーに近い順）
        chunksToGenerate.Sort((a, b) =>
            Vector2.Distance(a, lastChunkPosition).CompareTo(Vector2.Distance(b, lastChunkPosition))
        );

        // 近い順に生成
        foreach (var chunkPos in chunksToGenerate)
        {
            await ActivateChunkAsync(chunkPos);
        }
    }

    // Update is called once per frame
    async void Update()
    {
        currentChunkPosition = GetChunkPosition();

        // チャンクを跨いだ?
        if (currentChunkPosition != lastChunkPosition)
        {
            // 新規読み込みするチャンク、破棄するチャンクを判断----------------------------

            // 描画範囲内のチャンクを計算
            HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();
            for (int x = -renderRadius; x <= renderRadius; x++)
            {
                for (int z = -renderRadius; z <= renderRadius; z++)
                {
                    Vector2Int chunkPos = currentChunkPosition + new Vector2Int(x, z);
                    float distance = Vector2.Distance(chunkPos, currentChunkPosition);
                    if (distance <= renderRadius)
                    {
                        chunksToKeep.Add(chunkPos);
                        if (!activeChunks.ContainsKey(chunkPos))
                        {
                            await ActivateChunkAsync(chunkPos);
                        }
                    }
                }
            }

            // 破棄するチャンクを決定
            List<Vector2Int> chunksToRemove = new List<Vector2Int>();
            foreach (var chunkPos in activeChunks.Keys)
            {
                if (!chunksToKeep.Contains(chunkPos))
                {
                    chunksToRemove.Add(chunkPos);
                }
            }
            foreach (var chunkPos in chunksToRemove)
            {
                DeactivateChunk(chunkPos);
            }

            lastChunkPosition = currentChunkPosition;

        }
    }

    // プレイヤーのチャンク座標
    Vector2Int GetChunkPosition()
    {
        Vector2Int newChunkPosition = new Vector2Int(
            Mathf.FloorToInt(playerTransform.position.x / ChunkSize),
            Mathf.FloorToInt(playerTransform.position.z / ChunkSize)
        );
        return newChunkPosition;
    }

    private async Task ActivateChunkAsync(Vector2Int chunkPos)
    {
        Vector3 position = new Vector3(chunkPos.x * ChunkSize, 0, chunkPos.y * ChunkSize);
        GameObject chunkObject = Instantiate(chunkPrefab, position, Quaternion.identity);
        Chunk chunk = chunkObject.GetComponent<Chunk>();
        activeChunks[chunkPos] = chunk;

        await chunk.GenerateChunkAsync(chunkPos, seed);
    }

    private void DeactivateChunk(Vector2Int chunkPos)
    {
        if (activeChunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            activeChunks.Remove(chunkPos);
            Destroy(chunk.gameObject);
        }
    }

    public async Task SetBlockWorld(Vector3 position, int blockType)
    {
        // チャンク座標を計算
        Vector2Int chunkPos = new Vector2Int(
            Mathf.FloorToInt(position.x / ChunkSize),
            Mathf.FloorToInt(position.z / ChunkSize)
        );
        if (activeChunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            // チャンク内のローカル座標を計算
            Vector3Int localPosition = Vector3Int.FloorToInt(position - new Vector3(chunkPos.x * ChunkSize, 0, chunkPos.y * ChunkSize));
            await chunk.SetBlock(localPosition, blockType);
        }
    }
}
