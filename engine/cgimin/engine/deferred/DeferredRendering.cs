using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using cgimin.engine.object3d;
using cgimin.engine.fullscreensimple;
using cgimin.lighting.local;
using engine.lighting.fullscreen.directional;
using Engine.cgimin.engine.material.ibl;
using System.Collections.Generic;

namespace cgimin.engine.deferred
{

    public class DeferredRendering
    {

        // G Buffers
        private static int GFramebufferName;
        private static int width;
        private static int height;
        public static int GColorRoughnessBuffer;
        public static int GPositionBuffer;
        public static int GNormalBuffer;
        public static int GMetalnessShadowBuffer;
        public static int GGlowBuffer;

        // Offscreen Buffer
        private static int OffScreenBufferName;
        public static int OffScreenBufferTexture;

        // Split Color Buffer
        private static int SplitColorBufferName;
        public static int SplitNormalColorBufferTexture;
        public static int SplitHighColorBufferTexture;

        // Blur ping pong Buffer
        private static List<int> BlurPingPongBufferName;
        public static List<int> BlurPingPongBufferTexture;

        // Objects for deffered output
        private static BaseObject3D fullscreenQuad;
        private static ObjLoaderObject3D pointLightObject;

        // Debug Fullscreen Material
        private static FullscreenSimple fullscreenSimple;

        // Fullscreen Materials
        private static FullscreenIBL fullscreenIBL;
        private static DirectionalLight directionalLight;
        private static PointLight pointLight;

        public static void Init(int screenWidth, int screenHeight)
        {
            width = screenWidth;
            height = screenHeight;

            fullscreenIBL = new FullscreenIBL();
            directionalLight = new DirectionalLight();
            pointLight = new PointLight();
            fullscreenSimple = new FullscreenSimple();

            // the fullscreen quad object
            fullscreenQuad = new BaseObject3D();
            fullscreenQuad.AddTriangle(new Vector3(1, -1, 0), new Vector3(-1, -1, 0), new Vector3(1, 1, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(1, 1));
            fullscreenQuad.AddTriangle(new Vector3(-1, -1, 0), new Vector3(1, 1, 0), new Vector3(-1, 1, 0), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1));
            fullscreenQuad.CreateVAO();

            //point light object
            pointLightObject = new ObjLoaderObject3D("cgimin/engine/lighting/local/pointlight/sphere.obj", 1.0f, true);

            // Offscreen Buffer 
            OffScreenBufferName = 0;
            GL.GenFramebuffers(1, out OffScreenBufferName);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, OffScreenBufferName);
            GL.GenTextures(1, out OffScreenBufferTexture);

            GL.BindTexture(TextureTarget.Texture2D, OffScreenBufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, screenWidth, screenHeight, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, OffScreenBufferTexture, 0);

            int offscreenDepthRenderbuffer;
            GL.GenRenderbuffers(1, out offscreenDepthRenderbuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, offscreenDepthRenderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, screenWidth, screenHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, offscreenDepthRenderbuffer);

            // Split to normal and high color range buffer
            SplitColorBufferName = 0;
            GL.GenFramebuffers(1, out SplitColorBufferName);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, SplitColorBufferName);

            GL.GenTextures(1, out SplitNormalColorBufferTexture);
            GL.BindTexture(TextureTarget.Texture2D, SplitNormalColorBufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, screenWidth, screenHeight, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, SplitNormalColorBufferTexture, 0);

            GL.GenTextures(1, out SplitHighColorBufferTexture);
            GL.BindTexture(TextureTarget.Texture2D, SplitHighColorBufferTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, screenWidth, screenHeight, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, SplitHighColorBufferTexture, 0);

