using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Arms Material", menuName = "MFPS/Player/Arm Material")]
public class bl_FPArmsMaterial : ScriptableObject
{
    public string materialColorPropertyName = "_Color";
    public TeamMaterial[] ArmsMaterials;

    public void SelectTeamMaterial(Team playerTeam)
    {
        for (int i = 0; i < ArmsMaterials.Length; i++)
        {
            if (ArmsMaterials[i].Material == null) continue;
            ArmsMaterials[i].Material.mainTexture = playerTeam == Team.Team1 ? ArmsMaterials[i].Team1Texture : ArmsMaterials[i].Team2Texture;
        }
    }

    [Serializable]
    public class TeamMaterial
    {
        public Material Material;
        public Texture2D Team1Texture;
        public Texture2D Team2Texture;
    }

    public class MaterialColor
    {
        public Material Material;
        public Color Color;

        public MaterialColor(Material mat)
        {
            if (mat == null) return;
            Material = mat;
            Color = mat.color;
        }
    }
}