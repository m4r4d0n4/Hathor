

#region --- Using Directives ---

using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using cgimin.engine.texture;
using cgimin.engine.camera;
using OpenTK.Input;
using System.Collections.Generic;
using cgimin.engine.material.gbufferlayout;
using cgimin.engine.deferred;
using static cgimin.engine.material.BaseMaterial;
using cgimin.engine.octree;
using cgimin.engine.postprocessing.fxaa;
using cgimin.engine.postprocessing.splitcolors;
using cgimin.engine.postprocessing.combinedcolorcorrection;
using cgimin.engine.postprocessing.blur;
using cgimin.engine.gui;
using Engine.cgimin.engine.shadowmapping;
using cgimin.engine.material.castshadow;
using cgimin.engine.material.gbufferreceiveshadowlayout;
using cgimin.engine.material.gbufferreceiveshadowmask;
using Engine.cgimin.engine.skybox;

#endregion --- Using Directives ---

namespace Examples.Tutorial
{

    public class ExampleProject : GameWindow
    {
        public static int SCREEN_WIDTH = 1920;
        public static int SCREEN_HEIGHT = 1080;

        // the objects we load
        private ObjLoaderObject3D torusObject;
        private ObjLoaderObject3D steampunkGunObject;
        private ObjLoaderObject3D wallObject;
        private ObjLoaderObject3D cubeObject;

        // our textur-IDs
        private int singleColorTexture;
        private int singleNormalTexture;

        private int gunAlbedoTexture;
        private int gunNormalTexture;
        private int gunMaskTexture;

        private int wallNormalTexture;
        private int wallColorTexture;

        // global update counter for animations etc.
        private int updateCounter = 0;

        // switch to select display
        private int displaySwitch;

        // materials
        private GBufferReceiveShadowLayoutMaterial gBufferReceiveShadowLayoutMaterial;
        private GBufferReceiveShadowMaskMaterial gBufferReceiveShadowMaskMaterial;
        private GBufferLayoutMaterial gBufferLayoutMaterial;
        private GBufferMaskMaterial gBufferMaskMaterial;
        private CastShadowMaterial castShadowMaterial;

        // the octree
        private Octree octree;

        // post processing
        private FXAA postProcessing_FXAA;
        private SplitColors postProcessing_SplitColors;
        private CombinedColorCorrection postProcessing_CombinedColorCorrection;
        private Blur postProcessing_Blur;

        // Keys
        private bool keyLeft;
        private bool keyRight;
        private bool keyUp;
        private bool keyDown;
        private bool keyW;
        private bool keyS;
        private bool keyA;
        private bool keyD;

        // store mouse positions
        private int currentMouseX = 0;
        private int currentMouseY = 0;
        private int oldMouseX = 0;
        private int oldMouseY = 0;

        // IBL
        private int IBLSpecularSundown;
        private int IBLIrradianceSundown;

        // Skybox
        private SkyBox skyBox;

        // 2D Elements
        private BitmapGraphic logoSprite;
        private BitmapFont bitmapFont;

        private Random random = new Random();

        public ExampleProject()
            : base(1920, 1080, new GraphicsMode(32, 24, 8, 2), "CGI-MIN Example", GameWindowFlags.Fullscreen, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        { }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Initialize Camera
            Camera.Init();
            Camera.SetWidthHeightFov(1280, 720, 60);
            Camera.SetLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);

            // init 2d content
            int spriteTexture = TextureManager.LoadTexture("data/textures/sprites.png");
            logoSprite = new BitmapGraphic(spriteTexture, 256, 256, 10, 110, 196, 120);
            bitmapFont = new BitmapFont("data/fonts/abel_normal.fnt", "data/fonts/abel_normal.png");

            // Init Shadow Mapping
            CascadedShadowMapping.Init(4096, 4096, 1024, 15, 40, 120, 1);

            // Loading 3d objects
            torusObject = new ObjLoaderObject3D("data/objects/torus_smooth.obj", 5f, true);
            steampunkGunObject = new ObjLoaderObject3D("data/objects/steampunk_gun.obj", 15f, false);
            wallObject = new ObjLoaderObject3D("data/objects/thin_wall.obj", 30.0f, false);
            cubeObject = new ObjLoaderObject3D("data/objects/cube.obj", 2.0f, false);

            // Loading textures
            singleColorTexture = TextureManager.LoadTexture("data/textures/single_color.png");
            singleNormalTexture = TextureManager.LoadTexture("data/textures/single_normal.png");

            gunAlbedoTexture = TextureManager.LoadTexture("data/textures/Gun_01_albedo.png");
            gunNormalTexture = TextureManager.LoadTexture("data/textures/Gun_01_normal.png");
            gunMaskTexture = TextureManager.LoadTexture("data/textures/Gun_01_mask.png");

            wallNormalTexture = TextureManager.LoadTexture("data/textures/stone_normal.png");
            wallColorTexture = TextureManager.LoadTexture("data/textures/orange.png");

