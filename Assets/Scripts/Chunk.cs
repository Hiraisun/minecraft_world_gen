using System.Collections.Generic;
using UnityEngine;


public class Chunk : MonoBehaviour
{
    public const int ChunkSize = 16;

    // 0: 空気, 1: 石, 2: 土, 3: 草
    public int[] blocks = new int[ChunkSize * ChunkSize * ChunkSize];

    MeshFilter meshFilter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    // 安全にブロックを取得する関数（範囲外の場合は空気として扱う）
    private int GetBlockSafely(int x, int y, int z)
    {
        if (x < 0 || x >= ChunkSize || y < 0 || y >= ChunkSize || z < 0 || z >= ChunkSize)
        {
            return 0; // 範囲外は空気として扱う
        }
        int index = x + y * ChunkSize + z * ChunkSize * ChunkSize;
        return blocks[index];
    }


    public void GenerateChunkMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        //配列の走査
        for (int x = 0; x < ChunkSize; x++)
        {
            for (int y = 0; y < ChunkSize; y++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    int index = x + y * ChunkSize + z * ChunkSize * ChunkSize;
                    int blockType = blocks[index];
                    if (blockType == 0) continue; // 空気は無視

                    // メッシュの頂点、三角形、UVを生成
                    // 隣接ブロックが空気である場合にのみ面を生成
                    if (GetBlockSafely(x - 1, y, z) == 0) // 西
                    {
                        vertices.Add(new Vector3(x, y, z + 1));
                        vertices.Add(new Vector3(x, y + 1, z + 1));
                        vertices.Add(new Vector3(x, y + 1, z));
                        vertices.Add(new Vector3(x, y, z));
                        int baseIndex = vertices.Count - 4;
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 1);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex + 3);
                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(0, 1));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(1, 0));
                    }
                    if (GetBlockSafely(x + 1, y, z) == 0) // 東
                    {
                        vertices.Add(new Vector3(x + 1, y, z));
                        vertices.Add(new Vector3(x + 1, y + 1, z));
                        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                        vertices.Add(new Vector3(x + 1, y, z + 1));
                        int baseIndex = vertices.Count - 4;
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 1);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex + 3);
                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(0, 1));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(1, 0));
                    }
                    if (GetBlockSafely(x, y - 1, z) == 0) // 下
                    {
                        vertices.Add(new Vector3(x, y, z));
                        vertices.Add(new Vector3(x + 1, y, z));
                        vertices.Add(new Vector3(x + 1, y, z + 1));
                        vertices.Add(new Vector3(x, y, z + 1));
                        int baseIndex = vertices.Count - 4;
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 1);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex + 3);
                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(1, 0));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(0, 1));
                    }
                    if (GetBlockSafely(x, y + 1, z) == 0) // 上
                    {
                        vertices.Add(new Vector3(x, y + 1, z));
                        vertices.Add(new Vector3(x, y + 1, z + 1));
                        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                        vertices.Add(new Vector3(x + 1, y + 1, z));
                        int baseIndex = vertices.Count - 4;
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 1);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex + 3);
                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(1, 0));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(0, 1));
                    }
                    if (GetBlockSafely(x, y, z - 1) == 0) // 南
                    {
                        vertices.Add(new Vector3(x + 1, y, z));
                        vertices.Add(new Vector3(x, y, z));
                        vertices.Add(new Vector3(x, y + 1, z));
                        vertices.Add(new Vector3(x + 1, y + 1, z));
                        int baseIndex = vertices.Count - 4;
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 1);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex + 3);
                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(1, 0));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(0, 1));
                    }
                    if (GetBlockSafely(x, y, z + 1) == 0) // 北
                    {
                        vertices.Add(new Vector3(x, y, z + 1));
                        vertices.Add(new Vector3(x + 1, y, z + 1));
                        vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                        vertices.Add(new Vector3(x, y + 1, z + 1));
                        int baseIndex = vertices.Count - 4;
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 1);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex);
                        triangles.Add(baseIndex + 2);
                        triangles.Add(baseIndex + 3);
                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(1, 0));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(0, 1));
                    }

                }
            }
        }

        // メッシュの生成
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

    }
}