using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    //returns a float[,] with continuous perlin noise
    public static float[,] GenerateNoise(int mapWidth, int mapHeight, float scale, float perlinSeed, int octaves, float persistence, float lacunarity) { 

        float[,] noiseMap = new float[mapWidth, mapHeight];

        //to avoid dividing by 0
        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        //used to normalize at the end
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        //y and x loop over the array, i loops over noise octaves
        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                //add noise for each octave
                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency;
                    float sampleY = y / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX + perlinSeed/scale, sampleY + perlinSeed/scale) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;

                }

                if(noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if(noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;       
            }
        }

        //normalize values and flatten edges
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                //flattens the edges of the mountain
                float flattenMultiplier = 1 - (DistanceToCenter(x, y, noiseMap) * 1.5f);
                if(flattenMultiplier < 0) flattenMultiplier = 0;

                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]) * (1-(Mathf.Cos(Mathf.Lerp(0,3.14f,flattenMultiplier)) * .5f + .5f));
            }
        }

        //check if the noise is centered, if not, regenerate noiseMap. move noise sample by scale/5
        if (!CheckMaxCentered(noiseMap, 0.1f))
            noiseMap = GenerateNoise(mapWidth, mapHeight, scale, perlinSeed + (scale/5), octaves, persistence, lacunarity);

        return noiseMap;
    }

    //checks that the largest value in map is within maxPercentFromCenter from the centermost point in
    //maxPercentFromCenter should be around 0.1f
    public static bool CheckMaxCentered(float[,] map, float maxPercentFromCenter)
    {
        //find the max
        float maxValue = 0;
        int[] maxPosition = new int[2];

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] > maxValue)
                {
                    maxValue = map[x, y];
                    maxPosition[0] = x;
                    maxPosition[1] = y;
                }
            }
        }

        if(DistanceToCenter(maxPosition[0], maxPosition[1], map) < maxPercentFromCenter) return true;
        else return false;
    }

    //returns the distance of the given point to the center of the map as a percentage
    public static float DistanceToCenter(int x, int y, float[,] map)
    {
        float distance = Mathf.Pow(Mathf.Pow(x - (map.GetLength(0) / 2), 2) + Mathf.Pow(y - (map.GetLength(1) / 2), 2), 0.5f);
        float diagonalDistance = Mathf.Pow(Mathf.Pow(map.GetLength(0), 2) + Mathf.Pow(map.GetLength(1), 2), 0.5f) / 2;

        return distance / diagonalDistance;
    }
}
