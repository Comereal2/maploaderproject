using UnityEngine;
using static TileCreator;

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
        if (isResource)
        {
            tile = GetTile(tilePosition);
            if(tile == null)
            {
                tile = CreateEmptyTile();
                tile.transform.localPosition = tilePosition;
            }
            SetResourceSprite(tile.transform);
            return;
        }
        if (!isEditingTile)
        {
            tile = CreateEmptyTile();
            tile.transform.localPosition = tilePosition;
        }
        else
        {
            tile = GetTile(tilePosition);
        }

        if(tileDirection == TileDirection.FullTile)
        {
            SetTileSprite(tile.transform);
        }
        else
        {
            SetTilePartSprite(tile.transform, tileDirection);
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
        if(hits.Length > 0)
        {
            return hits[0].gameObject;
        }
        return null;
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

    private void SetTilePartSprite(Transform parentTransform, TileDirection terrainRotation)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>($"Tiles/{selectedBrush.name}");
        switch (terrainRotation)
        {
            case TileDirection.North:
                parentTransform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sprites[(int)terrainRotation];
                break;
            case TileDirection.East:
                parentTransform.GetChild(1).GetComponent<SpriteRenderer>().sprite = sprites[(int)terrainRotation];
                break;
            case TileDirection.South:
                parentTransform.GetChild(2).GetComponent<SpriteRenderer>().sprite = sprites[(int)terrainRotation];
                break;
            case TileDirection.West:
                parentTransform.GetChild(3).GetComponent<SpriteRenderer>().sprite = sprites[(int)terrainRotation];
                break;
        }
    }

    private void SetResourceSprite(Transform parentTransform)
    {
        parentTransform.GetChild(4).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Resources/{selectedBrush.name}");
    }
}