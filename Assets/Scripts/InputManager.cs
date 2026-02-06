using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // Always get InputMaster in Other Scripts StartMethod
    public InputMaster Input { get; private set; }

    void Awake()
    {
        Input = new InputMaster();
        Input.Enable();
    }

    private void OnEnable()
    {
        Input.Enable();
    }

    private void OnDisable()
    {
        Input.Disable();
    }

    private void OnDestroy()
    {
        Input.Dispose();
    }
}