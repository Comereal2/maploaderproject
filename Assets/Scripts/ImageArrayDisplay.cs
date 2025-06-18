using System.IO;
using UnityEditor;
using UnityEngine;

public class ImageArrayDisplay : MonoBehaviour
{
    [SerializeField] private DefaultAsset arrayDirectory;
    [SerializeField] private int imageAmountPerRow;

    private void Start()
    {
        if (arrayDirectory != null)
        {
            string path = AssetDatabase.GetAssetPath(arrayDirectory);
            string[] files = Directory.GetFiles(path, "*.*");
            int currentRow = 0;
            int currentCol = 0;
            for (int i = 0; i < files.Length; i++)
            {
                byte[] fileData = File.ReadAllBytes(files[i]);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                currentCol++;
                if (currentCol < imageAmountPerRow)
                {
                    GameObject image = new GameObject();
                    image.AddComponent<SpriteRenderer>().sprite = sprite;
                    image.name = sprite.name;
                    image.transform.position = new Vector3(transform.position.x + currentCol * (texture.width + 5), transform.position.y + currentRow * (texture.height + 5), transform.position.z);
                }
                else
                {
                    currentCol = 0;
                    currentRow++;
                }
            }
        }
        else
        {
            Debug.LogWarning("No folder selected for the image array.");
        }
    }
}
