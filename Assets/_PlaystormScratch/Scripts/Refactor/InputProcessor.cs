using System;
using UnityEngine;

namespace Scripts.Refactor
{
  public class InputProcessor : IInputProcessor
  {
    public event Action<Vector2> OnInputStart;
    public event Action<Vector2> OnInputMove;
    public event Action OnInputEnd;

    private readonly IInputManager[] _inputManagers;
    private Vector2 _lastPosition;
    private bool _wasActive;

    public InputProcessor()
    {
      _inputManagers = new IInputManager[]
      {
        new TouchInputManager(),
        new MouseInputManager()
      };

      foreach (var manager in _inputManagers)
      {
        manager.Initialize();
      }
    }

    public void Update()
    {
      foreach (var manager in _inputManagers)
      {
        manager.Update();

        if (manager.IsActive)
        {
          ProcessInput(manager);
          return;
        }
      }

      if (_wasActive)
      {
        OnInputEnd?.Invoke();
        _wasActive = false;
      }
    }

    private void ProcessInput(IInputManager manager)
    {
      var currentPosition = manager.GetInputPosition();

      if (!_wasActive)
      {
        OnInputStart?.Invoke(currentPosition);
        _wasActive = true;
      }
      else if (currentPosition != _lastPosition)
      {
        OnInputMove?.Invoke(currentPosition);
      }

      _lastPosition = currentPosition;
    }
  }
}