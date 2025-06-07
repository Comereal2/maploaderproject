using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MapInterpreter : MonoBehaviour
{
    private static Sprite[,] tiles = new Sprite[16,4];
    private static Sprite[] resources;

    [SerializeField] private GameObject tileTemplate;

    [SerializeField] private AssetReference assetReference;

    private static int tileByteSize = 3;

    private void Awake()
    {
        for (int i = 0; i < 16; i++)
        {
            Sprite[] loadedSprites = Resources.LoadAll<Sprite>("Tiles/Terrain" + i.ToString());
            if(loadedSprites == null)
            {
                Debug.LogWarning($"SpriteSheet with adress Tiles/Terrain{i.ToString()} not found");
                continue;
            }
            for (int j = 0; j < 4; j++)
            {
                tiles[i, j] = loadedSprites[j];
            }
        }
        resources = Resources.LoadAll<Sprite>("Resources");
    }

    private void Start()
    {
        assetReference.LoadAssetAsync<TextAsset>().Completed += MapLoadHandle_Completed;
    }

    private void MapLoadHandle_Completed(AsyncOperationHandle<TextAsset> handler)
    {
        if(handler.Status == AsyncOperationStatus.Succeeded)
        {
            LoadMap(handler.Result.bytes);
        }
        else
        {
            Debug.LogWarning($"Failed to load file");
        }
    }

    private void LoadMap(byte[] mapInformation)
    {
        Debug.Log("Map file found, loading map.");
        if (mapInformation.Length < 4)
        {
            Debug.LogError($"Map file size is too small. Interrupting load");
            return;
        }

        // How many cells are in a row  
        uint rowLength = BitConverter.ToUInt32(mapInformation, 0);
        Debug.Log($"Map row size read, current row size: {rowLength}");

        int chunkSize = (int)(tileByteSize * rowLength);

        Debug.Log($"Separating rows, chunk size in bytes: {chunkSize}");

        int rowAmount = (mapInformation.Length - 4) / chunkSize;

        Debug.Log($"Confirmed row amount: {rowAmount} \n Separating Chunks...");

        // Create an array to hold the chunks  
        byte[] chunk = new byte[chunkSize];
        byte[] tileInformation = new byte[tileByteSize];

        for (int i = 0; i < rowAmount; i++)
        {
            GameObject currentRowParent = new GameObject($"Row {i + 1} Parent");
            currentRowParent.transform.SetParent(transform);
            Array.Copy(mapInformation, 4 + i * chunkSize, chunk, 0, chunkSize);

            Debug.Log($"Chunk {i}: {BitConverter.ToString(chunk).Replace("-", " ")}");

            for(int j = 0; j*3 < chunk.Length; j++)
            {
                Array.Copy(chunk, tileByteSize * j, tileInformation, 0, tileByteSize);

                GameObject tile = CreateTileMashup(tileInformation);
                tile.name = $"Tile {j}";
                tile.transform.localScale = new Vector3(1.8f, 1, 1.8f);
                Instantiate(tile, new Vector3(j, (uint)(tileInformation[2] & 0x0F), i), Quaternion.Euler(0, 45, 0), currentRowParent.transform);
            }
        }
    }

    private GameObject CreateTileMashup(byte[] chunk)
    {
        GameObject tile = tileTemplate;
        //First 4 children of tile are the 4 parts of the terrain
        for(int i = 0; i < 2; i++)
        {
            tile.transform.GetChild(i*2).GetComponent<SpriteRenderer>().sprite = tiles[(uint)((chunk[i] & 0xF0) >> 4), i*2];
            tile.transform.GetChild(i*2+1).GetComponent<SpriteRenderer>().sprite = tiles[(uint)(chunk[i] & 0x0F), i*2+1];
        }
        //5th child of tile is the resource renderer
        tile.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = resources[(uint)((chunk[2] & 0xF0) >> 4)];
        return tile;
    }
}