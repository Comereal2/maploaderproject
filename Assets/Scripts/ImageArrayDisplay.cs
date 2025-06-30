using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ImageArrayDisplay : MonoBehaviour
{
    [Header("Image information")]
    [SerializeField] private float imageScale = 1f;
    [SerializeField] private float imageOffset = 10f;
    [SerializeField] private int imageAmountPerRow;
    [SerializeField] private Sprite outlineSprite;
    [SerializeField] private bool isHighlightable = true;
    [SerializeField] private Color highlightColor = new(255f, 255f, 255f, 0.1f);

    /*[Header("Image border settings")]
    [SerializeField] private Color borderColor = Color.black;
    [SerializeField] private float borderThickness = 1f;*/

#if UNITY_EDITOR
    [Header("Folder from Assets/Resources/...")]
    [SerializeField] private DefaultAsset folderReference;
#endif

    [Header("Retrieved folder path, do not edit")]
    [SerializeField] private string resourcesPath;

    private void Start()
    {
        if (resourcesPath != null)
        {
            Sprite[] files = Resources.LoadAll<Sprite>(resourcesPath);
            files = files.OrderBy(s => ExtractNumber(s.name)).ToArray();
            int currentRow = 0;
            int currentCol = 0;
            for (int i = 0; i < files.Length; i++)
            {
                Sprite sprite = files[i];

                if(currentCol >= imageAmountPerRow)
                {
                    currentCol = 0;
                    currentRow++;
                }

                Image image = CreateImage(sprite.name, transform);
                image.sprite = sprite;

                if (isHighlightable)
                {
                    Image panel = CreateImage("HighlightPanel", image.transform);
                    panel.color = highlightColor;
                    image.gameObject.AddComponent<SpriteHighlighter>();
                }

                RectTransform imageRect = image.gameObject.GetComponent<RectTransform>();
                image.transform.localScale = new Vector3(imageScale, imageScale, 1f);
                image.transform.position = new Vector3(transform.position.x + currentCol * (imageRect.rect.width * imageScale + imageOffset), transform.position.y - currentRow * (imageRect.rect.height * imageScale + imageOffset), transform.position.z);

                GameObject outlineObject = new GameObject("Outline");
                outlineObject.transform.SetParent(image.transform);

                Image outline = outlineObject.AddComponent<Image>();
                outline.type = Image.Type.Sliced;
                outline.sprite = outlineSprite;
                outlineObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
                outlineObject.GetComponent<RectTransform>().localScale = new Vector2(1.2f, 1.2f);

                currentCol++;
            }
        }
        else
        {
            Debug.LogWarning("No folder selected for the image array.");
        }
    }

    private int ExtractNumber(string name)
    {
        var match = Regex.Match(name, @"(\d+)$");
        if (match.Success)
        {
            return int.Parse(match.Value);
        }
        return int.MaxValue;
    }

    private Image CreateImage(string name, Transform parent)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent);
        imageObject.transform.localPosition = Vector3.zero;
        return imageObject.AddComponent<Image>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (folderReference == null) return;

        string fullPath = AssetDatabase.GetAssetPath(folderReference);

        int resourcesIndex = fullPath.IndexOf("Resources/");
        if (resourcesIndex >= 0)
        {
            string relPath = fullPath.Substring(resourcesIndex + "Resources/".Length);
            relPath = relPath.TrimEnd('/');
            resourcesPath = relPath;
        }
        else
        {
            Debug.LogWarning("The folder must be inside a Resources folder.");
        }
    }
#endif
}
