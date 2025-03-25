using TMPro;
using UnityEngine;

namespace Shears.UI
{
    public class ManagedText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMesh;

        public string Text { get => textMesh.text; set => textMesh.text = value; }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}
