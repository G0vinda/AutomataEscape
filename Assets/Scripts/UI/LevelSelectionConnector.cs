using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LevelSelectionConnector : MonoBehaviour
    {
        [SerializeField] private Image levelSelectionConnectionPrefab;
        [SerializeField] private Color disabledConnectionColor;

        public void CreateConnection(Vector2 from, Vector2 to, bool isEnabled)
        {
            var path = to - from;
            var connectionAngle = Vector2.SignedAngle(Vector2.up, path);

            var newConnection = Instantiate(levelSelectionConnectionPrefab, from, Quaternion.Euler(0, 0, connectionAngle),
                transform);

            var connectionSize = newConnection.rectTransform.sizeDelta;
            connectionSize.y = path.magnitude;
            newConnection.rectTransform.sizeDelta = connectionSize;

            if (!isEnabled)
                newConnection.color = disabledConnectionColor;
        }
    }
}
