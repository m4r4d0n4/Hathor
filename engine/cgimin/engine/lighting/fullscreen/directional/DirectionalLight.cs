using cgimin.engine.material;
using cgimin.engine.object3d;
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using cgimin.engine.camera;
using cgimin.engine.helpers;
using cgimin.engine.lighting;
using cgimin.engine.deferred;

namespace engine.lighting.fullscreen.directional
{
    class DirectionalLight : BaseLighting
    {

        private Vector3 lightDirection;
        private Vector3 lightColor;
        private float ambientIntensity;

        private int gNormalLocation;
        private int gPositionLocation;
        private int GColorAndRoughnessLocation;
        private int GMetalnessAndShadowLocation;

        private int cameraPosLocation;

        private int lightDirectionLocation;
        private int lightColorLocation;

        private int ambientIntensityLocation;

        public DirectionalLight()
        {
            // Shader-Programm loaded from external files
            Program = ShaderCompiler.CreateShaderProgram("cgimin/engine/lighting/fullscreen/directional/DirectionalLight_VS.glsl",
                                                         "cgimin/engine/lighting/fullscreen/directional/DirectionalLight_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            gNormalLocation = GL.GetUniformLocation(Program, "GNormal");
            gPositionLocation = GL.GetUniformLocation(Program, "GPosition");
            GColorAndRoughnessLocation = GL.GetUniformLocation(Program, "GColorAndRoughness");
            GMetalnessAndShadowLocation = GL.GetUniformLocation(Program, "GMetalness");

            cameraPosLocation = GL.GetUniformLocation(Program, "cameraPosition");

            lightDirectionLocation = GL.GetUniformLocation(Program, "lightDirection");
            lightColorLocation = GL.GetUniformLocation(Program, "lightColor");

            ambientIntensityLocation = GL.GetUniformLocation(Program, "ambientIntensity");
        }


        public void SetProperties(Vector3 direction, Vector3 color, float ambientIntensity)
        {
            this.lightDirection = Vector3.Normalize(direction);
            this.lightColor = color;
            this.ambientIntensity = ambientIntensity;
        }

        public void Draw(BaseObject3D object3d)
        {
            GL.UseProgram(Program);

            // das Vertex-Array-Objekt unseres Objekts wird benutzt
            GL.BindVertexArray(object3d.Vao);

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

            // Die Licht Parameter werden übergeben
            GL.Uniform3(lightDirectionLocation, lightDirection);
            GL.Uniform3(lightColorLocation, lightColor);
            GL.Uniform1(ambientIntensityLocation, ambientIntensity);

            GL.Uniform3(cameraPosLocation, Camera.Position);

            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

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
