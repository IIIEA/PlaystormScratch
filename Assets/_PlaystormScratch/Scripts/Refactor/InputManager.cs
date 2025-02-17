using UnityEngine;

namespace Scripts.Refactor
{
    public interface IInputManager
    {
        bool IsActive { get; }
        Vector2 GetInputPosition();
        void Initialize();
        void Update();
        void Cleanup();
    }
}