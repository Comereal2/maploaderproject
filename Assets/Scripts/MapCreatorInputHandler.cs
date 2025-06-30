using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapCreatorInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputMap;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private Button generateMapInfoButton;

    [HideInInspector] public static MapCreatorInputHandler mapCreatorInputHandler;

    private int tileHeight;
    private bool isPlacing = false;
    private bool canDragClick = false;

    private void Awake()
    {
        if (mapCreatorInputHandler == null)
        {
            mapCreatorInputHandler = this;
        }
        else
        {
            Destroy(mapCreatorInputHandler);
            mapCreatorInputHandler = this;
            Debug.LogWarning("Map creator input handler replaced, was this meant to happen?");
        }
        generateMapInfoButton.onClick.AddListener(GenerateMapInformation);
    }

    private void Update()
    {
        if (heightSlider != null) tileHeight = (int)heightSlider.value;
        if (Input.GetKeyDown(KeyCode.Mouse0) && (!isPlacing || canDragClick))
        {
            isPlacing = true;

            // Create pointer data for current mouse position
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            // Raycast to all UI elements under the pointer
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                foreach(var result in results)
                {
                    if (result.gameObject.CompareTag(ImageArrayDisplay.ImageTag.ResourceGUIItem.ToString()))
                    {
                        Debug.Log("Resource Selected");
                        TileCreator.tileCreator.SetSelection(result.gameObject.GetComponent<Image>().sprite, true);
                    }
                    else if (result.gameObject.CompareTag(ImageArrayDisplay.ImageTag.TileGUIItem.ToString()))
                    {
                        Debug.Log("Tile Selected");
                        TileCreator.tileCreator.SetSelection(result.gameObject.GetComponent<Image>().sprite, false);
                    }
                }
            }
            else
            {
                Vector3 tilePosition = GetTileLocation(pointerData.position);
                bool isEditingTile = EmptyTile(new Vector2(tilePosition.x, tilePosition.z));
                TileCreator.tileCreator.PlaceTile(GetTileDirection(), tilePosition, isEditingTile);
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            isPlacing = false;
        }
    }

    private void OnEnable()
    {
        inputMap.Enable();
    }

    private void OnDisable()
    {
        inputMap.Disable();
    }

    private TileCreator.TileDirection GetTileDirection()
    {
        int direction = GameObject.FindGameObjectWithTag("TileTypeGUIItem").GetComponent<TMP_Dropdown>().value;
        switch (direction)
        {
            case 0:
                return TileCreator.TileDirection.FullTile;
            case 1:
                return TileCreator.TileDirection.North;
            case 2:
                return TileCreator.TileDirection.East;
            case 3:
                return TileCreator.TileDirection.South;
            case 4:
                return TileCreator.TileDirection.West;
        }
        return TileCreator.TileDirection.FullTile;
    }

    private Vector3 GetTileLocation(Vector2 pointerPosition)
    {
        // Generate a ray from the camera through the mouse position
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        // Calculate intersection where y equals tileHeight
        if (Mathf.Approximately(ray.direction.y, 0f))
            return ray.origin;
        float t = (tileHeight - ray.origin.y) / ray.direction.y;
        return ray.origin + ray.direction * t;
    }

    private bool EmptyTile(Vector2 position)
    {
        // Define a box that covers the area at the given position with height from 0 to 15.
        // Center y is at 7.5 (average of 0 and 15), and halfExtents.y is 7.5.
        // The x and z extents are set small (0.1) to match a precise tile location.
        Vector3 center = new Vector3(position.x, 7.5f, position.y);
        Vector3 halfExtents = new Vector3(0.1f, 7.5f, 0.1f);

        // Check for any colliders overlapping this box.
        Collider[] hits = Physics.OverlapBox(center, halfExtents);
        if(hits.Length > 0)
        {
            foreach(Collider hit in hits)
            {
                if (hit.transform.position.y == tileHeight)
                {
                    return true;
                }
                Destroy(hit.gameObject);
            }
        }
        return false;
    }

    private void GenerateMapInformation()
    {
        var mapBoundaries = TileCreator.tileCreator.mapBoundaries;
        if (mapBoundaries[0, 0] == int.MaxValue) return;

        int tileByteSize = MapInterpreter.tileByteSize;
        // Adjust xCount and yCount for inclusive boundaries.
        int xCount = mapBoundaries[0, 1] - mapBoundaries[0, 0] + 1;
        int yCount = mapBoundaries[1, 1] - mapBoundaries[1, 0] + 1;
        byte[] mapInformation = new byte[4 + xCount * yCount * tileByteSize];

        // Store xCount as the first 4 bytes.
        Array.Copy(BitConverter.GetBytes((uint)xCount), 0, mapInformation, 0, 4);

        // Loop left->right (x axis) for each row starting from the top row moving down.
        for (int yIndex = 0; yIndex < yCount; yIndex++)
        {
            int j = mapBoundaries[1, 1] - yIndex; // top (inclusive) to bottom
            for (int xIndex = 0; xIndex < xCount; xIndex++)
            {
                int i = mapBoundaries[0, 0] + xIndex;
                int offset = 4 + ((yIndex * xCount) + xIndex) * tileByteSize;
                GameObject tile = FindTile(new Vector2(i, j));

                if (tile == null)
                {
                    for (int k = 0; k < tileByteSize; k++)
                    {
                        mapInformation[offset + k] = 0;
                    }
                    continue;
                }
                for (int k = 0; k < tileByteSize; k++)
                {
                    mapInformation[offset + k] = 0;
                }
                for (int k = 0; k < 5; k++)
                {
                    byte spriteValue = (byte)(ConvertSpriteToValue(tile.transform.GetChild(k)) & 0x0F);
                    int targetIndex = offset + (k / 2);

                    if (k % 2 == 0)
                    {
                        // For even k: set the lower 4 bits
                        mapInformation[targetIndex] = (byte)((mapInformation[targetIndex] & 0x0F) | spriteValue << 4);
                    }
                    else
                    {
                        // For odd k: set the upper 4 bits
                        mapInformation[targetIndex] = (byte)((mapInformation[targetIndex] & 0xF0) | (spriteValue));
                    }
                }
                mapInformation[offset + 2] = (byte)((mapInformation[offset + 2] & 0xF0) | ((UInt16)tileHeight));
            }
        }
        CreateMapFile(mapInformation);
    }

    private GameObject FindTile(Vector2 coordinates)
    {
        // Construct regex pattern: "Tile {coordinates.x}_{anything}_{coordinates.y}"
        string pattern = $"^Tile {coordinates.x}_.*_{coordinates.y}$";
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);

        // Find all GameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (regex.IsMatch(obj.name))
            {
                Debug.Log($"{obj.name} found");
                return obj;
            }
        }
        Debug.Log($"Tile {coordinates.x}_*_{coordinates.y} not found, generating empty...");
        return null;
    }

    private byte ConvertSpriteToValue(Transform transform)
    {
        var match = System.Text.RegularExpressions.Regex.Match(transform.GetComponent<SpriteRenderer>().sprite.name, @"\d+");
        if (match.Success)
        {
            return (byte)int.Parse(match.Value);
        }
        return 0;
    }

    private void CreateMapFile(byte[] mapInformation)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string filePath = System.IO.Path.Combine(desktopPath, "map.bytes");
        System.IO.File.WriteAllBytes(filePath, mapInformation);
        Debug.Log("Map file created at " + filePath);
    }
}