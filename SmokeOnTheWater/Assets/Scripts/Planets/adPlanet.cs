using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IcosphereGenerator : MonoBehaviour
{
    [Range(0, 6)] public int recursionLevel = 2;
    public float radius = 1f;
    public bool flatShading = false;
    public bool addMeshCollider = false;

    MeshFilter mf;
    MeshCollider mc;

    struct Tri { public int a,b,c; public Tri(int a,int b,int c){this.a=a;this.b=b;this.c=c;} } 

    List<Vector3> verts;
    List<Tri> faces;
    Dictionary<ulong,int> midpointCache;
    int vIndex;

    void Awake()     { EnsureRefs(); Generate(); }
    void OnValidate(){ EnsureRefs(); Generate(); }

    void EnsureRefs(){
        if (!mf) mf = GetComponent<MeshFilter>();
        if (addMeshCollider && !mc) mc = gameObject.GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();
        if (!addMeshCollider && mc) { DestroyImmediate(mc); mc = null; }
    }

    public void Generate(){
        Mesh mesh = CreateIcoSphere(radius, recursionLevel, flatShading);

        // 32-bit індекси, якщо потрібно
        if (mesh.vertexCount > 65535)
            mesh.indexFormat = IndexFormat.UInt32;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.sharedMesh = mesh;
        if (mc) mc.sharedMesh = mesh;
    }

    Mesh CreateIcoSphere(float r, int level, bool flat)
    {
        verts = new List<Vector3>(12);
        faces = new List<Tri>(20);
        midpointCache = new Dictionary<ulong, int>(1<<12);
        vIndex = 0;

        // 12 вершин ікосаедра
        float t = (1f + Mathf.Sqrt(5f)) * 0.5f;

        AddVertex(new Vector3(-1,  t,  0).normalized * r);
        AddVertex(new Vector3( 1,  t,  0).normalized * r);
        AddVertex(new Vector3(-1, -t,  0).normalized * r);
        AddVertex(new Vector3( 1, -t,  0).normalized * r);

        AddVertex(new Vector3( 0, -1,  t).normalized * r);
        AddVertex(new Vector3( 0,  1,  t).normalized * r);
        AddVertex(new Vector3( 0, -1, -t).normalized * r);
        AddVertex(new Vector3( 0,  1, -t).normalized * r);

        AddVertex(new Vector3( t,  0, -1).normalized * r);
        AddVertex(new Vector3( t,  0,  1).normalized * r);
        AddVertex(new Vector3(-t,  0, -1).normalized * r);
        AddVertex(new Vector3(-t,  0,  1).normalized * r);

        int[,] baseTris = {
            {0,11,5},{0,5,1},{0,1,7},{0,7,10},{0,10,11},
            {1,5,9},{5,11,4},{11,10,2},{10,7,6},{7,1,8},
            {3,9,4},{3,4,2},{3,2,6},{3,6,8},{3,8,9},
            {4,9,5},{2,4,11},{6,2,10},{8,6,7},{9,8,1}
        };

        for (int i=0;i<20;i++) faces.Add(new Tri(baseTris[i,0], baseTris[i,1], baseTris[i,2]));

        // підрозбиття
        for (int i=0;i<level;i++)
        {
            var next = new List<Tri>(faces.Count*4);
            foreach (var f in faces)
            {
                int ab = GetMidpoint(f.a, f.b, r);
                int bc = GetMidpoint(f.b, f.c, r);
                int ca = GetMidpoint(f.c, f.a, r);

                next.Add(new Tri(f.a, ab, ca));
                next.Add(new Tri(f.b, bc, ab));
                next.Add(new Tri(f.c, ca, bc));
                next.Add(new Tri(ab, bc, ca));
            }
            faces = next;
        }

        // Підготовка меша
        var mesh = new Mesh();
        if (flat)
        {
            // дублюємо вершини під кожен трикутник (пласкі нормалі)
            int triCount = faces.Count;
            var fVerts = new List<Vector3>(triCount * 3);
            var fUVs   = new List<Vector2>(triCount * 3);
            var indices= new List<int>(triCount * 3);

            for (int i=0;i<triCount;i++)
            {
                var f = faces[i];
                Vector3 v0 = verts[f.a];
                Vector3 v1 = verts[f.b];
                Vector3 v2 = verts[f.c];

                int baseIdx = fVerts.Count;
                fVerts.Add(v0); fVerts.Add(v1); fVerts.Add(v2);
                indices.Add(baseIdx); indices.Add(baseIdx+1); indices.Add(baseIdx+2);

                fUVs.Add(ToSphericalUV(v0));
                fUVs.Add(ToSphericalUV(v1));
                fUVs.Add(ToSphericalUV(v2));
            }

            mesh.SetVertices(fVerts);
            mesh.SetTriangles(indices, 0);
            mesh.SetUVs(0, fUVs);
            mesh.RecalculateNormals(); // будуть пласкими, бо вершини унікальні
        }
        else
        {
            // спільні вершини (гладкі нормалі)
            mesh.SetVertices(verts);

            var indices = new List<int>(faces.Count * 3);
            foreach (var f in faces) { indices.Add(f.a); indices.Add(f.b); indices.Add(f.c); }
            mesh.SetTriangles(indices, 0);

            // UV (сферичні; можливий шов по меридіану)
            var uvs = new List<Vector2>(verts.Count);
            for (int i=0;i<verts.Count;i++) uvs.Add(ToSphericalUV(verts[i]));
            mesh.SetUVs(0, uvs);
        }

        return mesh;
    }

    int AddVertex(Vector3 v){
        verts.Add(v);
        return vIndex++;
    }

    // Унікальний ключ пари вершин (порядок не важливий)
    static ulong EdgeKey(int a, int b){
        uint min = (uint)Mathf.Min(a,b);
        uint max = (uint)Mathf.Max(a,b);
        return ((ulong)min << 32) | (ulong)max;
    }

    int GetMidpoint(int a, int b, float r){
        ulong key = EdgeKey(a,b);
        if (midpointCache.TryGetValue(key, out int midIdx))
            return midIdx;

        Vector3 mid = (verts[a] + verts[b]) * 0.5f;
        mid = mid.normalized * r;
        int idx = AddVertex(mid);
        midpointCache[key] = idx;
        return idx;
    }

    static Vector2 ToSphericalUV(Vector3 p){
        Vector3 n = p.normalized;
        // u: [-pi..pi] -> [0..1], v: [-1..1] -> [0..1]
        float u = Mathf.Atan2(n.x, n.z) / (2f * Mathf.PI) + 0.5f;
        float v = n.y * 0.5f + 0.5f;
        return new Vector2(u, v);
    }
}
