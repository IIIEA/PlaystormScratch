using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace _PlaystormScratch.InputData
{
  public class PlayerInputs
  {
    private const int MAX_TOUCH_COUNT = 10;
    private const int RESERVE_TOUCH_COUNT = 1;

    private Vector2 _scratchPosition;
    
    private readonly Vector2[] _startInputData;
    private readonly Vector2[] _endInputData;
    private readonly Vector2?[] _previousPosition;
    private readonly bool[] _isInputsPressed;
    private readonly bool[] _isStartPosition;

    private bool IsScratching => _isInputsPressed != null && _isInputsPressed.Any(isPressed => isPressed);
    
    public event ScratchHandler OnScratch;
    public event Action<Vector2> OnScratchHole;
    public event Action<Vector2, Vector2> OnScratchLine;
    public delegate Vector2 ScratchHandler(Vector2 position);


    public PlayerInputs()
    {
      _isInputsPressed = new bool[MAX_TOUCH_COUNT + RESERVE_TOUCH_COUNT];
      _isStartPosition = new bool[MAX_TOUCH_COUNT + RESERVE_TOUCH_COUNT];
      _startInputData = new Vector2[MAX_TOUCH_COUNT + RESERVE_TOUCH_COUNT];
      _endInputData = new Vector2[MAX_TOUCH_COUNT + RESERVE_TOUCH_COUNT];
      _previousPosition = new Vector2?[MAX_TOUCH_COUNT + RESERVE_TOUCH_COUNT];
      
      for (var i = 0; i < _isStartPosition.Length; i++)
      {
        _isStartPosition[i] = true;
      }

      if (!EnhancedTouchSupport.enabled)
      {
        EnhancedTouchSupport.Enable();
      }
    }

    public bool TryUpdate()
    {
      if (Touchscreen.current != null && Touch.activeTouches.Count > 0)
      {
        foreach (var touch in Touch.activeTouches)
        {
          var fingerId = touch.finger.index;
          
          if (fingerId >= MAX_TOUCH_COUNT)
            continue;
          
          if (touch.phase == TouchPhase.Began)
          {
            _isInputsPressed[fingerId] = false;
            _isStartPosition[fingerId] = true;
          }

          if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
          {
            SetInputData(fingerId, touch.screenPosition);
          }

          if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
          {
            _isInputsPressed[fingerId] = false;
            _previousPosition[fingerId] = null;
          }

          Scratch();
        }
      }
      else if (Mouse.current != null)
      {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
          _isInputsPressed[0] = false;
          _isStartPosition[0] = true;
        }

        if (Mouse.current.leftButton.isPressed)
        {
          SetInputData(0, Mouse.current.position.ReadValue());
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
          _isInputsPressed[0] = false;
          _previousPosition[0] = null;
        }

        Scratch();
      }

      return IsScratching;
    }

    private void SetInputData(int fingerId, Vector2 position)
    {
      if (OnScratch != null)
      {
        _scratchPosition = OnScratch(position);
      }

      if (_isStartPosition[fingerId])
      {
        _startInputData[fingerId] = _scratchPosition;
        _endInputData[fingerId] = _startInputData[fingerId];
        _isStartPosition[fingerId] = !_isStartPosition[fingerId];
      }
      else
      {
        _startInputData[fingerId] = _endInputData[fingerId];
        _endInputData[fingerId] = _scratchPosition;
      }

      if (!_isInputsPressed[fingerId])
      {
        _endInputData[fingerId] = _startInputData[fingerId];
        _isInputsPressed[fingerId] = true;
      }
    }

    private void Scratch()
    {
      for (var i = 0; i < _isInputsPressed.Length; i++)
      {
        if (_isInputsPressed[i])
        {
          if (_startInputData[i] == _endInputData[i])
          {
            OnScratchHole?.Invoke(_endInputData[i]);
          }
          else
          {
            OnScratchLine?.Invoke(_startInputData[i],
              _endInputData[i]);
          }
        }
      }
    }
  }
}