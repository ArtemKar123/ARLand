using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Generation settings")] [Range(0.0f, 1)]
    private float _coefficient;

    private float R = 1, RY = 1; // Коэффициент скалистости
    private int GRAIN = 0; // Коэффициент зернистости
    public bool FLAT = false; // Делать ли равнины
    public Material material;
    private Painter painter;
    public int width = 256;
    public int height = 256;
    private float WH;
    private Color32[] cols, other_cols;
    private Texture2D texture;
    public double[] X = new[] { 50.0, 50, 450, 450};//{80.0, 120, 280, 490, 470, 320, 80}; //{ 5.0, 7, 10, 25, 30, 30, 35 };
    public double[] Y = new[] { 50.0, 450, 450, 50};//{80.0, 280, 360, 400, 200, 80, 80}; //{ 15.0, 25, 10, 20, 23, 40, 35 };
    public bool crop = false;
    public int crossfadeCoef = 8;
    private double[] InnerX = new[] {70.0, 70, 90, 90};
    private double[] InnerY = new[] {70.0, 90, 90, 70};

    public int zoneCount = 8;

    private int[,] zones = new int[,] //0-равнина, 1-горы, 2-плоскость(под домики)
    {
        /*x=0;x=1*/
        /*y=0*/ {1, 2, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        /*y=1*/ {0, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {1, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
    };

    public float[] Rs = new[] {0.2f, 0.4f, 0f};
    private int partialWidth;

    public void Start()
    {
        Debug.Log("Start " + zoneCount + " " + width);
        int resolution = width;
        WH = (float) width + height;
        partialWidth = width / zoneCount;
        // Задаём карту высот
        Terrain terrain = FindObjectOfType<Terrain>();
        float[,] heights = new float[resolution, resolution];

        // Создаём карту высот
        texture = new Texture2D(width, height);
        cols = new Color32[width * height];
        other_cols = new Color32[width * height];
        //drawPlasma(width, height);
        _coefficient = 0.5f;
        for (int y = 0; y < zoneCount; y++)
        {
            for (int x = 0; x < zoneCount; x++)
            {
                switch (zones[y, x])
                {
                    case 0:
                        GRAIN = 2;
                        break;
                    case 1:
                        GRAIN = 8;
                        break;
                }

                Debug.Log(x + " " + y + " Grain " + GRAIN);
                drawPlasma(partialWidth, partialWidth, y * partialWidth, x * partialWidth);
            }
        }

        /*GRAIN = 2;
        drawPlasma(64,64, 0, 64);*/
        texture.SetPixels32(other_cols);
        texture.Apply();

        // Используем шейдер (смотри пункт 3 во 2 части)
        material.SetTexture("HeightTex", texture);
        R = 0.4f;
        // Задаём высоту вершинам по карте высот
        float a = 129 % partialWidth;
        Debug.Log(a + " PARTIAL " + partialWidth);
        for (int i = 0; i < resolution; i++) //y
        {
            for (int k = 0; k < resolution; k++) //x
            {
                int zone_x = k / partialWidth;
                int zone_y = i / partialWidth;
                float curR = 0f, nextR = 0f, prevR = 0f, nextRY = 0f, prevRY = 0f;
                curR = Rs[zones[zone_y, zone_x]];
                if (zone_x < 15)
                {
                    nextR = Rs[zones[zone_y, zone_x + 1]];
                }
                else
                {
                    nextR = curR;
                }
                if (zone_x > 0)
                {
                    prevR = Rs[zones[zone_y, zone_x - 1]];
                }
                else
                {
                    prevR = curR;
                }
                if (zone_y < 15)
                {
                    nextRY = Rs[zones[zone_y + 1, zone_x]];
                }
                else
                {
                    nextRY = curR;
                }
                if (zone_y > 0)
                {
                    prevRY = Rs[zones[zone_y - 1, zone_x]];
                }
                else
                {
                    prevRY = curR;
                }
                if (k % partialWidth < partialWidth / crossfadeCoef)
                {
                    R = crossfade(k, (zone_x) * partialWidth, partialWidth / crossfadeCoef, prevR, curR);
                    if (i % partialWidth < partialWidth / crossfadeCoef)
                    {
                        R = (R + crossfade(i, (zone_y) * partialWidth, partialWidth / crossfadeCoef, prevRY, curR))/2;
                    }
                    else if (i % partialWidth > partialWidth - partialWidth / crossfadeCoef)
                    {
                        R = (R + crossfade(i, (zone_y + 1) * partialWidth, partialWidth / crossfadeCoef, curR, nextRY))/2;
                    }
                }
                else if (k % partialWidth > partialWidth - partialWidth / crossfadeCoef)
                {
                    R = crossfade(k, (zone_x + 1) * partialWidth, partialWidth / crossfadeCoef, curR, nextR);
                    if (i % partialWidth < partialWidth / crossfadeCoef)
                    {
                        R = (R + crossfade(i, (zone_y) * partialWidth, partialWidth / crossfadeCoef, prevRY, curR))/2;
                    }
                    else if (i % partialWidth > partialWidth - partialWidth / crossfadeCoef)
                    {
                        R = (R + crossfade(i, (zone_y + 1) * partialWidth, partialWidth / crossfadeCoef, curR, nextRY))/2;
                    }
                }else if (i % partialWidth < partialWidth / crossfadeCoef)
                {
                    R = crossfade(i, (zone_y) * partialWidth, partialWidth / crossfadeCoef, prevRY, curR);
                }
                else if (i % partialWidth > partialWidth - partialWidth / crossfadeCoef)
                {
                    R = crossfade(i, (zone_y + 1) * partialWidth, partialWidth / crossfadeCoef, curR, nextRY);
                }

                heights[i, k] = texture.GetPixel(i, k).grayscale * R;
            }
        }

        // Применяем изменения
        terrain.terrainData.size = new Vector3(width, width * 0.5f, height);
        terrain.terrainData.heightmapResolution = resolution;
        terrain.terrainData.alphamapResolution = width;
        terrain.terrainData.SetHeights(0, 0, heights);
        painter = terrain.GetComponent<Painter>();
        painter.Paint();
    }

    float crossfade(int x, int midX, int xSpread, float startR, float endR)
    {
        //int xSpread = endX - startX;
        // midX -= xSpread / 2;
        
        if (startR == endR)
            return startR;
        float returnR = 0f;
        if (startR > endR)
        {
            if (x < midX - xSpread / 2)
                return startR;
            if (x > midX + xSpread / 2)
                return endR;

            returnR = startR - ((startR-endR) / xSpread) * (xSpread / 2 - (midX - x));
        }
        else
        {
            if (x < midX - xSpread / 2)
                return startR;
            if (x > midX + xSpread / 2)
                return endR;

            returnR = startR + ((endR - startR) / xSpread) * (xSpread / 2 - (midX - x));
        }

        return returnR;
    }

    // Считаем рандомный коэффициент смещения для высоты
    float displace(float num)
    {
        float max = num / WH * GRAIN;
        return Random.Range(-0.5f, 0.5f) * max;
    }

    // Вызов функции отрисовки с параметрами
    void drawPlasma(float w, float h, float start_x, float start_y)
    {
        float c1, c2, c3, c4;
        c1 = Random.Range(0.1f, 0.15f);
        c2 = Random.Range(0.1f, 0.15f);
        c3 = Random.Range(0.1f, 0.15f);
        c4 = Random.Range(0.1f, 0.15f);
        if (GRAIN > 110)
        {
            c1 = 0.15f;
            c2 = 0.15f;
            c3 = 0.15f;
            c4 = 0.15f; 
        }
        Debug.Log(c1 + " " + c2 + " " + c3 + " " + c4);
        divide(start_x, start_y, w, h, c1, c2, c3, c4);
    }

    // Сама рекурсивная функция отрисовки
    void divide(float x, float y, float w, float h, float c1, float c2, float c3, float c4)
    {
        //Debug.Log(x + " " +  y);
        float newWidth = w * 0.5f;
        float newHeight = h * 0.5f;

        if (w < 1.0f && h < 1.0f) // || (x > 128 || y > 128)
        {
            float c = (c1 + c2 + c3 + c4) * _coefficient;
            var ins = inside(x, y, X.Length, X, Y);
            //var ins2 = inside(x, y, 4, InnerX, InnerY);
            cols[(int) x + (int) y * width] = new Color(c, c, c);
            if (ins == 0 && crop)
            {
                c = 1;
            }/*else if (ins == 1 && c <= 0)
            {
                c = 255;
            }*/
            other_cols[(int) x + (int) y * width] = new Color(c, c, c);
        }
        else
        {
            float middle = (c1 + c2 + c3 + c4) * 0.25f + displace(newWidth + newHeight);
            float edge1 = (c1 + c2) * 0.5f;
            float edge2 = (c2 + c3) * 0.5f;
            float edge3 = (c3 + c4) * 0.5f;
            float edge4 = (c4 + c1) * 0.5f;
            if (!FLAT)
            {
                if (middle <= 0)
                {
                    middle = 0;
                }
                else if (middle > 1.0f)
                {
                    middle = 1.0f;
                }
            }


            divide(x, y, newWidth, newHeight, c1, edge1, middle, edge4);
            divide(x + newWidth, y, newWidth, newHeight, edge1, c2, edge2, middle);
            divide(x + newWidth, y + newHeight, newWidth, newHeight, middle, edge2, c3, edge3);
            divide(x, y + newHeight, newWidth, newHeight, edge4, middle, edge3, c4);
        }
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
}