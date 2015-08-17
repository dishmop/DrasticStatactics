using UnityEngine;
using System.Collections;

public class TerrainController : MonoBehaviour {

    TerrainData terrainData;
    float[, ,] originalAlphas;
    float pE = 5.0f;
    bool pathDrawn = false;

	void Start () {
        terrainData = Terrain.activeTerrain.terrainData;
        originalAlphas = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        pE = Terrain.activeTerrain.heightmapPixelError;
	}

    void LateUpdate()
    {
        /*if (!pathDrawn)
        {
            DrawPath();
        }*/
    }

    void OnDestroy()
    {
        terrainData.SetAlphamaps(0, 0, originalAlphas);
    }

    void DrawPath()
    {
        float[, ,] alphas = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        /*//Draw a debug coordinate indicator
        for (int y = 245; y <= 255; y++)
        {
            for (int x = 245; x <= 255; x++)
            {
                AddTexture(ref alphas, x, y, 1, 1.0f);
            }
        }*/
        foreach (PathPoint pathPoint in GameObject.FindObjectsOfType<PathPoint>())
        {
            Vector2 start = new Vector2(pathPoint.transform.position.z + pE, pathPoint.transform.position.x + pE + 1.0f),
                displacement, position;
            float increment = 1.0f;
            int layer = Mathf.Max(0, Mathf.Min(terrainData.alphamapLayers, 1));
            AddTexture(ref alphas, start.x, start.y, layer, 0.2f);
            foreach (PathPoint nextPathPoint in pathPoint.nextPathPoints)
            {
                displacement
                    = new Vector2(nextPathPoint.transform.position.z + pE, nextPathPoint.transform.position.x + pE + 1.0f) - start;
                if (displacement != Vector2.zero)
                {
                    increment = 1.0f / displacement.magnitude;
                    for (float l = 1.0f; l >= 0.0f; l -= increment)
                    {
                        position = start + displacement * l;
                        if (position.x >= 1 && position.x <= terrainData.alphamapWidth - 1
                            && position.y >= 1 && position.y <= terrainData.alphamapHeight - 1)
                        {
                            for (int otherLayer = terrainData.alphamapLayers - 1; otherLayer >= 0; otherLayer--)
                            {
                                AddTexture(ref alphas, position.x, position.y, layer, 0.2f);
                            }
                        }
                    }
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, alphas);
        pathDrawn = true;
    }

    void AddTexture(ref float[, ,] alphas, float x, float y, int textureIndex, float addition = 0.5f)
    {
        int x1 = Mathf.FloorToInt(x);
        int x2 = Mathf.CeilToInt(x);
        int y1 = Mathf.FloorToInt(y);
        int y2 = Mathf.CeilToInt(y);
        float strength;
        //1,1
        strength = Mathf.Max(0.0f, Mathf.Min(1.0f, alphas[x1, y1, textureIndex]
            + addition * ((float)x2 - x) * ((float)y2 - y)));
        SetTexture(ref alphas, x1, y1, textureIndex, strength);
        //1,2
        strength = Mathf.Max(0.0f, Mathf.Min(1.0f, alphas[x1, y2, textureIndex]
            + addition * ((float)x2 - x) * (y - (float)y1)));
        SetTexture(ref alphas, x1, y2, textureIndex, strength);
        //2,1
        strength = Mathf.Max(0.0f, Mathf.Min(1.0f, alphas[x2, y1, textureIndex]
            + addition * (x - (float)x1) * ((float)y2 - y)));
        SetTexture(ref alphas, x2, y1, textureIndex, strength);
        //2,2
        strength = Mathf.Max(0.0f, Mathf.Min(1.0f, alphas[x2, y2, textureIndex]
            + addition * (x - (float)x1) * (y - (float)y1)));
        SetTexture(ref alphas, x2, y2, textureIndex, strength);
    }

    void AddTexture(ref float[, ,] alphas, int x, int y, int textureIndex, float addition = 0.5f)
    {
        float strength = Mathf.Max(0.0f, Mathf.Min(1.0f, alphas[x, y, textureIndex] + addition));
        SetTexture(ref alphas, x, y, textureIndex, strength);
    }
    void SetTexture(ref float[, ,] alphas, int x, int y, int textureIndex, float strength = 1.0f)
    {
        if (x >= 0 && x <= terrainData.alphamapWidth && y >= 0 && y <= terrainData.alphamapHeight
            && textureIndex >= 0 && textureIndex < terrainData.alphamapLayers)
        {
            if (strength != 1.0f)
            {
                float preTotal = 0.0f, postTotal = 1.0f;
                for (int layer = terrainData.alphamapLayers - 1; layer >= 0; layer--)
                {
                    if (layer == textureIndex)
                    {
                        postTotal = 1.0f - strength;
                        alphas[x, y, layer] = strength;
                    }
                    else
                    {
                        preTotal += alphas[x, y, layer];
                    }
                }
                for (int layer = terrainData.alphamapLayers - 1; layer >= 0; layer--)
                {
                    if (layer != textureIndex)
                    {
                        alphas[x, y, layer] *= postTotal / preTotal;
                    }
                }
            }
            else
            {
                for (int layer = terrainData.alphamapLayers - 1; layer >= 0; layer--)
                {
                    if (layer == textureIndex)
                    {
                        alphas[x, y, layer] = 1.0f;
                    }
                    else
                    {
                        alphas[x, y, layer] = 0.0f;
                    }
                }
            }
        }
    }
}
