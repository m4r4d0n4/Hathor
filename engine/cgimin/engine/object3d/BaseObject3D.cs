using System;
using System.Collections.Generic;
using cgimin.engine.camera;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace cgimin.engine.object3d
{

    public class BaseObject3D
    {
        // the transformation (position, rotation, scale) of the object
        public Matrix4 Transformation = Matrix4.Identity;

        // lists, filled with the 3d-data
        public List<Vector3> Positions;
        public List<Vector3> Normals;
        public List<Vector2> UVs;
        public List<Vector3> Tangents;
        public List<Vector3> BiTangents;

        // the index-List
        public List<int> Indices;

        // Vartex-Array-Object "VAO"
        public int Vao;

        // object radius
        public float radius { get; private set; }

        public BaseObject3D()
        {
            Positions = new List<Vector3>();
            Normals = new List<Vector3>();
            UVs = new List<Vector2>();
            Tangents = new List<Vector3>();
            BiTangents = new List<Vector3>();
            Indices = new List<int>();
        }

        // generates the Vartex-Array-Objekt
        public void CreateVAO()
        {
            // list of the complete vertex data
            List<float> allData = new List<float>();

            // "interleaved" means position, normal and uv in one block for each vertex
            float sqrRadiusMax = 0.0f;
            for (int i = 0; i < Positions.Count; i++) {
                float sqrRadius = Positions[i].LengthSquared;
                if (sqrRadius > sqrRadiusMax) sqrRadiusMax = sqrRadius;

                allData.Add(Positions[i].X);
                allData.Add(Positions[i].Y);
                allData.Add(Positions[i].Z);

                allData.Add(Normals[i].X);
                allData.Add(Normals[i].Y);
                allData.Add(Normals[i].Z);

                allData.Add(UVs[i].X);
                allData.Add(UVs[i].Y);

                allData.Add(Tangents[i].X);
                allData.Add(Tangents[i].Y);
                allData.Add(Tangents[i].Z);

                allData.Add(BiTangents[i].X);
                allData.Add(BiTangents[i].Y);
                allData.Add(BiTangents[i].Z);
            }
            // radius is calculated from the squaredRadius
            radius = (float)Math.Sqrt(sqrRadiusMax);

            // generate the VBO for the "interleaved" data
            int allBufferVBO;
            GL.GenBuffers(1, out allBufferVBO);

            // Buffer is "binded", following OpenGL commands refer to this buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, allBufferVBO);

            // Data is uploaded to graphics memory
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(allData.Count  * sizeof(float)), allData.ToArray(), BufferUsageHint.StaticDraw);

            // BindBuffer to 0, so the following commands do not overwrite the current vbo (state machine)
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


            // generating the index buffer
            int IndexBuffer;
            GL.GenBuffers(1, out IndexBuffer);

            // Buffer is "binded", following OpenGL commands refer to this buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);

            // index data is uploaded to grpahics mamory
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(uint) * Indices.Count), Indices.ToArray(), BufferUsageHint.StaticDraw);

            // BindBuffer to 0, so the following commands do not overwrite the current element buffer (state machine)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            

            // generating the Vertex-Array-Objects
            GL.GenVertexArrays(1, out Vao);

            // Vertex-Array-Objekt is "binded", following OpenGL commands refer to this VAO. Inmportant for the folling two calls of "BindBuffer"
            GL.BindVertexArray(Vao);

            // BindBuffer commands: Are saved by our VAO.
            // Element-Buffer (indices)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);

            // ... then our interleaved VBO.
            GL.BindBuffer(BufferTarget.ArrayBuffer, allBufferVBO);

            // five calls of GL.VertexAttribPointer do follow, must be first "enabled"
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.EnableVertexAttribArray(4);

            // The description of our "interleaved" data structure, the shader needs to know how tpo handle our data
            // Die assignment to the "Index", the first parameter, will be recognized by the shader
            int strideSize = Vector3.SizeInBytes * 4 + Vector2.SizeInBytes;

            // At Index 0 (so at first) we have our position data. The last parameter defines at which byte-place in the vertex block the data for the position is saved 
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, strideSize, 0);

            // At Index 1 we have our normal data. We have it after the position, which is a "Vector3" type, so the byte-place is "Vector3.SizeInBytes"
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, strideSize, Vector3.SizeInBytes);

            // At Index 2 we have our UV data. We have it after the position and the normal, which are both "Vector3" type, so the byte-place is "Vector3.SizeInBytes" * 2
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, true, strideSize, Vector3.SizeInBytes * 2);

            // At Index 3 tangents.
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, true, strideSize, Vector3.SizeInBytes * 2 + Vector2.SizeInBytes);

            // At Index 4 biTangents.
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, true, strideSize, Vector3.SizeInBytes * 3 + Vector2.SizeInBytes);

            // BindBuffer to 0, so the following commands do not overwrite the current VAO
            GL.BindVertexArray(0);

            // Note: The generated VAO defines a data-structure, which must be considered by the shader regarding the index-places defined by GL.VertexAttribPointer 
            // The data-format placing 0 = position; 1 = normal and 2 = uv must be used by our materials

        }


        // Adds a triangle
        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 n1, Vector3 n2, Vector3 n3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            int index = Positions.Count;

            Positions.Add(v1);
            Positions.Add(v2);
            Positions.Add(v3);

            Normals.Add(n1);
            Normals.Add(n2);
            Normals.Add(n3);

            UVs.Add(uv1);
            UVs.Add(uv2);
            UVs.Add(uv3);

            // calculate tangents / bi-tangents
            Vector3 edge1 = v2 - v1;
            Vector3 edge2 = v3 - v1;

            Vector2 deltaUV1 = uv2 - uv1;
            Vector2 deltaUV2 = uv3 - uv1;

            float f;
            if (Math.Abs(deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y) < 0.0001f)
            {
                f = 1.0f;
            }
            else
            {
                f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);
            }

            Vector3 tangent = new Vector3(f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X),
                                          f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y),
                                          f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z));
            tangent.Normalize();

            Vector3 biTangent = new Vector3(f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X),
                                            f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y),
                                            f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z));
            biTangent.Normalize();


            if (Vector3.Dot(Vector3.Cross(n1, tangent), biTangent) < 0.0f)
            {
                tangent = tangent * -1.0f;
            }


            Tangents.Add(tangent);
            Tangents.Add(tangent);
            Tangents.Add(tangent);

            BiTangents.Add(biTangent);
            BiTangents.Add(biTangent);
            BiTangents.Add(biTangent);

            Indices.Add(index);
            Indices.Add(index + 2);
            Indices.Add(index + 1);
        }

        // overloaded method, auto-calculating normal 
        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).Normalized();
            AddTriangle(v1, v2, v3, normal, normal, normal, uv1, uv2, uv3);
        }

        public void AverageTangents()
        {
            int len = Positions.Count;

            for (int i = 0; i < len - 1; i++)
            {
                for (int o = i + 1; o < len; o++)
                {

                    if (Positions[i] == Positions[o] && Normals[i] == Normals[o] && UVs[i] == UVs[o])
                    {
                        Vector3 tanI = Tangents[i];
                        Tangents[i] += Tangents[o];
                        Tangents[o] += tanI;

                        Vector3 biTanI = BiTangents[i];
                        BiTangents[i] += BiTangents[o];
                        BiTangents[o] += biTanI;
                    }
                }
            }
        }

        // checks whether object is in view frustum
        public bool IsInView()
        {
            Vector3 pos = new Vector3(Transformation.M41, Transformation.M42, Transformation.M43);
            if (Camera.SphereIsInFrustum(pos, radius))
            {
                return true;
            }
            return false;
        }

        // unloads from graphics memory
        public void UnLoad()
        {
            // tbd.

        }






    }
}
