using cgimin.engine.material;
using cgimin.engine.object3d;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using cgimin.engine.camera;
using System;
using cgimin.engine.helpers;
using cgimin.engine.lighting;
using cgimin.engine.deferred;

namespace cgimin.lighting.local
{
    class PointLight : BaseLighting
    {
        private int modelviewProjectionMatrixLocation;

        private int midPositionLocation;
        private int radiusLocation;
        private int colorLocation;
        private int cameraPosLocation;

        private int gNormalLocation;
        private int gPositionLocation;
        private int GColorAndRoughnessLocation;
        private int GMetalnessAndShadowLocation;

        private int screenWidthLocation;
        private int screenHeightLocation;

        public PointLight()
        {
            // Shader-Programm loaded from external files
            Program = ShaderCompiler.CreateShaderProgram("cgimin/engine/lighting/local/pointlight/PointLight_VS.glsl",
                                                         "cgimin/engine/lighting/local/pointlight/PointLight_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            modelviewProjectionMatrixLocation = GL.GetUniformLocation(Program, "modelview_projection_matrix");
            midPositionLocation = GL.GetUniformLocation(Program, "midPosition");
            radiusLocation = GL.GetUniformLocation(Program, "radius");
            colorLocation = GL.GetUniformLocation(Program, "color");
            gNormalLocation = GL.GetUniformLocation(Program, "GNormal");
            gPositionLocation = GL.GetUniformLocation(Program, "GPosition");
            GColorAndRoughnessLocation = GL.GetUniformLocation(Program, "GColorAndRoughness");
            GMetalnessAndShadowLocation = GL.GetUniformLocation(Program, "GMetalness");

            screenWidthLocation = GL.GetUniformLocation(Program, "screenWidth");
            screenHeightLocation = GL.GetUniformLocation(Program, "screenHeight");

            cameraPosLocation = GL.GetUniformLocation(Program, "camera_position");

        }


        public void Draw(BaseObject3D sphereObj, Vector3 pos, Vector3 color, float radius, int screenWidth, int screenHeight)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.UseProgram(Program);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);


            GL.BindVertexArray(sphereObj.Vao);

            // Unser Shader Programm wird benutzt
            GL.UseProgram(Program);

            GL.Uniform1(gNormalLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GNormalBuffer);

            GL.Uniform1(gPositionLocation, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GPositionBuffer);

            GL.Uniform1(GColorAndRoughnessLocation, 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GColorRoughnessBuffer);

            GL.Uniform1(GMetalnessAndShadowLocation, 3);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, DeferredRendering.GMetalnessShadowBuffer);

            Matrix4 model = Matrix4.Identity;
            model *= Matrix4.CreateScale(radius, radius, radius);
            model *= Matrix4.CreateTranslation(pos);

            Matrix4 modelviewProjection = model * Camera.Transformation * Camera.PerspectiveProjection;

            GL.UniformMatrix4(modelviewProjectionMatrixLocation, false, ref modelviewProjection);

            Vector3 midPos = new Vector3(0, 0, 0);
            midPos = Vector3.TransformPosition(midPos, model);
            GL.Uniform3(midPositionLocation, ref midPos);

            GL.Uniform1(radiusLocation, radius);
            GL.Uniform3(colorLocation, color);

            GL.Uniform1(screenWidthLocation, (float)screenWidth);
            GL.Uniform1(screenHeightLocation, (float)screenHeight);

            GL.Uniform3(cameraPosLocation, new Vector3(Camera.Position.X, Camera.Position.Y, Camera.Position.Z));

            // light is drawn
            GL.DrawElements(PrimitiveType.Triangles, sphereObj.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // cleanup
            GL.BindVertexArray(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 1);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, 2);

            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, 3);

        }


    }
}
