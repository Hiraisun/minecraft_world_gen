using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;

// メッシュデータを格納するクラス
public class MeshData
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uvs;

    public MeshData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
    }
}

// このクラスをChunkスクリプトの外側に追加
public static class BlockUV
{
    // テクスチャアトラスのサイズ (4x4)
    private const float AtlasSize = 4.0f;

    // 各ブロックのタイル座標 (左下を(0,0)とする)
    private static readonly Dictionary<int, Vector2> tileCoords = new Dictionary<int, Vector2>
    {
        { 1, new Vector2(0, 3) }, // 石 (左上)
        { 2, new Vector2(1, 3) }, // 土
        { 3, new Vector2(2, 3) }, // 草
    };

    public static Vector2[] GetUVs(int blockType)
    {
        if (!tileCoords.ContainsKey(blockType))
        {
            // 不明なブロックタイプの場合は空のUVを返す
            return new Vector2[4];
        }

        Vector2 tileCoord = tileCoords[blockType];
        float tileSize = 1.0f / AtlasSize;

        // タイルのUV座標を計算
        float uMin = tileCoord.x * tileSize;
        float vMin = tileCoord.y * tileSize;
        float uMax = uMin + tileSize;
        float vMax = vMin + tileSize;

        return new Vector2[]
        {
            new Vector2(uMin, vMin), // 左下
            new Vector2(uMin, vMax), // 左上
            new Vector2(uMax, vMax), // 右上
            new Vector2(uMax, vMin)  // 右下
        };
    }
}


public class Chunk : MonoBehaviour
{
    private const int ChunkSize = 16;
    private byte[] blocks = new byte[ChunkSize * ChunkSize * ChunkSize];
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;

    [Header("Terrain Generation Settings")]
    [SerializeField] private float noiseScale = 0.1f;
    [SerializeField] private float heightMultiplier = 8.0f;
    [SerializeField] private int baseHeight = 4;

    void Start()
    {

    }

    private int GetBlockSafely(int x, int y, int z)
    {
        if (x < 0 || x >= ChunkSize || y < 0 || y >= ChunkSize || z < 0 || z >= ChunkSize)
        {
            return 0;
        }
        int index = x + y * ChunkSize + z * ChunkSize * ChunkSize;
        return blocks[index];
    }

    public async Task GenerateChunkTerrainAsync(Vector2Int chunkPos, int seed)
    {
        await Task.Run(() =>
        {
            // シードを使ってノイズオフセットを生成
            System.Random random = new System.Random(seed);
            Vector2 noiseOffset = new Vector2(
                (float)(random.NextDouble() * 2000.0 - 1000.0),
                (float)(random.NextDouble() * 2000.0 - 1000.0)
            );

            // まず全てをクリア
            for (int i = 0; i < blocks.Length; i++)
            {
                blocks[i] = 0;
            }

            // 各XZ座標でパーリンノイズを使って高さを決定
            for (int x = 0; x < ChunkSize; x++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    // ワールド座標を計算（チャンク位置を考慮）
                    float worldX = chunkPos.x * ChunkSize + x;
                    float worldZ = chunkPos.y * ChunkSize + z;

                    // パーリンノイズの座標（オフセット付き）
                    float noiseX = (worldX + noiseOffset.x) * noiseScale;
                    float noiseZ = (worldZ + noiseOffset.y) * noiseScale;

                    float noiseValue = Mathf.PerlinNoise(noiseX, noiseZ);
                    int height = baseHeight + Mathf.RoundToInt(noiseValue * heightMultiplier);

                    // チャンクサイズ内に制限
                    height = Mathf.Clamp(height, 0, ChunkSize - 1);

                    // 高さまでブロックを配置
                    for (int y = 0; y <= height; y++)
                    {
                        int index = x + y * ChunkSize + z * ChunkSize * ChunkSize;

                        // 地層に応じてブロックタイプを設定
                        if (y >= height - 2 && y > baseHeight) // 表面近くは土
                        {
                            blocks[index] = 2; // 土
                        }
                        else // それ以外は石
                        {
                            blocks[index] = 1; // 石
                        }
                    }
                }
            }
        });

