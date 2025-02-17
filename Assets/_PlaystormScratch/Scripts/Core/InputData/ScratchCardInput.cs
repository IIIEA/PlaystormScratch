using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Scripts.Core.InputData
{
  public class ScratchCardInput
  {
    public event ScratchHandler OnScratch;
    public event Action<Vector2> OnScratchHole;
    public event Action<Vector2, Vector2> OnScratchLine;

    public delegate Vector2 ScratchHandler(Vector2 position);

    private bool IsScratching => _isScratching != null && _isScratching.Any(scratching => scratching);

    private Vector2 _scratchPosition;
    
    private readonly Vector2[] _startInputData;
    private readonly Vector2[] _endInputData;
    private readonly Vector2?[] _previousScratchPosition;
    private readonly bool[] _isScratching;
    private readonly bool[] _isStartPosition;

    private const int MAX_TOUCH_COUNT = 10;
    private const int RESERVE_TOUCH_COUNT = 1;

    public ScratchCardInput()
    {
      _isScratching = new bool[MAX_TOUCH_COUNT + RESERVE_TOUCH_COUNT];
      _isStartPosition = new bool[MAX_TOUCH_COUNT + RESERVE_TOUCH_COUNT];
      _startInputData = new Vector2[MAX_TOUCH_COUNT + RESERVE_TOUCH_COUNT];
      _endInputData = new Vector2[MAX_TOUCH_COUNT + RESERVE_TOUCH_COUNT];
      _previousScratchPosition = new Vector2?[MAX_TOUCH_COUNT + RESERVE_TOUCH_COUNT];
      
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
            _isScratching[fingerId] = false;
            _isStartPosition[fingerId] = true;
          }

          if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
          {
            SetInputData(fingerId, touch.screenPosition);
          }

          if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
          {
            _isScratching[fingerId] = false;
            _previousScratchPosition[fingerId] = null;
          }

          Scratch();
        }
      }
      else if (Mouse.current != null)
      {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
          _isScratching[0] = false;
          _isStartPosition[0] = true;
        }

        if (Mouse.current.leftButton.isPressed)
        {
          SetInputData(0, Mouse.current.position.ReadValue());
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
          _isScratching[0] = false;
          _previousScratchPosition[0] = null;
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

      if (!_isScratching[fingerId])
      {
        _endInputData[fingerId] = _startInputData[fingerId];
        _isScratching[fingerId] = true;
      }
    }

    private void Scratch()
    {
      for (var i = 0; i < _isScratching.Length; i++)
      {
        if (_isScratching[i])
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