            // Load IBL Texture cubes       
            IBLSpecularSundown = TextureManager.LoadIBLSpecularMap("data/ibl/night", "bmp");
            IBLIrradianceSundown = TextureManager.LoadIBLIrradiance("data/ibl/night", "bmp");

            // SkyBox
            skyBox = new SkyBox("data/ibl/night_c05.bmp", "data/ibl/night_c04.bmp", "data/ibl/night_c01.bmp", "data/ibl/night_c00.bmp", "data/ibl/night_c02.bmp", "data/ibl/night_c03.bmp");

            // post processing effects
            postProcessing_FXAA = new FXAA();
            postProcessing_SplitColors = new SplitColors();
            postProcessing_CombinedColorCorrection = new CombinedColorCorrection();
            postProcessing_Blur = new Blur();

            // enable z-buffer
            GL.Enable(EnableCap.DepthTest);

            // backface culling enabled
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            // init materials
            gBufferReceiveShadowLayoutMaterial = new GBufferReceiveShadowLayoutMaterial();
            gBufferReceiveShadowMaskMaterial = new GBufferReceiveShadowMaskMaterial();
            gBufferLayoutMaterial = new GBufferLayoutMaterial();
            gBufferMaskMaterial = new GBufferMaskMaterial();
            castShadowMaterial = new CastShadowMaterial();


            // Init Octree
            octree = new Octree(new Vector3(-30, -30, -30), new Vector3(30, 30, 30));

            MaterialSettings wallMaterial = new MaterialSettings()
            {
                colorTexture = wallColorTexture,
                normalTexture = wallNormalTexture,
                emission = 1.0f,
                metalness = 0.0f,
                roughness = 1.0f
            };

            octree.AddEntity(new OctreeEntity(wallObject, gBufferReceiveShadowLayoutMaterial, wallMaterial, Matrix4.Identity));

            // generate random positioned & rotated cubes and add them to the octree
            Random random = new Random();
            for (int i = 0; i < 300; i++)
            {
                Matrix4 cubeTransform = Matrix4.CreateRotationX(random.Next(360) * (float)Math.PI / 180.0f);
                cubeTransform *= Matrix4.CreateRotationY(random.Next(360) * (float)Math.PI / 180.0f);
                cubeTransform *= Matrix4.CreateRotationZ(random.Next(360) * (float)Math.PI / 180.0f);
                cubeTransform *= Matrix4.CreateTranslation(random.Next(-300, 300) / 10.0f, random.Next(-300, 300) / 10.0f, 0);
                octree.AddEntity(new OctreeEntity(cubeObject, gBufferReceiveShadowLayoutMaterial, wallMaterial, cubeTransform));
            }

            displaySwitch = 0;

            DeferredRendering.Init(SCREEN_WIDTH, SCREEN_HEIGHT);

            // Initialize Deferred directional Light
            DeferredRendering.SetDirectionalLight(new Vector3(1, -1, 1), new Vector3(5, 5, 5), 0.1f);

            // set input events
            this.KeyDown += new EventHandler<KeyboardKeyEventArgs>(KeyDownEvent);
            this.KeyUp += new EventHandler<KeyboardKeyEventArgs>(KeyUpEvent);
            this.MouseMove += new EventHandler<MouseMoveEventArgs>(MouseMoveEvent);

        }

        private void KeyDownEvent(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F11)
            {
                if (WindowState != WindowState.Fullscreen)
                {
                    WindowState = WindowState.Fullscreen;
                }
                else
                {
                    WindowState = WindowState.Normal;
                }
            }

            if (e.Key == Key.Escape) this.Exit();
            if (e.Key == Key.Number1) displaySwitch = 0;
            if (e.Key == Key.Number2) displaySwitch = 1;
            if (e.Key == Key.Number3) displaySwitch = 2;
            if (e.Key == Key.Number4) displaySwitch = 3;
            if (e.Key == Key.Number5) displaySwitch = 4;
            if (e.Key == Key.Number6) displaySwitch = 5;
            if (e.Key == Key.Number7) displaySwitch = 6;


