using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace {

    Mesh mesh;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    float noiseScale;
    float noiseStrength;

    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp, float NoiseScale = 0, float NoiseStrength = 0)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        noiseScale = NoiseScale;
        noiseStrength = NoiseStrength;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }
    public float Get3DPerlinNoise(float x, float y, float z)
        {
            // Sample 2D Perlin noise at different Z slices
            float noiseXY = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
            float noiseXZ = Mathf.PerlinNoise(x * noiseScale, z * 20);
            float noiseYZ = Mathf.PerlinNoise(y * noiseScale, z * 20);

            // Combine the results (simple averaging for demonstration)
            return (noiseXY + noiseXZ + noiseYZ) / 3f; 
        }
    public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                float noise = Get3DPerlinNoise(pointOnUnitSphere.x, pointOnUnitSphere.y, pointOnUnitSphere.z) * noiseStrength;
                pointOnUnitSphere *= 1 + noise;
                vertices[i] = pointOnUnitSphere;

                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}