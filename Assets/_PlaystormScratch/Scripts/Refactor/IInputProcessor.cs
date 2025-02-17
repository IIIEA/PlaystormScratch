using System;
using UnityEngine;

namespace Scripts.Refactor
{
  public interface IInputProcessor
  {
    event Action<Vector2> OnInputStart;
    event Action<Vector2> OnInputMove;
    event Action OnInputEnd;
    void Update();
  }
}