        await GenerateChunkMeshAsync();
    }

    private async Task GenerateChunkMeshAsync()
    {
        // メッシュデータを非同期で生成
        var meshData = await Task.Run(() => GenerateMeshData());

        // メインスレッドでMeshオブジェクトを作成・適用
        ApplyMeshData(meshData);
    }

    // 非同期で実行されるメッシュデータ生成
    private MeshData GenerateMeshData()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int x = 0; x < ChunkSize; x++)
        {
            for (int y = 0; y < ChunkSize; y++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    int index = x + y * ChunkSize + z * ChunkSize * ChunkSize;
                    int blockType = blocks[index];
                    if (blockType == 0) continue;

                    // ブロックタイプに応じたUV座標を取得
                    Vector2[] blockUVs = BlockUV.GetUVs(blockType);

                    // 西
                    if (GetBlockSafely(x - 1, y, z) == 0)
                    {
                        vertices.Add(new Vector3(x, y, z + 1));
                        vertices.Add(new Vector3(x, y + 1, z + 1));
                        vertices.Add(new Vector3(x, y + 1, z));
                        vertices.Add(new Vector3(x, y, z));
                        AddFaceToLists(vertices.Count, triangles, uvs, blockUVs);
                    }
                    // 東
                    if (GetBlockSafely(x + 1, y, z) == 0)
                    {
                        vertices.Add(new Vector3(x + 1, y, z));
                        vertices.Add(new Vector3(x + 1, y + 1, z));
                        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                        vertices.Add(new Vector3(x + 1, y, z + 1));
                        AddFaceToLists(vertices.Count, triangles, uvs, blockUVs);
                    }
                    // 下
                    if (GetBlockSafely(x, y - 1, z) == 0)
                    {
                        vertices.Add(new Vector3(x, y, z + 1));
                        vertices.Add(new Vector3(x, y, z));
                        vertices.Add(new Vector3(x + 1, y, z));
                        vertices.Add(new Vector3(x + 1, y, z + 1));
                        AddFaceToLists(vertices.Count, triangles, uvs, blockUVs);
                    }
                    // 上
                    if (GetBlockSafely(x, y + 1, z) == 0)
                    {
                        vertices.Add(new Vector3(x, y + 1, z));
                        vertices.Add(new Vector3(x, y + 1, z + 1));
                        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                        vertices.Add(new Vector3(x + 1, y + 1, z));
                        AddFaceToLists(vertices.Count, triangles, uvs, blockUVs);
                    }
                    // 南
                    if (GetBlockSafely(x, y, z - 1) == 0)
                    {
                        vertices.Add(new Vector3(x, y, z));
                        vertices.Add(new Vector3(x, y + 1, z));
                        vertices.Add(new Vector3(x + 1, y + 1, z));
                        vertices.Add(new Vector3(x + 1, y, z));
                        AddFaceToLists(vertices.Count, triangles, uvs, blockUVs);
                    }
                    // 北
                    if (GetBlockSafely(x, y, z + 1) == 0)
                    {
                        vertices.Add(new Vector3(x + 1, y, z + 1));
                        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                        vertices.Add(new Vector3(x, y + 1, z + 1));
                        vertices.Add(new Vector3(x, y, z + 1));
                        AddFaceToLists(vertices.Count, triangles, uvs, blockUVs);
                    }
                }
            }
        }

        return new MeshData(vertices, triangles, uvs);
    }

    // メインスレッドでMeshオブジェクトを作成・適用
    private void ApplyMeshData(MeshData meshData)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = meshData.vertices.ToArray();
        mesh.triangles = meshData.triangles.ToArray();
        mesh.uv = meshData.uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    // 非同期での面の生成処理
    private void AddFaceToLists(int vertexCount, List<int> triangles, List<Vector2> uvs, Vector2[] faceUVs)
    {
        int baseIndex = vertexCount - 4;
        triangles.Add(baseIndex);
        triangles.Add(baseIndex + 1);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 3);

        // UVの向きを頂点の向きに合わせる
        uvs.Add(faceUVs[0]);
        uvs.Add(faceUVs[1]);
        uvs.Add(faceUVs[2]);
        uvs.Add(faceUVs[3]);
    }

    public byte[] GetBlocks()
    {
        return blocks;
    }

    public async Task LoadChunkData(string filepath)
    {
        // チャンクデータを復元
        blocks = await File.ReadAllBytesAsync(filepath);
        Debug.Log($"Chunk data loaded: {blocks.Length} bytes");

        await GenerateChunkMeshAsync();
    }

    public async Task SetBlock(Vector3Int position, byte blockType)
    {
        if (position.x < 0 || position.x >= ChunkSize || position.y < 0 || position.y >= ChunkSize || position.z < 0 || position.z >= ChunkSize)
        {
            Debug.LogWarning("Block position out of bounds");
            return;
        }
        int index = position.x + position.y * ChunkSize + position.z * ChunkSize * ChunkSize;
        blocks[index] = blockType;

        // メッシュを再生成
        await GenerateChunkMeshAsync();
    }


}