using UnityEngine;

namespace Shears
{
    [CreateAssetMenu(fileName = "New Sprite Sheet 3x3", menuName = "2D/Sprites/Sprite Sheet 3x3", order = 10)]
    public class SpriteSheet3x3 : ScriptableObject
    {
        [field: SerializeField] public Sprite TopLeft { get; private set; }
        [field: SerializeField] public Sprite TopMiddle { get; private set; }
        [field: SerializeField] public Sprite TopRight { get; private set; }
        [field: SerializeField] public Sprite Left { get; private set; }
        [field: SerializeField] public Sprite Middle { get; private set; }
        [field: SerializeField] public Sprite Right { get; private set; }
        [field: SerializeField] public Sprite BottomLeft { get; private set; }
        [field: SerializeField] public Sprite BottomMiddle { get; private set; }
        [field: SerializeField] public Sprite BottomRight { get; private set; }
    }
}
