using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshExplode : MonoBehaviour {

    struct triangleMeshRaw
    {
        public Vector4 triPos;
        public Vector4 triNorms;
        public Vector2 triUVs;
    };

    public Mesh myMesh;
    private int[] triList;
    private int triangleCount;
    private int positionsCount;
    private Vector4[] triVertices;
    private Vector4[] triNormals;
    private Vector2[] triUVs;
    private Bounds bnds;
    private ComputeBuffer triBuffer;
    private ComputeBuffer argsBuffer;
    public ComputeShader cmptShader;
    private MaterialPropertyBlock mpb; //shadows issue hack
    public Material mat;
    public float noiseAmp = 0;
    public Vector3 forceVec = Vector3.zero;
    public Vector4 rotationAmp = Vector4.zero;
    private Mesh triangleMesh;

    void Start ()
    {
        triList = myMesh.triangles;
        triVertices = CreateVec4FromVec3(myMesh.vertices,1);
        triNormals = CreateVec4FromVec3(myMesh.normals,1);
        triUVs = myMesh.uv;
        triangleCount = triList.Length;
        positionsCount = triangleCount * 3;

        bnds = new Bounds(Vector3.zero, new Vector3(300.0f, 300.0f, 300.0f));

        triangleMeshRaw[] data = new triangleMeshRaw[positionsCount];

        // Triangle Mesh
        triangleMesh = new Mesh();
        triangleMesh.vertices = new Vector3[3];
        triangleMesh.SetIndices(new[] { 0, 1, 2 }, MeshTopology.Triangles, 0);
        triangleMesh.UploadMeshData(true);

        for (int i = 0; i < triList.Length; i+=3)
        {
            int idx1 = triList[i];
            int idx2 = triList[i+1];
            int idx3 = triList[i+2];

            data[i].triPos = triVertices[idx1];
            data[i + 1].triPos = triVertices[idx2];
            data[i + 2].triPos = triVertices[idx3];

            data[i].triNorms.w = Random.value;
            data[i + 1].triNorms.w = Random.value;
            data[i + 2].triNorms.w = Random.value;


            data[i].triNorms = triNormals[idx1];
            data[i + 1].triNorms = triNormals[idx2];
            data[i + 2].triNorms = triNormals[idx3];

            data[i].triUVs = triUVs[idx1];
            data[i + 1].triUVs = triUVs[idx2];
            data[i + 2].triUVs = triUVs[idx3];
        }

        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        triBuffer = new ComputeBuffer(positionsCount , (4+4+2) * 4);

        uint indexPerInstance = 3;
        argsBuffer.SetData(new uint[5] { indexPerInstance, (uint)triangleCount, 0, 0, 0 });
        triBuffer.SetData(data);

        // This property block is used only for avoiding a bug (issue #913828)
        mpb = new MaterialPropertyBlock();
        mpb.SetFloat("_UniqueID", Random.value);

        // Clone the given material before using.
        mat = new Material(mat);
        mat.name += " (matInstance)";

        var kernel = cmptShader.FindKernel("CSMain");
        cmptShader.SetBuffer(kernel, "_triBuffer", triBuffer);
        cmptShader.SetFloat(Shader.PropertyToID("noiseAmp"), 0F);
        cmptShader.SetVector(Shader.PropertyToID("forceVec"), forceVec);
        cmptShader.SetVector(Shader.PropertyToID("rotationAmp"), rotationAmp);
        cmptShader.Dispatch(kernel, triangleCount, 1, 1);
    }

	void Update ()
    {
        var kernel = cmptShader.FindKernel("CSMain");
        cmptShader.SetBuffer(kernel, "_triBuffer", triBuffer);
        cmptShader.SetFloat(Shader.PropertyToID("Time"), Time.time);
        cmptShader.SetFloat(Shader.PropertyToID("noiseAmp"), noiseAmp);
        cmptShader.SetVector(Shader.PropertyToID("forceVec"), forceVec);
        cmptShader.SetVector(Shader.PropertyToID("rotationAmp"), rotationAmp);
        cmptShader.Dispatch(kernel, triangleCount/8, 1, 1);
        
        // Draw the mesh with instancing.
        mat.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        mat.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);
        mat.SetBuffer("_triBuffer", triBuffer);
        Graphics.DrawMeshInstancedIndirect(triangleMesh, 0, mat, bnds, argsBuffer, 0, mpb,UnityEngine.Rendering.ShadowCastingMode.On);
    }

    void OnDestroy()
    {
        if (argsBuffer!= null) argsBuffer.Release();
        if (triBuffer != null) triBuffer.Release();
        //Destroy(mat);
        Destroy(triangleMesh);
    }

    Vector4[] CreateVec4FromVec3(Vector3[] V3l, float lastVal)
    {
        Vector4[] newList = new Vector4[V3l.Length];
        for (int i = 0; i < V3l.Length; i++)
        {
            newList[i] = new Vector4(V3l[i].x, V3l[i].y, V3l[i].z, lastVal);
        }
        return newList;
    }
}
