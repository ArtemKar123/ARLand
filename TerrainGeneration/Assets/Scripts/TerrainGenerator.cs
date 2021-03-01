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
    private int width = 256;
    private int height = 256;
    private float WH;
    private Color32[] cols, other_cols;
    private Texture2D texture;
    private double[] X = new[] {40.0, 60, 140, 245, 235, 160, 40}; //{ 5.0, 7, 10, 25, 30, 30, 35 };
    private double[] Y = new[] {40.0, 140, 180, 200, 100, 40, 40}; //{ 15.0, 25, 10, 20, 23, 40, 35 };

    private double[] InnerX = new[] {70.0, 70, 90, 90};
    private double[] InnerY = new[] {70.0, 90, 90, 70};

    public int zoneCount = 8;

    private int currentZone = 0;

    private int[,] zones = new int[,] //0-равнина, 1-горы, 2-плоскость(под домики)
    {
        /*x=0;x=1*/
        /*y=0*/ {2, 2, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        /*y=1*/ {2, 2, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {0, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
        {1, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0},
    };

    private float[] Rs = new[] {0.2f, 0.4f, 0f};
    private int partialWidth;

    // Случайные зоны
    void GenerateZones()
    {
        for (int i = 0; i < zones.GetLength(0); i++)
        {
            for (int j = 0; j < zones.GetLength(1); j++)
            {
                zones[i, j] = (int) Random.Range(0, 2);
            }
        }
    }


    // Алгоритм diamond square -- https://habr.com/ru/post/249027/
    public void Start()
    {
        Debug.Log("Start " + zoneCount + " " + width);
        GenerateZones();

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

        // Коэффициент высот
        _coefficient = 0.5f;
        for (int y = 0; y < zoneCount; y++)
        {
            for (int x = 0; x < zoneCount; x++)
            {
                switch (zones[y, x])
                {
                    case 0:
                        GRAIN = 2; // Коэффициент зернистости
                        break;
                    case 1:
                        GRAIN = 8;
                        break;
                }

                currentZone = zones[y, x];
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
        Debug.Log(a + " " + partialWidth);
        for (int i = 0; i < resolution; i++) //y
        {
            for (int k = 0; k < resolution; k++) //x
            {
                /*
                int zone_x = k / partialWidth;
                int zone_y = i / partialWidth;
                float curR = 0f, nextR = 0f, prevR = 0f, nextRY = 0f, prevRY = 0f;
                curR = Rs[zones[zone_y, zone_x]];
                nextR = Rs[zones[zone_y, zone_x + 1]];
                if (zone_x > 0)
                {
                    prevR = Rs[zones[zone_y, zone_x - 1]];
                }
                else
                {
                    prevR = curR;
                }

                nextRY = Rs[zones[zone_y + 1, zone_x]];
                if (zone_y > 0)
                {
                    prevRY = Rs[zones[zone_y - 1, zone_x]];
                }
                else
                {
                    prevRY = curR;
                }

                if (k % partialWidth < partialWidth / 8)
                {
                    R = crossfade(k, (zone_x) * partialWidth, partialWidth / 8, prevR, curR);
                    if (i % partialWidth < partialWidth / 8)
                    {
                        R = (R + crossfade(i, (zone_y) * partialWidth, partialWidth / 8, prevRY, curR)) / 2;
                    }
                    else if (i % partialWidth > partialWidth - partialWidth / 8)
                    {
                        R = (R + crossfade(i, (zone_y + 1) * partialWidth, partialWidth / 8, curR, nextRY)) / 2;
                    }
                }
                else if (k % partialWidth > partialWidth - partialWidth / 8)
                {
                    R = crossfade(k, (zone_x + 1) * partialWidth, partialWidth / 8, curR, nextR);
                    if (i % partialWidth < partialWidth / 8)
                    {
                        R = (R + crossfade(i, (zone_y) * partialWidth, partialWidth / 8, prevRY, curR)) / 2;
                    }
                    else if (i % partialWidth > partialWidth - partialWidth / 8)
                    {
                        R = (R + crossfade(i, (zone_y + 1) * partialWidth, partialWidth / 8, curR, nextRY)) / 2;
                    }
                }
                else if (i % partialWidth < partialWidth / 8)
                {
                    R = crossfade(i, (zone_y) * partialWidth, partialWidth / 8, prevRY, curR);
                }
                else if (i % partialWidth > partialWidth - partialWidth / 8)
                {
                    R = crossfade(i, (zone_y + 1) * partialWidth, partialWidth / 8, curR, nextRY);
                }

                if (R <= 0.05f) R += 0.4f;*/
                heights[i, k] = texture.GetPixel(i, k).grayscale * 0.4f; // * R;
            }
        }

        // Применяем изменения
        var terrainData = terrain.terrainData;
        terrainData.size = new Vector3(width, width * 1, height);
        terrainData.heightmapResolution = resolution;
        terrain.terrainData.SetHeights(0, 0, heights);
        painter = terrain.GetComponent<Painter>();
        painter.Paint();
    }

    // Попытка сделать плавный переход между зонами
    float crossfade(int x, int midX, int xSpread, float startR, float endR)
    {
        if (startR == endR)
            return startR;
        float returnR = 0f;
        if (startR > endR)
        {
            if (x < midX - xSpread / 2)
                return startR;
            if (x > midX + xSpread)
                return endR;

            returnR = startR - (endR / xSpread) * (xSpread - (midX - x));
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
        switch (currentZone)
        {
            case 0:
                return Random.Range(-0.5f, 0.25f) * max;
            case 1:
                return Random.Range(0f, 0.5f) * max;
        }

        return 0; //Random.Range(0f, 1.5f) * max;
    }

    // Вызов функции отрисовки с параметрами
    void drawPlasma(float w, float h, float start_x, float start_y)
    {
        float c1, c2, c3, c4;
        c1 = Random.Range(0.1f, 0.15f);
        c2 = Random.Range(0.1f, 0.15f);
        c3 = Random.Range(0.1f, 0.15f);
        c4 = Random.Range(0.1f, 0.15f);
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
            var ins = inside(x, y, 7, X, Y); //
            //var ins2 = inside(x, y, 4, InnerX, InnerY);
            cols[(int) x + (int) y * width] = new Color(c, c, c);
            if (c <= 0) c = 5;
            if (ins == 0)
            {
                c = 0;
            }
            else if (ins == 1 && c <= 0)
            {
                c = 255;
            }

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


    // Проверяет, что точка находится в заданном контуре. 
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