using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCreator : MonoBehaviour
{
    public static TileCreator tileCreator;
    public int tileHeight;
    private Sprite selectedBrush;
    private bool isResource = false;
    private bool isPlacing = false;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) isPlacing = true;

        if (isPlacing)
        {
            //Shoot a ray to y = height and place tile there
        }

        if (Input.GetKeyUp(KeyCode.Mouse1)) isPlacing = false;
    }

    public void SetSelection(Sprite selectedSprite, bool isResource)
    {
        selectedBrush = selectedSprite;
        this.isResource = isResource;
    }
}
