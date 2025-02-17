using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Scripts.Refactor
{
  public class TouchInputManager : IInputManager
  {
    public bool IsActive => Touch.activeTouches.Count > 0;

    public void Initialize()
    {
      if (!EnhancedTouchSupport.enabled)
      {
        EnhancedTouchSupport.Enable();
      }
    }

    public Vector2 GetInputPosition()
    {
      return Touch.activeTouches[0].screenPosition;
    }

    public void Update() { }

    public void Cleanup()
    {
      if (EnhancedTouchSupport.enabled)
      {
        EnhancedTouchSupport.Disable();
      }
    }
  }
}