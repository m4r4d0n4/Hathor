using System;
using cgimin.engine.camera;
using cgimin.engine.object3d;
using Engine.cgimin.engine.shadowmapping;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace cgimin.engine.material.gbufferreceiveshadowmask
{
    public class GBufferReceiveShadowMaskMaterial : BaseMaterial
    {
        private int modelMatrixLocation;
        private int modelViewMatrixLocation;
        private int modelviewProjectionMatrixLocation;

        private int colorTextureLocation;
        private int normalTextureLocation;
        private int maskTextureLocation;
        private int emissionLocation;
        private int dist1Location;
        private int dist2Location;
        private int dist3Location;

        private int DepthBiasMVPLocation1;
        private int DepthBiasMVPLocation2;
        private int DepthBiasMVPLocation3;

        private int shadowTextureLocation1;
        private int shadowTextureLocation2;
        private int shadowTextureLocation3;

        public GBufferReceiveShadowMaskMaterial()
        {
            // Shader-Programm wird aus den externen Files generiert...
            CreateShaderProgram("cgimin/engine/material/gbufferreceiveshadowmask/GBufferReceiveShadowMask_VS.glsl",
                                "cgimin/engine/material/gbufferreceiveshadowmask/GBufferReceiveShadowMask_FS.glsl");

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
            modelViewMatrixLocation = GL.GetUniformLocation(Program, "modelview_matrix");
            modelviewProjectionMatrixLocation = GL.GetUniformLocation(Program, "modelview_projection_matrix");

            colorTextureLocation = GL.GetUniformLocation(Program, "color_texture");
            normalTextureLocation = GL.GetUniformLocation(Program, "normalmap_texture");
            maskTextureLocation = GL.GetUniformLocation(Program, "maskmap_texture");

            emissionLocation = GL.GetUniformLocation(Program, "emission");

            // parameters for shadow mapping
            dist1Location = GL.GetUniformLocation(Program, "dist1");
            dist2Location = GL.GetUniformLocation(Program, "dist2");
            dist3Location = GL.GetUniformLocation(Program, "dist3");

            DepthBiasMVPLocation1 = GL.GetUniformLocation(Program, "DepthBiasMVP1");
            DepthBiasMVPLocation2 = GL.GetUniformLocation(Program, "DepthBiasMVP2");
            DepthBiasMVPLocation3 = GL.GetUniformLocation(Program, "DepthBiasMVP3");

            shadowTextureLocation1 = GL.GetUniformLocation(Program, "shadowmap_texture1");
            shadowTextureLocation2 = GL.GetUniformLocation(Program, "shadowmap_texture2");
            shadowTextureLocation3 = GL.GetUniformLocation(Program, "shadowmap_texture3");

        }

        public void Draw(BaseObject3D object3d, int textureID, int normalTextureID, int maskTextureID, float emission = 1.0f)
        {
            GL.UseProgram(Program);

            // Farb-Textur wird "gebunden"
            GL.Uniform1(colorTextureLocation, 0);
            GL.Uniform1(normalTextureLocation, 1);
            GL.Uniform1(maskTextureLocation, 2);

            // Das Vertex-Array-Objekt unseres Objekts wird benutzt
            GL.BindVertexArray(object3d.Vao);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // Normalmap-Textur wird "gebunden"
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, normalTextureID);

            // Maskmap-Textur wird "gebunden"
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, maskTextureID);

            // binding of cascaded shadow maps 
            GL.Uniform1(shadowTextureLocation1, 3);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, CascadedShadowMapping.cascades[0].depthTexture);

            GL.Uniform1(shadowTextureLocation2, 4);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, CascadedShadowMapping.cascades[1].depthTexture);

            GL.Uniform1(shadowTextureLocation3, 5);
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, CascadedShadowMapping.cascades[2].depthTexture);
            
            // model, modelview & modelviewprojection matrix
            Matrix4 modelViewProjection = object3d.Transformation * Camera.Transformation * Camera.PerspectiveProjection;
            GL.UniformMatrix4(modelviewProjectionMatrixLocation, false, ref modelViewProjection);

            Matrix4 modelView = object3d.Transformation * Camera.Transformation;
            GL.UniformMatrix4(modelViewMatrixLocation, false, ref modelView);

            Matrix4 model = object3d.Transformation;
            GL.UniformMatrix4(modelMatrixLocation, false, ref model);

            // emission value
            GL.Uniform1(emissionLocation, emission);

            // Shadow Mapping
            GL.Uniform1(dist1Location, CascadedShadowMapping.cascades[0].borderDistance);
            GL.Uniform1(dist2Location, CascadedShadowMapping.cascades[1].borderDistance);
            GL.Uniform1(dist3Location, CascadedShadowMapping.cascades[2].borderDistance);

            Matrix4 depthMVP = object3d.Transformation * CascadedShadowMapping.cascades[0].shadowTransformation * CascadedShadowMapping.cascades[0].depthBias * CascadedShadowMapping.cascades[0].shadowProjection;
            GL.UniformMatrix4(DepthBiasMVPLocation1, false, ref depthMVP);

            Matrix4 depthMVP2 = object3d.Transformation * CascadedShadowMapping.cascades[1].shadowTransformation * CascadedShadowMapping.cascades[1].depthBias * CascadedShadowMapping.cascades[1].shadowProjection;
            GL.UniformMatrix4(DepthBiasMVPLocation2, false, ref depthMVP2);

            Matrix4 depthMVP3 = object3d.Transformation * CascadedShadowMapping.cascades[2].shadowTransformation * CascadedShadowMapping.cascades[2].depthBias * CascadedShadowMapping.cascades[2].shadowProjection;
            GL.UniformMatrix4(DepthBiasMVPLocation3, false, ref depthMVP3);

            // Das Objekt wird gezeichnet
            GL.DrawElements(PrimitiveType.Triangles, object3d.Indices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            
            GL.BindVertexArray(0);

            // Active Textur wieder auf 0, um andere Materialien nicht durcheinander zu bringen
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        // implementatin for octree drawing logic
        public override void DrawWithSettings(BaseObject3D object3d, MaterialSettings settings)
        {
            Draw(object3d, settings.colorTexture, settings.normalTexture, settings.maskTexture, settings.emission);
        }



    }
}
