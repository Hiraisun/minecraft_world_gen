using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Chunk))]
public class ChunkEditor : Editor
{
    // パーリンノイズの設定
    private float noiseScale = 0.1f;
    private float heightMultiplier = 8.0f;
    private int baseHeight = 4;
    private Vector2 noiseOffset = Vector2.zero;
    

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

        // パーリンノイズ地形生成
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Perlin Noise Terrain", EditorStyles.boldLabel);

        // ノイズパラメータの設定UI
        noiseScale = EditorGUILayout.Slider("Noise Scale", noiseScale, 0.01f, 1.0f);
        heightMultiplier = EditorGUILayout.Slider("Height Multiplier", heightMultiplier, 1.0f, 15.0f);
        baseHeight = EditorGUILayout.IntSlider("Base Height", baseHeight, 0, Chunk.ChunkSize - 1);        noiseOffset = EditorGUILayout.Vector2Field("Noise Offset", noiseOffset);
        
        // 水平レイアウトでボタンを並べる
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Perlin Noise Terrain"))
        {
            GeneratePerlinNoiseTerrain(chunk);
            EditorUtility.SetDirty(chunk); // オブジェクトを変更済みとしてマーク
        }
        
        if (GUILayout.Button("Random Offset"))
        {
            noiseOffset = new Vector2(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));
        }
        EditorGUILayout.EndHorizontal();
    }

    // パーリンノイズを使った地形生成
    private void GeneratePerlinNoiseTerrain(Chunk chunk)
    {
        // まず全てをクリア
        ClearAllBlocks(chunk);
        
        // 各XZ座標でパーリンノイズを使って高さを決定
        for (int x = 0; x < Chunk.ChunkSize; x++)
        {
            for (int z = 0; z < Chunk.ChunkSize; z++)
            {
                // パーリンノイズの座標（オフセット付き）
                float noiseX = (x + noiseOffset.x) * noiseScale;
                float noiseZ = (z + noiseOffset.y) * noiseScale;
                
                // パーリンノイズの値を取得（0-1の範囲）
                float noiseValue = Mathf.PerlinNoise(noiseX, noiseZ);
                
                // 高さを計算（基本高さ + ノイズによる変動）
                int height = baseHeight + Mathf.RoundToInt(noiseValue * heightMultiplier);
                
                // チャンクサイズ内に制限
                height = Mathf.Clamp(height, 0, Chunk.ChunkSize - 1);
                
                // 高さまでブロックを配置
                for (int y = 0; y <= height; y++)
                {
                    int index = x + y * Chunk.ChunkSize + z * Chunk.ChunkSize * Chunk.ChunkSize;
                    
                    // 地層に応じてブロックタイプを設定
                    if (y >= height - 2 && y > baseHeight) // 表面近くは土
                    {
                        chunk.blocks[index] = 2; // 土
                    }
                    else // それ以外は石
                    {
                        chunk.blocks[index] = 1; // 石
                    }
                }
            }
        }
        
        Debug.Log($"Perlin noise terrain generated! Scale: {noiseScale}, Height Multiplier: {heightMultiplier}, Base Height: {baseHeight}");
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
                    
                    chunk.blocks[index] = 1; // 全て石
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
    }
}
