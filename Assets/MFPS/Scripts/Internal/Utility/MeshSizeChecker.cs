using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MFPSEditor
{
    public class MeshSizeChecker : MonoBehaviour
    {
        public Bounds bounds;

        public void Check()
        {
            var meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (meshes == null || meshes.Length <= 0) return;

            bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var item in meshes)
            {
                bounds.Encapsulate(item.bounds);
            }
        }

        public float Height => bounds == null ? 1 : bounds.size.y;

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Check();
            if (bounds == null) return;

            GUI.skin.label.richText = true;
            Vector3 right = transform.TransformDirection(Vector3.right);

            var bottomRightSide = (bounds.center + (right * (bounds.extents.x + 0.1f))) + (Vector3.down * bounds.extents.y);
            var topRightSide = bottomRightSide + (Vector3.up * bounds.size.y);

            if (bounds.size.y >= 1.91f && bounds.size.y <= 2.24f) Gizmos.color = Color.green;
            else Gizmos.color = Color.yellow;

            Gizmos.DrawLine(bottomRightSide, topRightSide);
            Gizmos.DrawLine(bottomRightSide + (-right * 0.1f), bottomRightSide + (right * 0.1f));
            Gizmos.DrawLine(topRightSide + (-right * 0.1f), topRightSide + (right * 0.1f));
            Handles.Label(bottomRightSide + (Vector3.up * bounds.extents.y), $"  <color=yellow>Model Size\n  {bounds.size.y.ToString("0.00")}m</color>");

            Gizmos.color = Color.green;

            bottomRightSide = bottomRightSide + right * 0.3f;
            topRightSide = topRightSide + right * 0.3f;
            topRightSide.y = bottomRightSide.y + 2;
            Gizmos.DrawLine(bottomRightSide, topRightSide);
            Gizmos.DrawLine(bottomRightSide + (-right * 0.1f), bottomRightSide + (right * 0.1f));
            Gizmos.DrawLine(topRightSide + (-right * 0.1f), topRightSide + (right * 0.1f));

            Gizmos.color = Color.white;
#endif
        }
    }
}