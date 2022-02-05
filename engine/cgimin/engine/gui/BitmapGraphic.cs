using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static cgimin.engine.texture.TextureManager;
using cgimin.engine.helpers;
using cgimin.engine.camera;

namespace cgimin.engine.gui
{
    public class BitmapGraphic
    {
        private int baseWidth;
        private int baseHeight;

        private int texWidth;
        private int texHeight;
        private int tID;

        private int indexCount;
        private int graphicVOA;

        public int Width { get; private set; }
        public int Height { get; private set; }

        // static
        private static int program = -1;
        private static int colorTextureLocation;
        private static int modelViewProjectionLocation;
        private static int alphaLocation;

        public BitmapGraphic(int textureID, int textureWidth, int textureHeight, int u1, int v1, int width, int height) : base()
        {
            Width = width;
            Height = height;

            texWidth = textureWidth;
            texHeight = textureHeight;
            tID = textureID;

            int u2 = u1 + width;
            int v2 = v1 + height;

            baseWidth = width;
            baseHeight = height;

            List<float> vertexData = new List<float>();
            List<int> indices = new List<int>();

            vertexData.Add(0); vertexData.Add(0); vertexData.Add(0);
            vertexData.Add((float)(u1) / texWidth); vertexData.Add((float)(v2) / texHeight);

            vertexData.Add(u2 - u1); vertexData.Add(0); vertexData.Add(0);
            vertexData.Add((float)(u2) / texWidth); vertexData.Add((float)(v2) / texHeight);

            vertexData.Add(u2 - u1); vertexData.Add((v2 - v1)); vertexData.Add(0);
            vertexData.Add((float)(u2) / texWidth); vertexData.Add((float)(v1) / texHeight);

            vertexData.Add(0); vertexData.Add((v2 - v1)); vertexData.Add(0);
            vertexData.Add((float)(u1) / texWidth); vertexData.Add((float)(v1) / texHeight);

            indices.Add(0); indices.Add(1); indices.Add(2);
            indices.Add(0); indices.Add(2); indices.Add(3);

            indexCount = indices.Count;

            int graphicVBO;
            GL.GenBuffers(1, out graphicVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, graphicVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Count * sizeof(float)), vertexData.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            int indexBuffer;
            GL.GenBuffers(1, out indexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(uint) * indices.Count), indices.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.GenVertexArrays(1, out graphicVOA);
            GL.BindVertexArray(graphicVOA);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, graphicVBO);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes + Vector2.SizeInBytes, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, true, Vector3.SizeInBytes + Vector2.SizeInBytes, Vector3.SizeInBytes);
            GL.BindVertexArray(0);

            if (program == -1)
            {
                program = ShaderCompiler.CreateShaderProgram("cgimin/engine/gui/shader/BitmapGraphic_VS.glsl", "cgimin/engine/gui/shader/BitmapGraphic_FS.glsl");
                GL.BindAttribLocation(program, 0, "in_position");
                GL.BindAttribLocation(program, 1, "in_uv");
                GL.LinkProgram(program);

                modelViewProjectionLocation = GL.GetUniformLocation(program, "modelview_projection_matrix");
                colorTextureLocation = GL.GetUniformLocation(program, "sampler");
                alphaLocation = GL.GetUniformLocation(program, "alpha");
            }

        }


      

        public void Draw(float positionX, float positionY, float alpha = 1.0f)
        {
            GL.BindVertexArray(graphicVOA);

            GL.UseProgram(program);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Uniform1(colorTextureLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, tID);

            Matrix4 refMatric = Matrix4.CreateTranslation(positionX, positionY, 0) * Camera.GuiProjection;
            GL.UniformMatrix4(modelViewProjectionLocation, false, ref refMatric);

            GL.Uniform1(alphaLocation, alpha);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(0);

        }


    }
}
