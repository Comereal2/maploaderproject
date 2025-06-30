using UnityEngine;

public class TileCreator : MonoBehaviour
{
    public enum TileDirection : int
    {
        FullTile = 5,
        North = 0,
        East = 1,
        South = 2,
        West = 3
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
        if (selectedBrush == null) return;
        GameObject tile;
        if (!isEditingTile)
        {
            tile = CreateEmptyTile();
            tile.transform.localPosition = tilePosition;
        }
        else
        {
            tile = GetTile(tilePosition);
        }

        switch (tileDirection)
        {
            case TileDirection.FullTile:
                SetTileSprite(tile.transform);
                break;
            case TileDirection.North:
                SetTilePartSprite(tile.transform.GetChild(0), tileDirection);
                break;
            case TileDirection.East:
                SetTilePartSprite(tile.transform.GetChild(1), tileDirection);
                break;
            case TileDirection.South:
                SetTilePartSprite(tile.transform.GetChild(2), tileDirection);
                break;
            case TileDirection.West:
                SetTilePartSprite(tile.transform.GetChild(3), tileDirection);
                break;
        }
    }

    private GameObject CreateEmptyTile()
    {
        GameObject tile = Instantiate(tilePrefab, transform);
        tile.name = $"{selectedBrush.name} Tile";
        SetTileSprite(tile.transform, "Terrain0");
        return tile;
    }

    private GameObject GetTile(Vector3 tilePosition)
    {
        Collider[] hits = Physics.OverlapBox(tilePosition, tilePosition);
        return hits[0].gameObject;
    }

    private void SetTileSprite(Transform parentTransform)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>($"Tiles/{selectedBrush.name}");
        for(int i = 0; i < 4; i++)
        {
            parentTransform.GetChild(i).GetComponent<SpriteRenderer>().sprite = sprites[i];
        }
    }

    private void SetTileSprite(Transform parentTransform, string brush)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>($"Tiles/{brush}");
        for (int i = 0; i < 4; i++)
        {
            parentTransform.GetChild(i).GetComponent<SpriteRenderer>().sprite = sprites[i];
        }
    }

    private void SetTilePartSprite(Transform transform, TileDirection terrainRotation)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>($"Tiles/{selectedBrush.name}");
        transform.GetComponent<SpriteRenderer>().sprite = sprites[(int)terrainRotation];
    }
}