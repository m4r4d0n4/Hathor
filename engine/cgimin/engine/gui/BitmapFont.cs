using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.helpers;
using static cgimin.engine.texture.TextureManager;
using cgimin.engine.camera;
using cgimin.engine.texture;

namespace cgimin.engine.gui
{
    public class BitmapFont
    {

        private class CharSettings
        {
            public int id;
            public int x;
            public int y;
            public int width;
            public int height;
            public int xoffset;
            public int yoffset;
            public int xadvance;
        }

        private class Kerning
        {
            public int first;
            public int second;
            public int amount;
        }

        private Matrix4 intTransform;

        private List<CharSettings> chars;
        private List<Kerning> kernings;

        private int baseHeight;
        private int baseWidth;
        private int textWidth;
        private int textHeight;

        // ogl
        private int indexCount;
        private int bitmapFontVOA;
        private int textureID;

        private static int program = -1;
        private static int colorTextureLocation;
        private static int charPositionLocation;
        private static int projectionLocation;
        private static int colorLocation;

        public BitmapFont(string fontFilePath, string bitmapFilePath)
        {
            textureID = TextureManager.LoadTexture(bitmapFilePath, false);

            chars = new List<CharSettings>();
            for (int i = 0; i < 256; i++)
            {
                chars.Add(new CharSettings());
                int index = chars.Count - 1;
                chars[index].id = i;
                chars[index].x = 0;
                chars[index].y = 0;
                chars[index].width = 0;
                chars[index].height = 0;
                chars[index].xoffset = 0;
                chars[index].yoffset = 0;
                chars[index].xadvance = 0;
            }

            kernings = new List<Kerning>();

            var input = File.ReadLines(fontFilePath);

            foreach (string line in input)
            {
                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts[0] == "common")
                {
                    baseHeight = int.Parse(parts[1].Split(new char[] { '=' })[1]);
                    baseWidth = int.Parse(parts[2].Split(new char[] { '=' })[1]);
                    textWidth = int.Parse(parts[3].Split(new char[] { '=' })[1]);
                    textHeight = int.Parse(parts[4].Split(new char[] { '=' })[1]);
                }

                if (parts[0] == "char")
                {
                    int index = int.Parse(parts[1].Split(new char[] { '=' })[1]);
                    chars[index].id = index;
                    chars[index].x = int.Parse(parts[2].Split(new char[] { '=' })[1]);
                    chars[index].y = int.Parse(parts[3].Split(new char[] { '=' })[1]);
                    chars[index].width = int.Parse(parts[4].Split(new char[] { '=' })[1]);
                    chars[index].height = int.Parse(parts[5].Split(new char[] { '=' })[1]);
                    chars[index].xoffset = int.Parse(parts[6].Split(new char[] { '=' })[1]);
                    chars[index].yoffset = int.Parse(parts[7].Split(new char[] { '=' })[1]);
                    chars[index].xadvance = int.Parse(parts[8].Split(new char[] { '=' })[1]);
                }

                if (parts[0] == "kerning")
                {
                    kernings.Add(new Kerning());
                    int index = kernings.Count - 1;
                    kernings[index].first = int.Parse(parts[1].Split(new char[] { '=' })[1]);
                    kernings[index].second = int.Parse(parts[2].Split(new char[] { '=' })[1]);
                    kernings[index].amount = int.Parse(parts[3].Split(new char[] { '=' })[1]);
                }
            }


            // OpenGL

            List<float> fontData = new List<float>();
            List<int> indices = new List<int>();

            for (int i = 0; i < 256; i++)
            {
                fontData.Add(0);
                fontData.Add(0);
                fontData.Add(0);
                fontData.Add(chars[i].x / (float)textWidth);
                fontData.Add((chars[i].y + chars[i].height) / (float)textHeight);

                fontData.Add(chars[i].width);
                fontData.Add(0);
                fontData.Add(0);
                fontData.Add((chars[i].x + chars[i].width) / (float)textWidth);
                fontData.Add((chars[i].y + chars[i].height) / (float)textHeight);

                fontData.Add(chars[i].width);
                fontData.Add(chars[i].height);
                fontData.Add(0);
                fontData.Add((chars[i].x + chars[i].width) / (float)textWidth);
                fontData.Add(chars[i].y / (float)textHeight);

                fontData.Add(0);
                fontData.Add(chars[i].height);
                fontData.Add(0);
                fontData.Add(chars[i].x / (float)textWidth);
                fontData.Add(chars[i].y / (float)textHeight);

                int bIndex = i * 4;

                indices.Add(bIndex);
                indices.Add(bIndex + 2);
                indices.Add(bIndex + 1);

                indices.Add(bIndex);
                indices.Add(bIndex + 3);
                indices.Add(bIndex + 2);
            }


            indexCount = indices.Count;

            int bitmapFontVBO;
            GL.GenBuffers(1, out bitmapFontVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bitmapFontVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(fontData.Count * sizeof(float)), fontData.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            int indexBuffer;
            GL.GenBuffers(1, out indexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(uint) * indices.Count), indices.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.GenVertexArrays(1, out bitmapFontVOA);
            GL.BindVertexArray(bitmapFontVOA);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bitmapFontVBO);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes + Vector2.SizeInBytes, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, true, Vector3.SizeInBytes + Vector2.SizeInBytes, Vector3.SizeInBytes);
            GL.BindVertexArray(0);

            if (program == -1)
            {
                program = ShaderCompiler.CreateShaderProgram("cgimin/engine/gui/shader/BitmapFont_VS.glsl", "cgimin/engine/gui/shader/BitmapFont_FS.glsl");
                GL.BindAttribLocation(program, 0, "in_position");
                GL.BindAttribLocation(program, 1, "in_uv");
                GL.LinkProgram(program);

                colorTextureLocation = GL.GetUniformLocation(program, "color_texture");
                charPositionLocation = GL.GetUniformLocation(program, "char_position");
                projectionLocation = GL.GetUniformLocation(program, "projection_matrix");
                colorLocation = GL.GetUniformLocation(program, "color");
            }

        }

        public int CalcTextWidth(string wString)
        {
            int len = wString.Length;
            int w = 0;
            for (int i = 0; i < len; i++)
            {
                char c = wString[i];
                w += chars[c].xadvance;
            }
            return w;
        }

        public void DrawString(string wString, float xPosition, float yPosition, int r, int g, int b, int a)
        {
            GL.BindVertexArray(bitmapFontVOA);

            GL.UseProgram(program);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Uniform1(colorTextureLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            Matrix4 projection = Matrix4.CreateTranslation(xPosition, yPosition, 0) * Camera.GuiProjection;
            GL.UniformMatrix4(projectionLocation, false, ref projection);

            Vector4 color = new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
            GL.Uniform4(colorLocation, ref color);

            int len = wString.Length;
            int pos = 0;

            for (int i = 0; i < len; i++)
            {
                char c = wString[i];
                GL.Uniform3(charPositionLocation, new Vector3(pos + chars[c].xoffset, baseHeight -(chars[c].height + chars[c].yoffset), 1));
                pos += chars[c].xadvance;
                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, c * 6 * sizeof(int));
            }

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(0);

        }


    }
}
