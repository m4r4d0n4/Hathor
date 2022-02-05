using cgimin.engine.camera;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.cgimin.engine.shadowmapping
{
    public class CascadedShadowMapping
    {
        private const int numTextures = 3;

        public struct Cascade
        {
            public int depthTexture;
            public Matrix4 depthBias;
            public Matrix4 shadowTransformation;
            public Matrix4 shadowProjection;

            public float cascadeXYSize;
            public float cascadeZSize;
            public float cascadeDistance;
            public float borderDistance;
        }

        public static Cascade[] cascades { get; private set; }

        private static int FramebufferName;

        private static Vector3 cameraStartPosition;
        private static Vector3 nearNormal;

        private static float dist1;
        private static float dist2;
        private static float dist3;
        private static float minZdist;
        private static List<int> textureSizes;

        public static Vector3 LightDirection;

        public static void Init(int textureDimension1, int textureDimension2, int textureDimension3, float distance1, float distance2, float distance3, float minZdistance)
        {

            dist1 = distance1;
            dist2 = distance2;
            dist3 = distance3;
            minZdist = minZdistance;

            cascades = new Cascade[numTextures];

            textureSizes = new List<int>();
            textureSizes.Add(textureDimension1);
            textureSizes.Add(textureDimension2);
            textureSizes.Add(textureDimension3);

            FramebufferName = 0;
            GL.GenFramebuffers(1, out FramebufferName);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferName);

            int[] depthTextures = new int[numTextures];
            GL.GenTextures(numTextures, depthTextures);

            for (int i = 0; i < numTextures; i++)
            {
                GL.BindTexture(TextureTarget.Texture2D, depthTextures[i]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, textureSizes[i], textureSizes[i], 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Lequal);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRToTexture);

                cascades[i].depthTexture = depthTextures[i];
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, cascades[i].depthTexture, 0);
            }

            //GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, cascades[0].depthTexture, 0);
            GL.DrawBuffer(DrawBufferMode.None);

            SetLightDirection(new Vector3(1, -1, 1));
        }

        public static void StartShadowMapping()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferName);

            DrawBuffersEnum[] drawEnum = {};
            GL.DrawBuffers(0, drawEnum);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            cameraStartPosition = Camera.Position;
            nearNormal = Camera.Planes[0].normal;

            float dist0 = 0.0f;

            cascades[0].borderDistance = dist1;
            cascades[1].borderDistance = dist2;
            cascades[2].borderDistance = dist3;

            cascades[0].cascadeDistance = (dist0 + dist1) * 0.5f;
            cascades[1].cascadeDistance = (dist1 + dist2) * 0.5f;
            cascades[2].cascadeDistance = (dist2 + dist3) * 0.5f;

            //float angleDist = (float)Math.Tan(Camera.Fov / 2.0 * Math.PI / 180.0f);
            float angleDist = (float)Math.Tan(90 / 2.0 * Math.PI / 180.0f);

            float xDist = angleDist * dist1;
            cascades[0].cascadeXYSize = (float)Math.Sqrt(xDist * xDist + xDist * xDist + (cascades[0].cascadeDistance - dist1) * (cascades[0].cascadeDistance - dist1));
            cascades[0].cascadeZSize = cascades[0].cascadeXYSize;
            if (cascades[0].cascadeZSize < minZdist) cascades[0].cascadeZSize = minZdist;

            xDist = angleDist * dist2;
            cascades[1].cascadeXYSize = (float)Math.Sqrt(xDist * xDist + xDist * xDist + (cascades[1].cascadeDistance - dist2) * (cascades[1].cascadeDistance - dist2));
            cascades[1].cascadeZSize = cascades[1].cascadeXYSize;
            if (cascades[1].cascadeZSize < minZdist) cascades[1].cascadeZSize = minZdist;

            xDist = angleDist * dist3;
            cascades[2].cascadeXYSize = (float)Math.Sqrt(xDist * xDist + xDist * xDist + (cascades[2].cascadeDistance - dist3) * (cascades[2].cascadeDistance - dist3));
            cascades[2].cascadeZSize = cascades[2].cascadeXYSize;
            if (cascades[2].cascadeZSize < minZdist) cascades[2].cascadeZSize = minZdist;

        }

        public static void SetDepthTextureTarget(int target)
        {
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, cascades[target].depthTexture, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 ddProjection = new Matrix4();
            Matrix4.CreateOrthographicOffCenter(-cascades[target].cascadeXYSize, cascades[target].cascadeXYSize, -cascades[target].cascadeXYSize, cascades[target].cascadeXYSize, -cascades[target].cascadeZSize, cascades[target].cascadeZSize, out ddProjection);
            Camera.SetProjectionMatrix(ddProjection);

            GL.Viewport(0, 0, textureSizes[target], textureSizes[target]);

            Vector3 textCamPosition = cameraStartPosition + nearNormal * cascades[target].cascadeDistance;
            Camera.SetTransformMatrix(Matrix4.LookAt(textCamPosition, textCamPosition - LightDirection, new Vector3(0, 1, 0)));
            Camera.CreateViewFrustumPlanes(Camera.Transformation * Camera.PerspectiveProjection);

            cascades[target].shadowTransformation = Camera.Transformation;
            cascades[target].shadowProjection = ddProjection;

            cascades[target].depthBias = Matrix4.CreateScale(0.5f, 0.5f, 0.5f);
            cascades[target].depthBias *= Matrix4.CreateTranslation(cascades[target].cascadeXYSize * 0.5f, cascades[target].cascadeXYSize * 0.5f, -cascades[target].cascadeZSize * 0.5f);

        }

        public static void EndShadowMapping()
        {
            Camera.SetBackToLastCameraSettings();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
        }

        public static void SetLightDirection(Vector3 direction)
        {
            LightDirection = direction.Normalized();
        }


    }
}
