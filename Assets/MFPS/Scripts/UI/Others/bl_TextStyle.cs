using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace MFPS.Runtime.UI
{
    [AddComponentMenu("UI/Letter Spacing", 14)]
    public class bl_TextStyle : BaseMeshEffect
    {
        [SerializeField, Range(0, 100)]
        private float Spacing = 7f;
        private const string SupportedTagRegexPattersn = @"<b>|</b>|<i>|</i>|<size=.*?>|</size>|<color=.*?>|</color>|<material=.*?>|</material>";

        protected bl_TextStyle() { }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            spacing = Spacing;
            base.OnValidate();
        }
#endif
        private Text _text;
        private Text textComponent
        {
            get
            {
                if (_text == null) { _text = GetComponent<Text>(); }
                return _text;
            }
        }

        private string[] GetLines()
        {
            IList<UILineInfo> lineInfos = textComponent.cachedTextGenerator.lines;
            string[] lines = new string[lineInfos.Count];
            for (int i = 0; i < lineInfos.Count; i++)
            {
                if ((i + 1) < lineInfos.Count)
                {
                    int length = (lineInfos[i + 1].startCharIdx - 1) - lineInfos[i].startCharIdx;
                    lines[i] = this.textComponent.text.Substring(lineInfos[i].startCharIdx, length);
                }
                else
                {
                    lines[i] = this.textComponent.text.Substring(lineInfos[i].startCharIdx);
                }
            }
            return lines;
        }

        public float spacing
        {
            get { return Spacing; }
            set
            {
                if (Spacing == value) return;
                Spacing = value;
                if (graphic != null) graphic.SetVerticesDirty();
            }
        }

        public void ModifyVertices(List<UIVertex> verts)
        {
            if (!IsActive()) return;

            if (textComponent == null)
            {
                Debug.LogWarning("LetterSpacing: Missing Text component");
                return;
            }

            string[] lines = GetLines();

            Vector3 pos;
            float letterOffset = spacing * (float)textComponent.fontSize / 100f;
            float alignmentFactor = 0;
            int glyphIdx = 0;

            switch (textComponent.alignment)
            {
                case TextAnchor.LowerLeft:
                case TextAnchor.MiddleLeft:
                case TextAnchor.UpperLeft:
                    alignmentFactor = 0f;
                    break;

                case TextAnchor.LowerCenter:
                case TextAnchor.MiddleCenter:
                case TextAnchor.UpperCenter:
                    alignmentFactor = 0.5f;
                    break;

                case TextAnchor.LowerRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.UpperRight:
                    alignmentFactor = 1f;
                    break;
            }

            for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
            {
                //the real line
                string realLine = lines[lineIdx];
                string parsedLine = "";
                //line without rich text characters
                string unrichLine = WithoutRichText(realLine, out parsedLine);

                float lineOffset = (unrichLine.Length - 1) * letterOffset * alignmentFactor;

                int skipChar = 0;
                for (int charIdx = 0, charPositionIndex = 0; charIdx < realLine.Length; charIdx++, charPositionIndex++)
                {
#if UNITY_2019_OR_LATER
                    if (realLine[charIdx] == ' ') continue;
#else
                    if (parsedLine[charIdx] == '♯') { skipChar++; }
#endif
                    int idx1 = glyphIdx * 6;
                    int idx2 = glyphIdx * 6 + 1;
                    int idx3 = glyphIdx * 6 + 2;
                    int idx4 = glyphIdx * 6 + 3;
                    int idx5 = glyphIdx * 6 + 4;
                    int idx6 = glyphIdx * 6 + 5;

                    // Check for truncated text (doesn't generate verts for all characters)
                    if (idx6 > verts.Count - 1) { return; }

                    UIVertex vert1 = verts[idx1];
                    UIVertex vert2 = verts[idx2];
                    UIVertex vert3 = verts[idx3];
                    UIVertex vert4 = verts[idx4];
                    UIVertex vert5 = verts[idx5];
                    UIVertex vert6 = verts[idx6];

                    pos = Vector3.right * ((letterOffset * (charPositionIndex - skipChar)) - lineOffset);

                    vert1.position += pos;
                    vert2.position += pos;
                    vert3.position += pos;
                    vert4.position += pos;
                    vert5.position += pos;
                    vert6.position += pos;

                    verts[idx1] = vert1;
                    verts[idx2] = vert2;
                    verts[idx3] = vert3;
                    verts[idx4] = vert4;
                    verts[idx5] = vert5;
                    verts[idx6] = vert6;

#if !UNITY_2019_OR_LATER
                    glyphIdx++;
#endif
                }
#if !UNITY_2019_OR_LATER
                glyphIdx++;
#endif
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!this.IsActive())
                return;

            List<UIVertex> vertexList = new List<UIVertex>();
            vh.GetUIVertexStream(vertexList);

            ModifyVertices(vertexList);

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertexList);
        }

        private string WithoutRichText(string line, out string parseLine)
        {
            parseLine = Regex.Replace(line, SupportedTagRegexPattersn, m =>
            {
                return new string('♯', m.Groups[0].Value.Length);
            });
            line = Regex.Replace(line, SupportedTagRegexPattersn, "");
            return line;
        }
    }
}