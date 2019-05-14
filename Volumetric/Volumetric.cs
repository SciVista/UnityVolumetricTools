using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System;

public static class VolumetricTools
{

    public static Texture3D CreateTexture3D(int size)
    {
        Color[] colorArray = new Color[size * size * size];
        Texture3D texture = new Texture3D(size, size, size, TextureFormat.RGBA32, true);
        float r = 1.0f / (size - 1.0f);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Color c = new Color(x * r, y * r, z * r, 1.0f);
                    colorArray[x + (y * size) + (z * size * size)] = c;
                }
            }
        }
        texture.SetPixels(colorArray);
        texture.Apply();
        return texture;
    }


    public static Texture3D Create3dTexture(Vector3Int size, Func<Vector3, Color> generator)
    {
        if (generator == null)
        {
            throw new ArgumentNullException("generator");
        }
        int xSize = size.x;
        int ySize = size.y;
        int zSize = size.z;

        int totalCount = size.x * size.y * size.z;
        Color[] colorArray = new Color[totalCount];
        Texture3D texture = new Texture3D(xSize, ySize, zSize, TextureFormat.RGBA32, true);
        int currentXPoint = 0;
        int currentYPoint = 0;
        int currentZPoint = 0;
        Vector3 curPoint;
        for (int i = 0; i < totalCount; i++)
        {
            //float entry = values[i][key].AsFloat / 255;

            if (currentXPoint == xSize)
            {
                currentXPoint = 0;
                currentYPoint++;
            }
            if (currentYPoint > ySize)
            {
                currentYPoint = 0;
                currentZPoint++;
            }
            if (currentZPoint > zSize)
            {
                Debug.Log("Break");
                break;
            }
            curPoint = new Vector3(currentXPoint, currentYPoint, currentZPoint);
            colorArray[i] = generator(curPoint);
            currentXPoint++;
        }
        texture.SetPixels(colorArray);
        texture.Apply();
        return texture;
    }

    public static JobHandle ScheduleVolumetricTextureJob(NativeArray<float> values, NativeArray<Color> results, Vector3Int size, int innerBatchLoop = 64)
    {
        int totalCount = values.Length;
        int totalSize = size.x * size.y * size.z;
        if (totalCount != totalSize) throw new ArgumentOutOfRangeException();
        VolumetricTextureJob volumeJob = new VolumetricTextureJob()
        {
            Results = results,
            Values = values,
            size = size

        };
        JobHandle jobHandle = volumeJob.Schedule(totalCount, innerBatchLoop);
        JobHandle.ScheduleBatchedJobs();

        return jobHandle;
    }

    public static JobHandle ScheduleVolumetricTextureJob(NativeArray<float> values, NativeArray<Color> results, Vector3Int size, Action resultCallback, int innerBatchLoop = 64)
    {
        int totalCount = values.Length;
        int totalSize = size.x * size.y * size.z;
        if (totalCount != totalSize) throw new ArgumentOutOfRangeException();
        VolumetricTextureJob volumeJob = new VolumetricTextureJob()
        {
            Results = results,
            Values = values,
            size = size

        };
        JobHandle jobHandle = volumeJob.Schedule(totalCount, innerBatchLoop);
        JobHandle.ScheduleBatchedJobs();

        return jobHandle;
    }
    
    public static Texture3D GenerateTexture3D(Color[] colors, Vector3Int size)
    {
        Texture3D texture = new Texture3D(size.x, size.y, size.z, TextureFormat.RGBA32, false);
        texture.SetPixels(colors);
        return texture;
    }

    public static IEnumerator CreateVolumetricTexture(float[] values, Vector3Int size, Action<Texture3D> resultCallback)
    {
        Debug.Log("Generating Texture3d");

        NativeArray<float> _values = new NativeArray<float>(values, Allocator.TempJob);
        NativeArray<Color> _results = new NativeArray<Color>(values.Length, Allocator.TempJob, NativeArrayOptions.ClearMemory);
        JobHandle volumetricJobHandle = ScheduleVolumetricTextureJob(_values, _results, size);
        volumetricJobHandle.Complete();

        yield return new WaitForSeconds(5f);
        yield return new WaitUntil(() => volumetricJobHandle.IsCompleted);

        Texture3D tex = GenerateTexture3D(_results.ToArray(), size);
        _results.Dispose();
        _values.Dispose();
        Debug.Log("Finished generating Texture3d");
        resultCallback(tex);
    }

}

public struct VolumetricTextureJob : IJobParallelFor
{
    [WriteOnly]
    public NativeArray<Color> Results;

    [ReadOnly]
    public NativeArray<float> Values;

    [ReadOnly]
    public Vector3 size;

    public void Execute(int i)
    {
        float value = Values[i];
        Results[i] = new Color(value, value, value, value);
    }
}