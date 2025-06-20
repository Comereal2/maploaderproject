using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ImageArrayDisplay : MonoBehaviour
{
    [SerializeField] private Material imageOutlineMaterial;
    [SerializeField] private int imageAmountPerRow;

    [Header("Image border settings")]
    [SerializeField] private Color borderColor = Color.black;
    [SerializeField] private float borderThickness = 1f;

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
            imageOutlineMaterial.SetColor("_OutlineColor", borderColor);
            imageOutlineMaterial.SetFloat("_OutlineThickness", borderThickness);
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

                GameObject imageObject = new GameObject(sprite.name);
                imageObject.transform.SetParent(transform);
                imageObject.transform.position = new Vector3(transform.position.x + currentCol * (sprite.rect.width + 5), transform.position.y - currentRow * (sprite.rect.height + 5), transform.position.z);
                
                Image image = imageObject.AddComponent<Image>();
                image.sprite = sprite;
                image.material = imageOutlineMaterial;

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
