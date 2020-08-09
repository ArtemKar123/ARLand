using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Painter : MonoBehaviour
{
    public double[] X = new[] { 50.0, 50, 450, 450};//{80.0, 120, 280, 490, 470, 320, 80}; //{ 5.0, 7, 10, 25, 30, 30, 35 };
    public double[] Y = new[] { 50.0, 450, 450, 50};
    public Terrain terrain;
    [System.Serializable]
    public class SplatHeights
    {
        public int textureIndex;
        public float startingHeight;
        public int overlap;
    }

    public SplatHeights[] splatHeights;

    void normalize(float[] v)
    {
        float total = 0;
        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }

        for (int i = 0; i < v.Length; i++)
        {
            v[i] /= total;
        }
    }

    public float map(float value, float sMin, float sMax, float mMin, float mMax)
    {
        return (value - sMin) * (mMax - mMin) / (sMax - sMin) + mMin;
    }

    public void Paint()
    {
        TerrainData terrainData = terrain.terrainData;
        float[,,] splatmapData = new float[terrainData.alphamapWidth,
            terrainData.alphamapHeight, terrainData.alphamapLayers];
        Debug.Log(terrainData.alphamapHeight + " " + terrainData.alphamapWidth + " " + terrainData.alphamapLayers);
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float terrainHeight = terrainData.GetHeight(y, x); //(y,x)

                var ins = inside(x, y, X.Length, X, Y);
                float[] splat = new float[splatHeights.Length];
                if (ins == 0)
                {
                    splat[0] = 1;
                }
                else
                {
                    for (int i = 1; i < splatHeights.Length; i++)
                    {
                        float thisNoise = map(Mathf.PerlinNoise(x * 0.03f, y * 0.03f), 0, 1, 0.5f, 1);
                        float thisHeightStart = splatHeights[i].startingHeight * thisNoise -
                                                splatHeights[i].overlap * thisNoise;
                        float nextHeightStart = 0;
                        if (i != splatHeights.Length - 1)
                            nextHeightStart = splatHeights[i + 1].startingHeight * thisNoise +
                                              splatHeights[i + 1].overlap * thisNoise;
                        if (i == splatHeights.Length - 1 && terrainHeight >= thisHeightStart)
                            splat[i] = 1;
                        else if (terrainHeight >= thisHeightStart && terrainHeight <= nextHeightStart) //
                            splat[i] = 1;
                    }
                    normalize(splat);
                }

                for (int j = 0; j < splatHeights.Length; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }
        }

        Debug.Log(terrainData.GetHeight(128, 128));
        Debug.Log(terrainData.GetHeight(5, 5));
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    int inside(double px, double py, int n, double[] x, double[] y)
    {
        int i, j, s;
        s = 0;
        j = n - 1;
        for (i = 0; i < n; j = i++)
        {
            if ((py < y[i] ^ py < y[j]) || py == y[i] || py == y[j])
            {
                if ((px < x[i] ^ px < x[j]) || px == x[i] || px == x[j])
                {
                    if (y[i] < y[j] ^ (x[i] - px) * (y[j] - py) > (y[i] - py) * (x[j] - px)) s ^= 1;
                }
                else
                {
                    if (px > x[i] && y[i] != y[j]) s ^= 1;
                }
            }
        }

        return s;
    }

    // Start is called before the first frame update
    void Start()
    {
        Paint();
    }
}