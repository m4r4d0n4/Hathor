using cgimin.engine.material;
using cgimin.engine.object3d;
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using cgimin.engine.camera;
using cgimin.engine.helpers;
using Engine.cgimin.engine.postprocessing;

namespace cgimin.engine.postprocessing.fxaa
{
    public class FXAA : BasePostProcessing
    {
        private int screenWidthLocation;
        private int screenHeightLocation;

        public FXAA(): base()
        {

            // Shader-Programm loaded from external files
            Program = ShaderCompiler.CreateShaderProgram("cgimin/engine/postprocessing/fxaa/FXAA_VS.glsl",
                                                         "cgimin/engine/postprocessing/fxaa/FXAA_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            screenWidthLocation = GL.GetUniformLocation(Program, "screenWidth");
            screenHeightLocation = GL.GetUniformLocation(Program, "screenHeight");
        }


        public void Draw(int sourceTexture, int screenWidth, int screenHeight)
        {
            GL.Disable(EnableCap.CullFace);

            GL.UseProgram(Program);

            // das Vertex-Array-Objekt unseres Objekts wird benutzt
            GL.BindVertexArray(fullscreenQuad.Vao);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, sourceTexture);

            // pass screen width / height
            GL.Uniform1(screenWidthLocation, (float)screenWidth);
            GL.Uniform1(screenHeightLocation, (float)screenHeight);




            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, fullscreenQuad.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindVertexArray(0);
            
            GL.Enable(EnableCap.CullFace);
        }


    }
}
