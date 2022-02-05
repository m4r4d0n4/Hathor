using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.texture;
using cgimin.engine.helpers;
using cgimin.engine.object3d;
using cgimin.engine.camera;
using cgimin.engine.lighting;

namespace Engine.cgimin.engine.material.ibl
{
    class FullscreenIBL : BaseLighting
    {
        private int gColorRoughnessLocation;
        private int gNormalLocation;
        private int gPositionLocation;
        private int gMetalnessAndShadowLocation;

        private int iblSpcularCubeLocation;
        private int iblIrradianceCubeLocation;

        private int cameraPosLocation;

        private int iblLookupTexture;
        private int iblLookupTextureLocation;

        public FullscreenIBL()
        {
            iblLookupTexture = TextureManager.LoadTexture("cgimin/engine/lighting/fullscreen/ibl/ibl_brdf_lut.png");

            Program = ShaderCompiler.CreateShaderProgram("cgimin/engine/lighting/fullscreen/ibl/FullscreenIBL_VS.glsl", "cgimin/engine/lighting/fullscreen/ibl/FullscreenIBL_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            gColorRoughnessLocation = GL.GetUniformLocation(Program, "gColorRoughness");
            gNormalLocation = GL.GetUniformLocation(Program, "gNormal");
            gPositionLocation = GL.GetUniformLocation(Program, "gPosition");
            gMetalnessAndShadowLocation = GL.GetUniformLocation(Program, "gMetalAndShadow");

            iblSpcularCubeLocation = GL.GetUniformLocation(Program, "iblSpecular");
            iblIrradianceCubeLocation = GL.GetUniformLocation(Program, "iblIrradiance");

            cameraPosLocation = GL.GetUniformLocation(Program, "camera_position");

            iblLookupTextureLocation = GL.GetUniformLocation(Program, "brdfLUT");
        }

        public void Draw(BaseObject3D object3d, int colorRoughnessBufferTexID, int normalTexID, int positionTexID, int metalnessTexID, int iblSpecularCube, int iblIrradianceCube)
        {
            // das Vertex-Array-Objekt unseres Objekts wird benutzt
            GL.BindVertexArray(object3d.Vao);

            // Unser Shader Programm wird benutzt
            GL.UseProgram(Program);

            // Farb-Textur wird "gebunden"
            GL.Uniform1(gColorRoughnessLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, colorRoughnessBufferTexID);

            GL.Uniform1(gNormalLocation, 1);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, normalTexID);

            GL.Uniform1(gPositionLocation, 2);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, positionTexID);

            GL.Uniform1(gMetalnessAndShadowLocation, 3);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, metalnessTexID);

            GL.Uniform1(iblLookupTextureLocation, 5);
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, iblLookupTexture);

            // Cube Mapping -Textur wird "gebunden"
            GL.Uniform1(iblSpcularCubeLocation, 6);
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.TextureCubeMap, iblSpecularCube);
           
            GL.Uniform1(iblIrradianceCubeLocation, 7);
            GL.ActiveTexture(TextureUnit.Texture7);
            GL.BindTexture(TextureTarget.TextureCubeMap, iblIrradianceCube);
           
            // Positions Parameter
            GL.Uniform4(cameraPosLocation, new Vector4(Camera.Position.X, Camera.Position.Y, Camera.Position.Z, 1));

            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // Unbinden des Vertex-Array-Objekt damit andere Operation nicht darauf basieren
            GL.BindVertexArray(0);

            // Active Textur wieder auf 0, um andere Materialien nicht durcheinander zu bringen
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }


     
    }
}
