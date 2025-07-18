using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Chunk))]
public class ChunkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // デフォルトのインスペクタを描画
        DrawDefaultInspector();

        // 空白を追加
        EditorGUILayout.Space();

        // Chunkスクリプトの参照を取得
        Chunk chunk = (Chunk)target;

        // ボタンを追加
        if (GUILayout.Button("Generate Chunk Mesh"))
        {
            // メッシュ生成を実行
            chunk.GenerateChunkMesh();
            
            // シーンビューを更新
            SceneView.RepaintAll();
            
            Debug.Log("Chunk mesh generated!");
        }

        // テスト用のボタンも追加
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Test Functions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Fill with Test Blocks"))
        {
            FillWithTestBlocks(chunk);
            EditorUtility.SetDirty(chunk); // オブジェクトを変更済みとしてマーク
        }

        if (GUILayout.Button("Clear All Blocks"))
        {
            ClearAllBlocks(chunk);
            EditorUtility.SetDirty(chunk); // オブジェクトを変更済みとしてマーク
        }
    }

    // テスト用のブロック配置
    private void FillWithTestBlocks(Chunk chunk)
    {
        // 簡単なテストパターンでブロックを配置
        for (int x = 0; x < Chunk.ChunkSize; x++)
        {
            for (int y = 0; y < Chunk.ChunkSize; y++)
            {
                for (int z = 0; z < Chunk.ChunkSize; z++)
                {
                    int index = x + y * Chunk.ChunkSize + z * Chunk.ChunkSize * Chunk.ChunkSize;
                    
                    // 底面から数層は石で埋める
                    if (y < 4)
                    {
                        chunk.blocks[index] = 1; // 石
                    }
                    // その上は土
                    else if (y < 8)
                    {
                        chunk.blocks[index] = 2; // 土
                    }
                    // 表面は草
                    else if (y == 8)
                    {
                        chunk.blocks[index] = 3; // 草
                    }
                    // それ以外は空気
                    else
                    {
                        chunk.blocks[index] = 0; // 空気
                    }
                }
            }
        }
        
        Debug.Log("Test blocks filled!");
    }

    // 全ブロックをクリア
    private void ClearAllBlocks(Chunk chunk)
    {
        for (int i = 0; i < chunk.blocks.Length; i++)
        {
            chunk.blocks[i] = 0; // 全て空気に
        }
        
        // 既存のメッシュをクリア
        MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            meshFilter.mesh = null;
        }
        
        Debug.Log("All blocks cleared!");
    }
}
