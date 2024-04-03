using MFPSEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_TerrainSurfaces : MonoBehaviour
{
    public List<TerrainSurface> terrainSurfaces = new List<TerrainSurface>();
	[Space]
    public TerrainLayer[] terrainLayers = new TerrainLayer[0];
    public Terrain terrain;

	public string GetSurfaceTag(Vector3 position)
    {
		int layerID = GetMainTexture(position, terrain);
		if (layerID >= terrainSurfaces.Count) return string.Empty;

		return terrainSurfaces[layerID].Tag;
    }

	public static float[] GetTextureMix(Vector3 worldPos, Terrain terrain)
	{
		// returns an array containing the relative mix of textures
		// on the main terrain at this world position.
		// The number of values in the array will equal the number
		// of textures added to the terrain.
		TerrainData terrainData = terrain.terrainData;
		Vector3 terrainPos = terrain.transform.position;

		// calculate which splat map cell the worldPos falls within (ignoring y)
		int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
		int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

		// get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
		float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

		// extract the 3D array data to a 1D array:
		float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

		for (int n = 0; n < cellMix.Length; ++n)
		{
			cellMix[n] = splatmapData[0, 0, n];
		}

		return cellMix;
	}

	public static int GetMainTexture(Vector3 worldPos, Terrain terrain)
	{
		// returns the zero-based index of the most dominant texture
		// on the main terrain at this world position.
		float[] mix = GetTextureMix(worldPos, terrain);
		float maxMix = 0;
		int maxIndex = 0;

		// loop through each mix value and find the maximum
		for (int n = 0; n < mix.Length; ++n)
		{
			if (mix[n] > maxMix)
			{
				maxIndex = n;
				maxMix = mix[n];
			}
		}

		return maxIndex;
	}


	private void OnValidate()
    {
        if (terrain == null) terrain = GetComponent<Terrain>();
        if (terrain == null) return;

		terrainLayers = terrain.terrainData.terrainLayers;
        for (int i = 0; i < terrainLayers.Length; i++)
        {
            if (!terrainSurfaces.Exists(x => x.terrainLayer == terrainLayers[i]))
            {
                terrainSurfaces.Add(new TerrainSurface()
                {
                    terrainLayer = terrainLayers[i],
                    MainTexture = terrainLayers[i].diffuseTexture,
                    Tag = "Generic",
                });
            }
        }
    }

    [System.Serializable]
    public class TerrainSurface
    {
        public string Tag;
        [SpritePreview] public Texture2D MainTexture;
        public TerrainLayer terrainLayer;
    }
}