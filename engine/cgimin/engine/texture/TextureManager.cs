using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace cgimin.engine.texture
{
    public class TextureManager
    {

        // Methode zum laden einer Textur
        public static int LoadTexture(string fullAssetPath, bool clampEdges = false)
        {
            // Textur wird generiert
            int returnTextureID = GL.GenTexture();

            // Textur wird "gebunden", folgende Befehle beziehen sich auf die gesetzte Textur (Statemachine)
            GL.BindTexture(TextureTarget.Texture2D, returnTextureID);

            Bitmap bmp = new Bitmap(fullAssetPath);
            int width = bmp.Width;
            int height = bmp.Height;

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Textur-Parameter, Pixelformat etc.
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            if (clampEdges)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }

            bmp.UnlockBits(bmpData);

            // Mip-Map Daten werden generiert
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // Textur-ID wird zurückgegeben
            return returnTextureID;
        }

        public static int LoadCubemap(List<string> faces)
        {
            int textureID = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);

            for (int i = 0; i < faces.Count; i++)
            {
                Bitmap bmp = new Bitmap(faces[i]);
                int width = bmp.Width;
                int height = bmp.Height;

                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);


            return textureID;
        }

        public static int LoadIBLSpecularMap(string baseName, string fileType)
        {
            int textureID = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);

            for (int i = 0; i < 9; i++)
            {

                for (var o = 0; o < 6; o++)
                {
                    TextureTarget target = TextureTarget.TextureCubeMapPositiveX;
                    if (o == 0) target = TextureTarget.TextureCubeMapPositiveX;
                    if (o == 1) target = TextureTarget.TextureCubeMapNegativeX;
                    if (o == 2) target = TextureTarget.TextureCubeMapPositiveY;
                    if (o == 3) target = TextureTarget.TextureCubeMapNegativeY;
                    if (o == 4) target = TextureTarget.TextureCubeMapPositiveZ;
                    if (o == 5) target = TextureTarget.TextureCubeMapNegativeZ;

                    string fileName = baseName + "_m0" + i.ToString() + "_c0" + o.ToString() + "." + fileType;

                    Bitmap bmp = new Bitmap(fileName);
                    int width = bmp.Width;
                    int height = bmp.Height;

                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(target, i, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);

                }

            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            return textureID;
        }


        public static int LoadIBLIrradiance(string baseName, string fileType)
        {
            int textureID = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);


            for (var o = 0; o < 6; o++)
            {
                TextureTarget target = TextureTarget.TextureCubeMapPositiveX;
                if (o == 0) target = TextureTarget.TextureCubeMapPositiveX;
                if (o == 1) target = TextureTarget.TextureCubeMapNegativeX;
                if (o == 2) target = TextureTarget.TextureCubeMapPositiveY;
                if (o == 3) target = TextureTarget.TextureCubeMapNegativeY;
                if (o == 4) target = TextureTarget.TextureCubeMapPositiveZ;
                if (o == 5) target = TextureTarget.TextureCubeMapNegativeZ;

                string fileName = baseName + "_c0" + o.ToString() + "." + fileType;

                Bitmap bmp = new Bitmap(fileName);
                int width = bmp.Width;
                int height = bmp.Height;

                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(target, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);

            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            return textureID;
        }



    }
}
