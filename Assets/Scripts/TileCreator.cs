using UnityEngine;

public class TileCreator : MonoBehaviour
{
    public enum TileDirection
    {
        FullTile,
        North,
        East,
        South,
        West
    }

    public static TileCreator tileCreator;
    private Sprite selectedBrush;
    private bool isResource = false;

    [SerializeField] private GameObject tilePrefab;

    private void Awake()
    {
        if (tileCreator == null)
        {
            tileCreator = this;
        }
        else
        {
            Destroy(tileCreator);
            tileCreator = this;
            Debug.LogWarning("Tile creator replaced, was this meant to happen?");
        }
    }

    public void SetSelection(Sprite selectedSprite, bool isResource)
    {
        selectedBrush = selectedSprite;
        this.isResource = isResource;
    }

    public void PlaceTile(TileDirection tileDirection, Vector3 tilePosition, bool isEditingTile)
    {
        Debug.Log($"Placing tile at {tilePosition} with direction of {tileDirection} and tile type of {selectedBrush}, which is {isResource} a resource");
        GameObject tile;
        if (!isEditingTile)
        {
            tile = Instantiate(tilePrefab, transform);
            tile.transform.localPosition = tilePosition;
            tile.name = $"{selectedBrush.name} Resource";
        }
        else
        {
            tile = GetTile(tilePosition);
        }

        switch (tileDirection)
        {
            case TileDirection.FullTile:
                SetTilePartSprite(transform.GetChild(0));
                SetTilePartSprite(transform.GetChild(1));
                SetTilePartSprite(transform.GetChild(2));
                SetTilePartSprite(transform.GetChild(3));
                break;
            case TileDirection.North:
                SetTilePartSprite(transform.GetChild(0));
                break;
            case TileDirection.East:
                SetTilePartSprite(transform.GetChild(1));
                break;
            case TileDirection.South:
                SetTilePartSprite(transform.GetChild(2));
                break;
            case TileDirection.West:
                SetTilePartSprite(transform.GetChild(3));
                break;
        }
    }

    private GameObject GetTile(Vector3 tilePosition)
    {
        Collider[] hits = Physics.OverlapBox(tilePosition, tilePosition);
        return hits[0].gameObject;
    }

    private void SetTilePartSprite(Transform transform)
    {
        transform.GetComponent<SpriteRenderer>().sprite = selectedBrush;
    }
}
