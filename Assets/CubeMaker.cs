using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class MeshCreator
{

    private List<Vector3> vertices = new List<Vector3>();

    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();

    private List<int> triangleIndices = new List<int>();

    private Mesh mesh = null;


    public MeshCreator()
    {
        // No initialization required in constuctor
    }

    public void BuildTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
    {
        Vector3 normal = Vector3.Cross(vertex1 - vertex0, vertex2 - vertex0).normalized;

        BuildTriangle(vertex0, vertex1, vertex2, normal);
    }

    public void BuildTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 normal)
    {
        int v0Index = vertices.Count;
        int v1Index = vertices.Count + 1;
        int v2Index = vertices.Count + 2;

        // Put vertex data into vertices array
        vertices.Add(vertex0);
        vertices.Add(vertex1);
        vertices.Add(vertex2);

        // Use the same normal for all vertices (i.e., a surface normal)
        // Could change function signature to pass in 3 normals if needed
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);

        // Use standard uv coordinates
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        // Add integer pointers to vertices into triangles list
        triangleIndices.Add(v0Index);
        triangleIndices.Add(v1Index);
        triangleIndices.Add(v2Index);

    }

    public Mesh CreateMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
        }

        mesh.vertices = vertices.ToArray();

        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.triangles = triangleIndices.ToArray();

        return mesh;
    }

    public void Clear()
    {
        if (mesh != null)
        {
            mesh.Clear();
        }

        vertices.Clear();

        normals.Clear();

        uvs.Clear();

        triangleIndices.Clear();

    }

}




public class CubeMaker : MonoBehaviour
{

    public Vector3 size = Vector3.one;
    MeshCreator mc = new MeshCreator();
    float resize = 1.0f;
    float distance = 1.2f;
    public ParticleSystem system;



    // Update is called once per frame
    void Update()
    {
        float[] spectrum = new float[256];
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        var emitParams = new ParticleSystem.EmitParams();
        system.GetComponent<ParticleSystem>();


        MeshFilter meshFilter = this.GetComponent<MeshFilter>();
        // one submesh for each face
        Vector3 center = new Vector3(0, 0, 0);

        mc.Clear(); // Clear internal lists and mesh

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            resize++;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && resize > 1.0f)
        {
            resize--;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            distance += 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && distance > 1.2f)
        {
            distance -= 0.1f;
        }
        if(Input.GetKeyDown(KeyCode.A))
        {
            this.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            this.GetComponent<MeshRenderer>().material.color = Color.blue;
        }



        for (int j = 0; j < 20; j++)
        {
            for (int i = 0; i < 20; i++)
            {

                Vector3 cubeSize = size * 0.5f;
                //float timepass = 0;
                float timepass = Time.deltaTime * 20f;

                float noisy = cubeSize.y + Perlin.Noise(cubeSize.x + size.x * i * 0.12f * spectrum[i] * 100.0f + timepass, 0, cubeSize.z + size.z * j * 0.12f * spectrum[i] * 100.0f + timepass);
                //float noisy = cubeSize.y + Perlin.Noise(cubeSize.x + size.x * i * 0.12f + timepass , 0, cubeSize.z + size.z * j * 0.12f + timepass);

                // top of the cube
                // t0 is top left point
                Vector3 t0 = new Vector3(cubeSize.x + size.x * i * distance, noisy * resize, -cubeSize.z + size.z * j * distance);
                Vector3 t1 = new Vector3(-cubeSize.x + size.x * i * distance, noisy * resize, -cubeSize.z + size.z * j * distance);
                Vector3 t2 = new Vector3(-cubeSize.x + size.x * i * distance, noisy * resize, cubeSize.z + size.z * j * distance);
                Vector3 t3 = new Vector3(cubeSize.x + size.x * i * distance, noisy * resize, cubeSize.z + size.z * j * distance);

                // bottom of the cube
                Vector3 b0 = new Vector3(cubeSize.x + size.x * i * distance, -cubeSize.y, -cubeSize.z + size.z * j * distance);
                Vector3 b1 = new Vector3(-cubeSize.x + size.x * i * distance, -cubeSize.y, -cubeSize.z + size.z * j * distance);
                Vector3 b2 = new Vector3(-cubeSize.x + size.x * i * distance, -cubeSize.y, cubeSize.z + size.z * j * distance);
                Vector3 b3 = new Vector3(cubeSize.x + size.x * i * distance, -cubeSize.y, cubeSize.z + size.z * j * distance);

                // Top square
                mc.BuildTriangle(t0, t1, t2);
                mc.BuildTriangle(t0, t2, t3);

                // Bottom square
                mc.BuildTriangle(b2, b1, b0);
                mc.BuildTriangle(b3, b2, b0);

                // Back square
                mc.BuildTriangle(b0, t1, t0);
                mc.BuildTriangle(b0, b1, t1);

                mc.BuildTriangle(b1, t2, t1);
                mc.BuildTriangle(b1, b2, t2);

                mc.BuildTriangle(b2, t3, t2);
                mc.BuildTriangle(b2, b3, t3);

                mc.BuildTriangle(b3, t0, t3);
                mc.BuildTriangle(b3, b0, t0);

                meshFilter.mesh = mc.CreateMesh();

                //if (noisy > 1.5f)
                //{
                    //emitParams.position = new Vector3(cubeSize.x + size.x * i * distance, noisy * resize, -cubeSize.z + size.z * j * distance);
                    //system.Emit(emitParams, 10);
                //}

            }
        }

    }
}




