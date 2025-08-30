using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlanetGenerator : MonoBehaviour
{
    [Header("Mesh Settings")]
    [Range(2, 256)] public int resolution = 100;

    [Header("Noise Settings")]
    public float baseNoiseScale = 1f;
    public float baseNoiseStrength = 0.2f;

    public float ridgedStrength = 0.3f;
    public float warpStrength = 0.05f;
    public float warpScale = 2f;

    [Range(1, 6)] public int octaves = 3;
    [Range(0.1f, 0.9f)] public float persistence = 0.5f;
    [Range(1f, 4f)] public float lacunarity = 2f;

    [Header("Biome Settings")]
    public Material planetMaterial;
    [Tooltip("Textures applied based on height levels")]
    public Texture2D oceanTex;
    public Texture2D grassTex;
    public Texture2D rockTex;
    public Texture2D snowTex;

    MeshFilter meshFilter;

    void OnValidate()
    {
        GeneratePlanet();
    }

    void GeneratePlanet()
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        Vector3[] faceDirs = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        foreach (var localUp in faceDirs)
        {
            Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            Vector3 axisB = Vector3.Cross(localUp, axisA);

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int i = vertices.Count;
                    Vector2 percent = new Vector2(x, y) / (resolution - 1);
                    Vector3 pointOnCube = localUp + (percent.x - 0.5f) * 2f * axisA + (percent.y - 0.5f) * 2f * axisB;
                    Vector3 pointOnSphere = pointOnCube.normalized;

                    // --- Шум (Base + Ridged + Warp) ---
                    float elevation = 1 + GenerateNoise(pointOnSphere);
                    Vector3 finalPoint = pointOnSphere * elevation;

                    vertices.Add(finalPoint);

                    // UV → висота
                    float height01 = Mathf.InverseLerp(0.8f, 1.2f, elevation);
                    uvs.Add(new Vector2(height01, 0));

                    // Трикутники
                    if (x != resolution - 1 && y != resolution - 1)
                    {
                        triangles.Add(i);
                        triangles.Add(i + resolution + 1);
                        triangles.Add(i + resolution);

                        triangles.Add(i);
                        triangles.Add(i + 1);
                        triangles.Add(i + resolution + 1);
                    }
                }
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;

        // --- застосовуємо матеріал ---
        if (planetMaterial != null)
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            mr.sharedMaterial = planetMaterial;
            mr.sharedMaterial.SetTexture("_OceanTex", oceanTex);
            mr.sharedMaterial.SetTexture("_GrassTex", grassTex);
            mr.sharedMaterial.SetTexture("_RockTex", rockTex);
            mr.sharedMaterial.SetTexture("_SnowTex", snowTex);
        }
    }
    float PerlinNoise3D(Vector3 p, float scale)
    {
        float xy = Mathf.PerlinNoise(p.x * scale, p.y * scale);
        float yz = Mathf.PerlinNoise(p.y * scale, p.z * scale);
        float zx = Mathf.PerlinNoise(p.z * scale, p.x * scale);

        float yx = Mathf.PerlinNoise(p.y * scale, p.x * scale);
        float zy = Mathf.PerlinNoise(p.z * scale, p.y * scale);
        float xz = Mathf.PerlinNoise(p.x * scale, p.z * scale);

        return (xy + yz + zx + yx + zy + xz) / 6f; // усереднення
    }
    float GenerateNoise(Vector3 point)
    {
        // Warp noise
        Vector3 warp = point * warpScale;
        warp += new Vector3(
            Mathf.PerlinNoise(point.y, point.z),
            Mathf.PerlinNoise(point.z, point.x),
            Mathf.PerlinNoise(point.x, point.y)
        ) * warpStrength;

        float frequency = baseNoiseScale;
        float amplitude = baseNoiseStrength;
        float noiseValue = 0f;

        for (int o = 0; o < octaves; o++)
        {
            float n = PerlinNoise3D(point + warp, frequency);
            n = 1f - Mathf.Abs(n * 2 - 1f); // ridged noise
            noiseValue += n * amplitude;

            frequency *= lacunarity;
            amplitude *= persistence;
        }

        return noiseValue * ridgedStrength;
    }
}
