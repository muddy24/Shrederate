using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    //returns a float[,] with continuous perlin noise
    public static float[,] GenerateNoise(int mapWidth, float scale, Vector2 perlinSeed, int octaves, float persistence, float lacunarity, float ridgeSmoothing, AnimationCurve edgeSmoothCurve) { 

        float[,] noiseMap = new float[mapWidth, mapWidth];

        //to avoid dividing by 0
        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        //used to normalize at the end
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        //loop through each point in matrix
        for(int y = 0; y < mapWidth; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                //loop through octaves
                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency;
                    float sampleY = y / scale * frequency;

                    float perlinValue = SmoothMin(.9f, 1-Mathf.Abs(Mathf.PerlinNoise(sampleX + perlinSeed.x/scale, sampleY + perlinSeed.y/scale) * 2 - 1), ridgeSmoothing);
                 
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;

                }

                //find highest point
                if(noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if(noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                //add point to matrix
                noiseMap[x, y] = noiseHeight;       
            }
        }

        //returns the min between a and b, smoothed by a factor of k
        //used for smoothing ridges
        float SmoothMin(float a, float b, float k)
        {
            float h = Mathf.Clamp01((b - a + k) / (2 * k));
            return a * h + b * (1 - h) - k * h * (1 - h);
        }


        //normalize values and flatten edges
        for (int y = 0; y < mapWidth; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                //flattens the edges of the mountain
                float flattenMultiplier = edgeSmoothCurve.Evaluate(DistanceToCenter(x, y, noiseMap)*1.44f);//1 - Mathf.Pow((DistanceToCenter(x, y, noiseMap) * 1.44f);
                if(flattenMultiplier < 0) flattenMultiplier = 0;

                //noiseMap[x, y] = ((flattenMultiplier * maxNoiseHeight) + noiseMap[x, y]) / 2; //average point and curve
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]) * flattenMultiplier;//(1-(Mathf.Cos(Mathf.Lerp(0,3.14f,flattenMultiplier)) * .5f + .5f));
            }
        }

        //check if the noise is centered, if not, regenerate noiseMap. move noise sample so that peak is in the middle
        //TODO: confirm that the centering is actually working. Might just be moving randomly if I screwed up the adjustment
        if (!CheckMaxCentered(noiseMap, 0.1f))
            noiseMap = GenerateNoise(mapWidth, scale, perlinSeed + maxPoint(noiseMap) - new Vector2((mapWidth/2),(mapWidth/2)), octaves, persistence, lacunarity, ridgeSmoothing, edgeSmoothCurve);

                return noiseMap;
    }

    //returns the highest point on the map
    public static Vector2 maxPoint(float[,] map)
    {
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

        return new Vector2(maxPosition[0], maxPosition[1]);
    }

    //checks that the largest value in map is within maxPercentFromCenter from the centermost point in
    //maxPercentFromCenter should be around 0.1f
    public static bool CheckMaxCentered(float[,] map, float maxPercentFromCenter)
    {
        Vector2 max = maxPoint(map);

        if(DistanceToCenter((int)max.x, (int)max.y, map) < maxPercentFromCenter) return true;
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
