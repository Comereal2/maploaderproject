using UnityEngine;
using UnityEngine.InputSystem;

public class MapCreatorInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputMap;

    private void OnEnable()
    {
        inputMap.Enable();
    }

    private void OnDisable()
    {
        inputMap.Disable();
    }
}
