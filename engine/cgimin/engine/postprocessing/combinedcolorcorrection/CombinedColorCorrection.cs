using cgimin.engine.object3d;
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using cgimin.engine.camera;
using cgimin.engine.helpers;
using Engine.cgimin.engine.postprocessing;

namespace cgimin.engine.postprocessing.combinedcolorcorrection
{
    public class CombinedColorCorrection : BasePostProcessing
    {
        private int normalColorTextureLocation;
        private int highColorTextureLocation;
        private int exposureLocation;
        private int gammaLocation;

        public CombinedColorCorrection()
        {
            Program = ShaderCompiler.CreateShaderProgram("cgimin/engine/postprocessing/combinedcolorcorrection/CombinedColorCorrection_VS.glsl",
                                                         "cgimin/engine/postprocessing/combinedcolorcorrection/CombinedColorCorrection_FS.glsl");

            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");

            GL.LinkProgram(Program);

            normalColorTextureLocation = GL.GetUniformLocation(Program, "normalColor");
            highColorTextureLocation = GL.GetUniformLocation(Program, "highColor");

            exposureLocation = GL.GetUniformLocation(Program, "exposure");
            gammaLocation = GL.GetUniformLocation(Program, "gamma");

           
        }


        public void Draw(int normalColorTexture, int highColorTexture, float exposure, float gamma)
        {
            GL.Disable(EnableCap.CullFace);

            GL.UseProgram(Program);

            GL.Uniform1(normalColorTextureLocation, 0);
            GL.Uniform1(highColorTextureLocation, 1);

            GL.BindVertexArray(fullscreenQuad.Vao);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, normalColorTexture);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, highColorTexture);

            GL.Uniform1(exposureLocation, exposure);
            GL.Uniform1(gammaLocation, gamma);

            GL.DrawElements(PrimitiveType.Triangles, fullscreenQuad.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindVertexArray(0);

            GL.Enable(EnableCap.CullFace);

        }


    }
}
