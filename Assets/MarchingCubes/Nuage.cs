using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using ProceduralNoiseProject;
using Common.Unity.Drawing;

namespace MarchingCubesProject
{

    public enum MARCHING_MODE {  CUBES, TETRAHEDRON };

    public enum NOISE_TYPE { PERLIN, VALUE, SIMPLEX, VORONOI, WORLEY }

    public class Nuage : MonoBehaviour
    {

        public Material material;

        public MARCHING_MODE mode = MARCHING_MODE.CUBES;

        public NOISE_TYPE noiseType = NOISE_TYPE.PERLIN;

        [Range(1, 100)]
        public int seed = 0;

        [Range(1, 80)]
        public int octaves = 4;

        [Range(0.1f, 10f)]
        public float frequency = 1.0f;

        public bool smoothNormals = false;

        public bool drawNormals = false;

        public bool rotate = false;

        private Material an_material;
        
        private MARCHING_MODE an_mode;
        
        private NOISE_TYPE an_noiseType;
        
        private int an_seed;
        
        private int an_octaves;
        
        private float an_frequency;
        
        private bool an_smoothNormals;
        
        private bool an_drawNormals;

        private List<GameObject> meshes = new List<GameObject>();

        private NormalRenderer normalRenderer;

        void Start()
        {
            an_material = material;
            an_mode = mode;
            an_noiseType = noiseType;
            an_seed = seed;
            an_octaves = octaves;
            an_frequency = frequency;
            an_smoothNormals = smoothNormals;
            an_drawNormals = drawNormals;

            INoise noise = GetNoise();
            FractalNoise fractal = new FractalNoise(noise, octaves, frequency);

            //Set the mode used to create the mesh.
            //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
            Marching marching = null;
            if(mode == MARCHING_MODE.TETRAHEDRON)
                marching = new MarchingTertrahedron();
            else
                marching = new MarchingCubes();

            //Surface is the value that represents the surface of mesh
            //For example the noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;

            //The size of voxel array.
            int width = 32;
            int height = 32;
            int depth = 32;

            var voxels = new VoxelArray(width, height, depth);

            //Fill voxels with values. Im using noise but any method to create voxels will work.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        float u = x / (width - 1.0f);
                        float v = y / (height - 1.0f);
                        float w = z / (depth - 1.0f);

                        voxels[x,y,z] = fractal.Sample3D(u, v, w);
                    }
                }
            }

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels.Voxels, verts, indices);

            //Create the normals from the voxel.

            if (smoothNormals)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    //Presumes the vertex is in local space where
                    //the min value is 0 and max is width/height/depth.
                    Vector3 p = verts[i];

                    float u = p.x / (width - 1.0f);
                    float v = p.y / (height - 1.0f);
                    float w = p.z / (depth - 1.0f);

                    Vector3 n = voxels.GetNormal(u, v, w);

                    normals.Add(n);
                }

                normalRenderer = new NormalRenderer();
                normalRenderer.DefaultColor = Color.red;
                normalRenderer.Length = 0.25f;
                normalRenderer.Load(verts, normals);
            }

            var position = new Vector3(-width / 2, -height / 2, -depth / 2);

            CreateMesh32(verts, normals, indices, position);

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

                case NOISE_TYPE.VORONOI:
                    return new VoronoiNoise(seed, 20);

                case NOISE_TYPE.WORLEY:
                    return new WorleyNoise(seed, 20, 1.0f);

                default:
                    return new PerlinNoise(seed, 20);
            }
        }

        private void CreateMesh32(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.localPosition = position;

            meshes.Add(go);
        }

        /// <summary>
        /// UPDATE - Unity now supports 32 bit indices so the method is optional.
        /// 
        /// A mesh in unity can only be made up of 65000 verts.
        /// Need to split the verts between multiple meshes.
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        /// <param name="position"></param>
        private void CreateMesh16(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
        {

            int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
            int numMeshes = verts.Count / maxVertsPerMesh + 1;

            for (int i = 0; i < numMeshes; i++)
            {
                List<Vector3> splitVerts = new List<Vector3>();
                List<Vector3> splitNormals = new List<Vector3>();
                List<int> splitIndices = new List<int>();

                for (int j = 0; j < maxVertsPerMesh; j++)
                {
                    int idx = i * maxVertsPerMesh + j;

                    if (idx < verts.Count)
                    {
                        splitVerts.Add(verts[idx]);
                        splitIndices.Add(j);

                        if(normals.Count != 0)
                            splitNormals.Add(normals[idx]);
                    }
                }

                if (splitVerts.Count == 0) continue;

                Mesh mesh = new Mesh();
                mesh.indexFormat = IndexFormat.UInt16;
                mesh.SetVertices(splitVerts);
                mesh.SetTriangles(splitIndices, 0);

                if(splitNormals.Count > 0)
                    mesh.SetNormals(splitNormals);
                else
                    mesh.RecalculateNormals();

                mesh.RecalculateBounds();

                GameObject go = new GameObject("Mesh");
                go.transform.parent = transform;
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                go.GetComponent<Renderer>().material = material;
                go.GetComponent<MeshFilter>().mesh = mesh;
                go.transform.localPosition = position;

                meshes.Add(go);
            }
        }

        private void Update()
        {
            if(rotate)
                transform.Rotate(Vector3.up, 10.0f * Time.deltaTime);

            if (an_material != material)
            {
                for(int i = 0; i < meshes.Count; i++)
                {
                    Destroy(meshes[i]);
                }
                Start();
            }

            if (an_mode != mode)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    Destroy(meshes[i]);
                }
                Start();
            }

            if (an_noiseType != noiseType)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    Destroy(meshes[i]);
                }
                Start();
            }

            if (an_seed != seed)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    Destroy(meshes[i]);
                }
                Start();
            }

            if (an_octaves != octaves)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    Destroy(meshes[i]);
                }
                Start();
            }

            if (an_frequency != frequency)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    Destroy(meshes[i]);
                }
                Start();
            }

            if (an_smoothNormals != smoothNormals)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    Destroy(meshes[i]);
                }
                Start();
            }

            if (an_drawNormals != drawNormals)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    Destroy(meshes[i]);
                }
                Start();
            }

        }

        private void OnRenderObject()
        {
            if(normalRenderer != null && meshes.Count > 0 && drawNormals)
            {
                var m = meshes[0].transform.localToWorldMatrix;

                normalRenderer.LocalToWorld = m;
                normalRenderer.Draw();
            }
            
        }

    }

}
