using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ProceduralNoiseProject;
using Common.Unity.Drawing;

namespace MarchingCubesProject
{

    //Noise list
    public enum NOISE_TYPE { PERLIN, VALUE, SIMPLEX }

    public class Cloud : MonoBehaviour
    {

        public Material material;

        public NOISE_TYPE noiseType = NOISE_TYPE.PERLIN;

        [Range(1, 100)]
        public int seed = 19;

        [Range(1, 10)]
        public int octaves = 3;

        [Range(0.1f, 5f)]
        public float frequency = 0.1f;

        [Range(-1f, 1f)]
        public float surface = 0.2f;

        public int width = 300;

        public int height = 60;

        public int depth = 150;

        public bool smoothNormals = true;

        private Material an_material;

        private NOISE_TYPE an_noiseType;

        private int an_seed;

        private int an_octaves;

        private float an_frequency;

        private float an_surface;

        private bool an_smoothNormals;

        private List<GameObject> meshes = new List<GameObject>();

        private NormalRenderer normalRenderer;

        void Start()
        {
            //Take defaults values of editables variables for Update
            an_material = material;
            an_noiseType = noiseType;
            an_seed = seed;
            an_octaves = octaves;
            an_frequency = frequency;
            an_surface = surface;
            an_smoothNormals = smoothNormals;

            //Initialisation of the noise
            INoise noise = GetNoise();
            FractalNoise fractal = new FractalNoise(noise, octaves, frequency);

            Marching marching = new MarchingCubes();

            //Surface is the suface of the mesh with the noise go to -1 to 1.
            marching.Surface = surface;

            var voxels = new VoxelArray(width, height, depth);

            //Put voxels with noise
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        float u = x / (width - 1.0f);
                        float v = y / (height - 1.0f);
                        float w = z / (depth - 1.0f);

                        voxels[x, y, z] = fractal.Sample3D(u, v, w);
                    }
                }
            }

            List<Vector3> vertexs = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            marching.Generate(voxels.Voxels, vertexs, indices);

            //Create normals with voxels
            if (smoothNormals)
            {
                for (int i = 0; i < vertexs.Count; i++)
                {
                    Vector3 p = vertexs[i];

                    float u = p.x / (width - 1.0f);
                    float v = p.y / (height - 1.0f);
                    float w = p.z / (depth - 1.0f);

                    Vector3 norm = voxels.GetNormal(u, v, w);

                    normals.Add(norm);
                }

                normalRenderer = new NormalRenderer();
                normalRenderer.DefaultColor = Color.red;
                normalRenderer.Length = 0.25f;
                normalRenderer.Load(vertexs, normals);
            }

            var position = new Vector3(-width / 2, -height / 2, -depth / 2);

            CreateMesh(vertexs, normals, indices, position);

        }

        private INoise GetNoise()
        {
            switch (noiseType)
            {
                case NOISE_TYPE.PERLIN:
                    return new PerlinNoise(seed, 20);

                case NOISE_TYPE.VALUE:
                    return new ValueNoise(seed, 20);

                case NOISE_TYPE.SIMPLEX:
                    return new SimplexNoise(seed, 20);

                default:
                    return new PerlinNoise(seed, 20);
            }
        }

        private void CreateMesh(List<Vector3> vertexs, List<Vector3> normals, List<int> indices, Vector3 position)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertexs);
            mesh.SetTriangles(indices, 0);

            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            GameObject cloud = new GameObject("Cloud Mesh");
            
            cloud.transform.parent = transform;
            cloud.AddComponent<MeshFilter>();
            cloud.AddComponent<MeshRenderer>();
            cloud.GetComponent<Renderer>().material = material;
            cloud.GetComponent<MeshFilter>().mesh = mesh;
            cloud.transform.localPosition = position;

            meshes.Add(cloud);
        }

        private void Update()
        {
            //Restart if editables variables change
            if (an_material != material || an_noiseType != noiseType || an_seed != seed
                || an_octaves != octaves || an_frequency != frequency || an_surface != surface
                || an_smoothNormals != smoothNormals)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    Destroy(meshes[i]);
                }
                Start();
            }

        }
    }
}