            if (e.Key == Key.Left) keyLeft = true;
            if (e.Key == Key.Right) keyRight = true;
            if (e.Key == Key.Up) keyUp = true;
            if (e.Key == Key.Down) keyDown = true;
            if (e.Key == Key.W) keyW = true;
            if (e.Key == Key.S) keyS = true;
            if (e.Key == Key.A) keyA = true;
            if (e.Key == Key.D) keyD = true;
        }

        private void KeyUpEvent(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Left) keyLeft = false;
            if (e.Key == Key.Right) keyRight = false;
            if (e.Key == Key.Up) keyUp = false;
            if (e.Key == Key.Down) keyDown = false;
            if (e.Key == Key.W) keyW = false;
            if (e.Key == Key.S) keyS = false;
            if (e.Key == Key.A) keyA = false;
            if (e.Key == Key.D) keyD = false;
        }

        private void MouseMoveEvent(object sender, MouseMoveEventArgs e)
        {
            currentMouseX = e.X;
            currentMouseY = e.Y;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // update the fly-cam with keyboard input
            if (oldMouseX > 0) Camera.UpdateMouseCamera(0.2f, keyA, keyD, keyW, keyS, (oldMouseY - currentMouseY) / 200.0f, (oldMouseX - currentMouseX) / 200.0f);

            oldMouseX = currentMouseX;
            oldMouseY = currentMouseY;

            // updateCounter simply increaes
            updateCounter++;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {

            // Set transform matrix for every dynamic object
            steampunkGunObject.Transformation = Matrix4.CreateRotationX(updateCounter / 60.0f);
            steampunkGunObject.Transformation *= Matrix4.CreateRotationY(updateCounter / 60.0f);
            steampunkGunObject.Transformation *= Matrix4.CreateRotationZ(updateCounter / 60.0f);
            steampunkGunObject.Transformation *= Matrix4.CreateTranslation(10, 0, 12);

            torusObject.Transformation = Matrix4.CreateRotationX(updateCounter / 60.0f);
            torusObject.Transformation *= Matrix4.CreateRotationY(updateCounter / 60.0f);
            torusObject.Transformation *= Matrix4.CreateRotationZ(updateCounter / 60.0f);
            torusObject.Transformation *= Matrix4.CreateTranslation(-10, 0, 9);

            // --- rendering all shadow casting objects
            CascadedShadowMapping.SetLightDirection(new Vector3(-1, 1, 1));

            GL.Disable(EnableCap.CullFace);
            CascadedShadowMapping.StartShadowMapping();
            for (int i = 0; i < 3; i++)
            {
                CascadedShadowMapping.SetDepthTextureTarget(i);

                castShadowMaterial.Draw(steampunkGunObject);
                castShadowMaterial.Draw(torusObject);
            }
            CascadedShadowMapping.EndShadowMapping();

            // --- rendering all objects into Gbuffer(s) ---
            DeferredRendering.StartGBufferRendering();

            octree.Draw();
            gBufferMaskMaterial.Draw(steampunkGunObject, gunAlbedoTexture, gunNormalTexture, gunMaskTexture);
            gBufferLayoutMaterial.Draw(torusObject, singleColorTexture, singleNormalTexture, 0.0f, 0.0f);


            // --- light pass ---
            if (displaySwitch == 0)
            {
                DeferredRendering.StartOffscreenBufferRendering();
                DeferredRendering.DrawFullscreenIBL(IBLSpecularSundown, IBLIrradianceSundown);

                DeferredRendering.CopyDepthToOffscreenBuffer();
                skyBox.Draw();

                // Split the resulting image to high and low range buffer, giving a threshold value
                DeferredRendering.StartSplitHighColorRendering();
                postProcessing_SplitColors.Draw(DeferredRendering.OffScreenBufferTexture, 0.75f);

                // blur the resulting high range buffer
                DeferredRendering.StartBlurPingPongBufferRendering(0);
                postProcessing_Blur.Draw(DeferredRendering.SplitHighColorBufferTexture, SCREEN_WIDTH, SCREEN_HEIGHT, new Vector2(1, 0));

                for (int i = 0; i < 20; i++)
                {
                    DeferredRendering.StartBlurPingPongBufferRendering(1);
                    postProcessing_Blur.Draw(DeferredRendering.BlurPingPongBufferTexture[0], SCREEN_WIDTH, SCREEN_HEIGHT, new Vector2(0, 1));
                    DeferredRendering.StartBlurPingPongBufferRendering(0);
                    postProcessing_Blur.Draw(DeferredRendering.BlurPingPongBufferTexture[1], SCREEN_WIDTH, SCREEN_HEIGHT, new Vector2(1, 0));
                }

                // combine high and low range buffer, also apply gamma correction and tone mapping
                DeferredRendering.StartOffscreenBufferRendering();
                postProcessing_CombinedColorCorrection.Draw(DeferredRendering.SplitNormalColorBufferTexture, DeferredRendering.BlurPingPongBufferTexture[1], 4.0f, 0.4f);

                DeferredRendering.StartScreenRendering();


                postProcessing_FXAA.Draw(DeferredRendering.OffScreenBufferTexture, SCREEN_WIDTH, SCREEN_HEIGHT);

                // finally draw 2d graphics
                logoSprite.Draw(-logoSprite.Width / 2, SCREEN_HEIGHT / 2 - logoSprite.Height);
                bitmapFont.DrawString("Hallo, dies ist ein Test.", -SCREEN_WIDTH / 2 + 50, -SCREEN_HEIGHT / 2 + 50, 255, 255, 255, 255);

            }
            else
            {
                DeferredRendering.DrawDebugFullscreen(displaySwitch - 1);
            }

            SwapBuffers();
        }



        protected override void OnUnload(EventArgs e)
        {

        }


        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            Camera.SetWidthHeightFov(Width, Height, 60);
        }


        [STAThread]
        public static void Main()
        {
            using (ExampleProject example = new ExampleProject())
            {
                example.Run(60.0, 60.0);
            }
        }
    }

}

