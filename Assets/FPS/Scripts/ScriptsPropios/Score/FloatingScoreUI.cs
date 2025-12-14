using TMPro;
using UnityEngine;

namespace Unity.FPS.ours
{
    public class FloatingScoreUI : MonoBehaviour
    {
        public TextMeshProUGUI tmpText;
        public float moveSpeed = 50f;        
        public float disappearTime = 1.0f;     

        private float timer;
        private Color originalColor;

        public void Setup(int amount, bool isGain)
        {
            if (tmpText == null) return;

            if (isGain)
            {
                tmpText.text = "+" + amount;
                tmpText.color = Color.green;
            }
            else
            {
                tmpText.text = "-" + amount;
                tmpText.color = Color.red;
            }

            originalColor = tmpText.color;
            timer = 0;
        }

        void Update()
        {
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            if (timer >= disappearTime)
            {
                Destroy(gameObject);
            }
            else if (tmpText != null)
            {
                float alpha = 1 - (timer / disappearTime);
                tmpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
        }
    }
}