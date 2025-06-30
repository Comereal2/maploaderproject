using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapCreatorInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputMap;
    [SerializeField] private GameObject heightSliderObject;

    [HideInInspector] public static MapCreatorInputHandler mapCreatorInputHandler;

    private int tileHeight;
    private bool isPlacing = false;
    private bool canDragClick = false;
    private Slider heightSlider;

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
        heightSlider = heightSliderObject.GetComponent<Slider>();
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
                Debug.Log("Map element hit!");
                Vector3 tilePosition = GetTileLocation(pointerData.position);
                bool isEditingTile = EmptyTile(tilePosition);
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
            return false;
        }
        return false;
    }
}