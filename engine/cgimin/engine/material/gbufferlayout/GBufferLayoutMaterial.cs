using System;
using cgimin.engine.camera;
using cgimin.engine.object3d;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace cgimin.engine.material.gbufferlayout
{
    public class GBufferLayoutMaterial : BaseMaterial
    {
        private int modelMatrixLocation;
        private int modelviewProjectionMatrixLocation;

        private int colorTextureLocation;
        private int normalTextureLocation;

        private int roughnessLocation;
        private int metalnessLocation;
        private int emissionLocation;

        public GBufferLayoutMaterial()
        {
            // Shader-Programm wird aus den externen Files generiert...
            CreateShaderProgram("cgimin/engine/material/gbufferlayout/GBufferLayout_VS.glsl",
                                "cgimin/engine/material/gbufferlayout/GBufferLayout_FS.glsl");

            // GL.BindAttribLocation, gibt an welcher Index in unserer Datenstruktur welchem "in" Parameter auf unserem Shader zugeordnet wird
            // folgende Befehle müssen aufgerufen werden...
            GL.BindAttribLocation(Program, 0, "in_position");
            GL.BindAttribLocation(Program, 1, "in_normal");
            GL.BindAttribLocation(Program, 2, "in_uv");
            GL.BindAttribLocation(Program, 3, "in_tangent");
            GL.BindAttribLocation(Program, 4, "in_bitangent");

            // ...bevor das Shader-Programm "gelinkt" wird.
            GL.LinkProgram(Program);

            modelMatrixLocation = GL.GetUniformLocation(Program, "model_matrix");
            modelviewProjectionMatrixLocation = GL.GetUniformLocation(Program, "modelview_projection_matrix");

            colorTextureLocation = GL.GetUniformLocation(Program, "color_texture");
            normalTextureLocation = GL.GetUniformLocation(Program, "normalmap_texture");
            roughnessLocation = GL.GetUniformLocation(Program, "roughness");
            metalnessLocation = GL.GetUniformLocation(Program, "metalness");

            emissionLocation = GL.GetUniformLocation(Program, "emission");
        }

        public void Draw(BaseObject3D object3d, int textureID, int normalTextureID, float roughness, float metalness, float emission = 1.0f)
        {
            GL.UseProgram(Program);

            // Farb-Textur wird "gebunden"
            GL.Uniform1(colorTextureLocation, 0);
            GL.Uniform1(normalTextureLocation, 1);

            // Das Vertex-Array-Objekt unseres Objekts wird benutzt
            GL.BindVertexArray(object3d.Vao);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // Normalmap-Textur wird "gebunden"
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, normalTextureID);

            // Die Matrix, welche wir als "modelview_projection_matrix" übergeben, wird zusammengebaut:
            // Objekt-Transformation * Kamera-Transformation * Perspektivische Projektion der kamera.
            // Auf dem Shader wird jede Vertex-Position mit dieser Matrix multipliziert. Resultat ist die Position auf dem Screen.
            Matrix4 modelViewProjection = object3d.Transformation * Camera.Transformation * Camera.PerspectiveProjection;

            // Die ModelViewProjection Matrix wird dem Shader als Parameter übergeben
            GL.UniformMatrix4(modelviewProjectionMatrixLocation, false, ref modelViewProjection);

            Matrix4 model = object3d.Transformation;
            GL.UniformMatrix4(modelMatrixLocation, false, ref model);

            GL.Uniform1(roughnessLocation, roughness);
            GL.Uniform1(metalnessLocation, metalness);

            // emission value
            GL.Uniform1(emissionLocation, emission);

            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            
            GL.BindVertexArray(0);

            // Active Textur wieder auf 0, um andere Materialien nicht durcheinander zu bringen
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        // implementatin for octree drawing logic
        public override void DrawWithSettings(BaseObject3D object3d, MaterialSettings settings)
        {
            Draw(object3d, settings.colorTexture, settings.normalTexture, settings.roughness, settings.metalness, settings.emission);
        }



    }
}
