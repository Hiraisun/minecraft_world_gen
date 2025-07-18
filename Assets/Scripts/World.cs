using UnityEngine;

public class World : MonoBehaviour
{
    public const int ChunkSize = 16;

    [SerializeField] private GameObject chunkPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivateChunk(new Vector2Int(0, 0));
        ActivateChunk(new Vector2Int(1, 0));
        ActivateChunk(new Vector2Int(0, 1));
        ActivateChunk(new Vector2Int(1, 1));
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void ActivateChunk(Vector2Int chunkPos)
    {
        Vector3 position = new Vector3(chunkPos.x * ChunkSize, 0, chunkPos.y * ChunkSize);
        GameObject chunkObject = Instantiate(chunkPrefab, position, Quaternion.identity);
        Chunk chunk = chunkObject.GetComponent<Chunk>();

        chunk.GenerateChunkTerrain();
        chunk.GenerateChunkMesh();
    }
}
