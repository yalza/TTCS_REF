using UnityEngine;

namespace MFPS.Tween
{
    public interface ITween
    {
        void StartTween();
        void StartReverseTween(bool desactive);
    }

    public static class TweenHelper
    {
        public static void SetActiveTween(this GameObject go, bool active)
        {
            if (go == null)
                return;

            ITween tween = go.GetComponent<ITween>();
            if (tween == null)
            {
                go.SetActive(active);
            }
            else
            {
                if (active) { go.SetActive(true); tween.StartTween(); } else { if (go.activeInHierarchy) tween.StartReverseTween(true); }
            }
        }
    }
}