            // Blur ping pong buffer
            int pingPongBuffer1Name = 0;
            int pingPongBuffer1Texture = 0;
            GL.GenFramebuffers(1, out pingPongBuffer1Name);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, pingPongBuffer1Name);
            GL.GenTextures(1, out pingPongBuffer1Texture);

            GL.BindTexture(TextureTarget.Texture2D, pingPongBuffer1Texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, screenWidth, screenHeight, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, pingPongBuffer1Texture, 0);
            
            int pingPongBuffer2Name = 0;
            int pingPongBuffer2Texture = 0;
            GL.GenFramebuffers(1, out pingPongBuffer2Name);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, pingPongBuffer2Name);
            GL.GenTextures(1, out pingPongBuffer2Texture);

            GL.BindTexture(TextureTarget.Texture2D, pingPongBuffer2Texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, screenWidth, screenHeight, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, pingPongBuffer2Texture, 0);

            BlurPingPongBufferName = new List<int>();
            BlurPingPongBufferName.Add(pingPongBuffer1Name);
            BlurPingPongBufferName.Add(pingPongBuffer2Name);

            BlurPingPongBufferTexture = new List<int>();
            BlurPingPongBufferTexture.Add(pingPongBuffer1Texture);
            BlurPingPongBufferTexture.Add(pingPongBuffer2Texture);


            // G Buffers
            GFramebufferName = 0;
            GL.GenFramebuffers(1, out GFramebufferName);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, GFramebufferName);

            GL.GenTextures(1, out GColorRoughnessBuffer);
            GL.BindTexture(TextureTarget.Texture2D, GColorRoughnessBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, screenWidth, screenHeight, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, GColorRoughnessBuffer, 0);

            GL.GenTextures(1, out GPositionBuffer);
            GL.BindTexture(TextureTarget.Texture2D, GPositionBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, screenWidth, screenHeight, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, GPositionBuffer, 0);

            GL.GenTextures(1, out GNormalBuffer);
            GL.BindTexture(TextureTarget.Texture2D, GNormalBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, screenWidth, screenHeight, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, GNormalBuffer, 0);

            GL.GenTextures(1, out GMetalnessShadowBuffer);
            GL.BindTexture(TextureTarget.Texture2D, GMetalnessShadowBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rg16f, screenWidth, screenHeight, 0, PixelFormat.Rg, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, GMetalnessShadowBuffer, 0);

            GL.GenTextures(1, out GGlowBuffer);
            GL.BindTexture(TextureTarget.Texture2D, GGlowBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, screenWidth, screenHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment4, GGlowBuffer, 0);


            DrawBuffersEnum[] drawEnum = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3, DrawBuffersEnum.ColorAttachment4 };
            GL.DrawBuffers(5, drawEnum);

            int depthrenderbuffer;
            GL.GenRenderbuffers(1, out depthrenderbuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthrenderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, screenWidth, screenHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthrenderbuffer);

            FramebufferErrorCode eCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (eCode != FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("GBuffer init wrong" + eCode.ToString());
            }
            else
            {
                Console.WriteLine("GBuffer init Correct");
            }

        }


        public static void ClearCurrentBuffer()
        {
            GL.ClearColor(new Color4(0, 0, 0, 0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public static void StartGBufferRendering()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, GFramebufferName);
            DrawBuffersEnum[] drawEnum = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3, DrawBuffersEnum.ColorAttachment4 };
            GL.DrawBuffers(5, drawEnum);

            GL.ClearColor(new Color4(0, 0, 0, 0));
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, width, height);
        }


        public static void StartBlurPingPongBufferRendering(int index)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, BlurPingPongBufferName[index]);
            DrawBuffersEnum[] drawEnum = { DrawBuffersEnum.ColorAttachment0 };
            GL.DrawBuffers(1, drawEnum);
            GL.Viewport(0, 0, width, height);
        }


        public static void StartSplitHighColorRendering()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, SplitColorBufferName);
            DrawBuffersEnum[] drawEnum = { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 };
            GL.DrawBuffers(2, drawEnum);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, width, height);
        }

        public static void StartOffscreenBufferRendering()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, OffScreenBufferName);
            DrawBuffersEnum[] drawEnum = { DrawBuffersEnum.ColorAttachment0};
            GL.DrawBuffers(1, drawEnum);
            GL.Viewport(0, 0, width, height);
        }

        public static void StartScreenRendering()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(new Color4(0, 0, 0, 0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, width, height);
            GL.Disable(EnableCap.DepthTest);
        }


        public static void DrawFullscreenTexture(int tex)
        {
            GL.ClearColor(new Color4(1, 0, 0, 0));
            GL.Disable(EnableCap.DepthTest);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.CullFace);

            GL.Viewport(0, 0, width, height);

            fullscreenSimple.Draw(fullscreenQuad, tex);
        }

        public static void DrawDirectionalLight()
        {
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            directionalLight.Draw(fullscreenQuad);
            GL.Enable(EnableCap.CullFace);
        }

        public static void DrawFullscreenIBL(int iblSpecularCube, int iblIrradianceCube)
        {
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            fullscreenIBL.Draw(fullscreenQuad, GColorRoughnessBuffer, GNormalBuffer, GPositionBuffer, GMetalnessShadowBuffer, iblSpecularCube, iblIrradianceCube);
            GL.Enable(EnableCap.CullFace);
        }

        public static void CopyDepthToOffscreenBuffer()
        {
            // copy depth to main screen
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, GFramebufferName);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, OffScreenBufferName);
            GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
      
            // enable z-buffer again
            GL.Enable(EnableCap.DepthTest);
        }

        public static void DrawDebugFullscreen(int mode)
        {

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.ClearColor(new Color4(1, 0, 0, 0));
            GL.Disable(EnableCap.DepthTest);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.CullFace);

            GL.Viewport(0, 0, width, height);

            if (mode == 0) fullscreenSimple.Draw(fullscreenQuad, GColorRoughnessBuffer);
            if (mode == 1) fullscreenSimple.Draw(fullscreenQuad, GPositionBuffer);
            if (mode == 2) fullscreenSimple.Draw(fullscreenQuad, GNormalBuffer);
            if (mode == 3) fullscreenSimple.Draw(fullscreenQuad, GMetalnessShadowBuffer);
            if (mode == 4) fullscreenSimple.Draw(fullscreenQuad, SplitHighColorBufferTexture);
            if (mode == 5) fullscreenSimple.Draw(fullscreenQuad, OffScreenBufferTexture);


            GL.Enable(EnableCap.CullFace);
        }

        public static void DrawDebugTexture(int tex)
        {

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.ClearColor(new Color4(1, 0, 0, 0));
            GL.Disable(EnableCap.DepthTest);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.CullFace);

            GL.Viewport(0, 0, width, height);

            fullscreenSimple.Draw(fullscreenQuad, tex);

            GL.Enable(EnableCap.CullFace);
        }

        public static void DrawPointLight(Vector3 pos, Vector3 color, float radius)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            pointLight.Draw(pointLightObject, pos, color, radius, width, height);

        }

        public static void SetDirectionalLight(Vector3 direction, Vector3 color, float ambientIntensity)
        {
            directionalLight.SetProperties(direction, color, ambientIntensity);
        }



    }
}
