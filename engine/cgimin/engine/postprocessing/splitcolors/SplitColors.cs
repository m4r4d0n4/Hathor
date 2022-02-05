using cgimin.engine.object3d;
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using cgimin.engine.camera;
using cgimin.engine.helpers;
using Engine.cgimin.engine.postprocessing;

namespace cgimin.engine.postprocessing.splitcolors
{
    public class SplitColors : BasePostProcessing
    {
        private int sourceTextureLocation;
        private int thresholdLocation;

        public SplitColors()
        {
            Program = ShaderCompiler.CreateShaderProgram("cgimin/engine/postprocessing/splitcolors/SplitColors_VS.glsl",
                                                         "cgimin/engine/postprocessing/splitcolors/SplitColors_FS.glsl");

            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");

            GL.LinkProgram(Program);

            sourceTextureLocation = GL.GetUniformLocation(Program, "sampler");
            thresholdLocation = GL.GetUniformLocation(Program, "threshold");
        }


        public void Draw(int sourceTexture, float brightnessThreshold)
        {
            GL.Disable(EnableCap.CullFace);

            GL.UseProgram(Program);

            GL.Uniform1(sourceTextureLocation, 0);

            GL.BindVertexArray(fullscreenQuad.Vao);
            
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, sourceTexture);

            GL.Uniform1(thresholdLocation, brightnessThreshold);

            GL.DrawElements(PrimitiveType.Triangles, fullscreenQuad.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindVertexArray(0);

            GL.Enable(EnableCap.CullFace);

        }


    }
}
