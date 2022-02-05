using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using cgimin.engine.camera;

namespace cgimin.engine.material.castshadow
{
    public class CastShadowMaterial : BaseMaterial
    {

        private int depthMVPLocation;

        public CastShadowMaterial()
        {
            // Shader-Programm wird aus den externen Files generiert...
            CreateShaderProgram("cgimin/engine/material/castshadow/CastShadow_VS.glsl",
                                "cgimin/engine/material/castshadow/CastShadow_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            depthMVPLocation = GL.GetUniformLocation(Program, "depthMVP");
        }

        public void Draw(BaseObject3D object3d)
        {
            GL.UseProgram(Program);

            Matrix4 depthMVPShadow = object3d.Transformation * Camera.Transformation * Camera.PerspectiveProjection;
            GL.UniformMatrix4(depthMVPLocation, false, ref depthMVPShadow);
            GL.BindVertexArray(object3d.Vao);

            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindVertexArray(0);
        }


        public override void DrawWithSettings(BaseObject3D object3d, MaterialSettings settings)
        {
            Draw(object3d);
        }


    }
}
