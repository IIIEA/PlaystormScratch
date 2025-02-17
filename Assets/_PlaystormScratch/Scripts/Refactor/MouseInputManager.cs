using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Refactor
{
  public class MouseInputManager : IInputManager
  {
    public bool IsActive => Mouse.current != null && Mouse.current.leftButton.isPressed;

    public void Initialize() { }

    public Vector2 GetInputPosition()
    {
      return Mouse.current.position.ReadValue();
    }

    public void Update() { }

    public void Cleanup() { }
  }
}