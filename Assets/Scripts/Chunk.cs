using System.Collections.Generic;
using UnityEngine;

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
    public const int ChunkSize = 16;
    public int[] blocks = new int[ChunkSize * ChunkSize * ChunkSize];
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;

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

    public void GenerateChunkTerrain()
    {
        // 仮に全blockを石で初期化
        for (int x = 0; x < ChunkSize; x++)
        {
            for (int y = 0; y < ChunkSize; y++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    int index = x + y * ChunkSize + z * ChunkSize * ChunkSize;
                    blocks[index] = 1; // 全て石で初期化
                }
            }
        }
    }

    public void GenerateChunkMesh()
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
                        AddFace(vertices.Count, triangles, uvs, blockUVs);
                    }
                    // 東
                    if (GetBlockSafely(x + 1, y, z) == 0)
                    {
                        vertices.Add(new Vector3(x + 1, y, z));
                        vertices.Add(new Vector3(x + 1, y + 1, z));
                        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                        vertices.Add(new Vector3(x + 1, y, z + 1));
                        AddFace(vertices.Count, triangles, uvs, blockUVs);
                    }
                    // 下
                    if (GetBlockSafely(x, y - 1, z) == 0)
                    {
                        vertices.Add(new Vector3(x, y, z + 1));
                        vertices.Add(new Vector3(x, y, z));
                        vertices.Add(new Vector3(x + 1, y, z));
                        vertices.Add(new Vector3(x + 1, y, z + 1));
                        AddFace(vertices.Count, triangles, uvs, blockUVs);
                    }
                    // 上
                    if (GetBlockSafely(x, y + 1, z) == 0)
                    {
                        vertices.Add(new Vector3(x, y + 1, z));
                        vertices.Add(new Vector3(x, y + 1, z + 1));
                        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                        vertices.Add(new Vector3(x + 1, y + 1, z));
                        AddFace(vertices.Count, triangles, uvs, blockUVs);
                    }
                    // 南
                    if (GetBlockSafely(x, y, z - 1) == 0)
                    {
                        vertices.Add(new Vector3(x, y, z));
                        vertices.Add(new Vector3(x, y + 1, z));
                        vertices.Add(new Vector3(x + 1, y + 1, z));
                        vertices.Add(new Vector3(x + 1, y, z));
                        AddFace(vertices.Count, triangles, uvs, blockUVs);
                    }
                    // 北
                    if (GetBlockSafely(x, y, z + 1) == 0)
                    {
                        vertices.Add(new Vector3(x + 1, y, z + 1));
                        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                        vertices.Add(new Vector3(x, y + 1, z + 1));
                        vertices.Add(new Vector3(x, y, z + 1));
                        AddFace(vertices.Count, triangles, uvs, blockUVs);
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    // 面の生成処理を共通化
    private void AddFace(int vertexCount, List<int> triangles, List<Vector2> uvs, Vector2[] faceUVs)
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
}