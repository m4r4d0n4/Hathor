using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgimin.engine.fbo
{
    public class Fbo 
    {
        //We are using this for the water reflections
        //Constants
        protected const int REFLECTION_WIDTH = 1280;
        private const int REFLECTION_HEIGHT = 720;
        private const int REFRACTION_WIDTH = 1280;
        private const int REFRACTION_HEIGHT = 720;

        public int reflectionFrameBuffer { get; set; }
        public int reflectionTexture { get; set; }
        public int reflectionDepthBuffer { get; set; }

        public int refractionFrameBuffer { get; set; }
        public int refractionTexture { get; set; }
        public int refractionDepthTexture { get; set; }
        public Fbo()
        {
            initialiseReflectionFrameBuffer();

            initialiseRefractionFrameBuffer();
        }
        //Creates the FBO
        private int createFrameBuffer()
        {
            int frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            return frameBuffer;
        }
        private int createTextureAttachment(int width, int height)
        {
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr)null);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, texture, 0);
            return texture;
        }

        private int createDepthTextureAttachment(int width, int height)
        {
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)null);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, texture, 0);
            return texture;
        }
        private void bindFrameBuffer(int frameBuffer, int width, int height)
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            GL.Viewport(0, 0, width, height);
        }

        public void unbindCurrentFrameBuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, DisplayDevice.Default.Width, DisplayDevice.Default.Height);
        }
        private void initialiseRefractionFrameBuffer()
        {
            refractionFrameBuffer = createFrameBuffer();
            refractionTexture = createTextureAttachment(REFRACTION_WIDTH, REFRACTION_HEIGHT);
            refractionDepthTexture = createDepthTextureAttachment(REFRACTION_WIDTH, REFRACTION_HEIGHT);
            unbindCurrentFrameBuffer();
        }
        private void initialiseReflectionFrameBuffer()
        {
            reflectionFrameBuffer = createFrameBuffer();
            reflectionTexture = createTextureAttachment(REFLECTION_WIDTH, REFLECTION_HEIGHT);
            reflectionDepthBuffer = createDepthTextureAttachment(REFLECTION_WIDTH, REFLECTION_HEIGHT);
            unbindCurrentFrameBuffer();
        }
        public void bindRefractionFrameBuffer()
        {
            bindFrameBuffer(refractionFrameBuffer, REFRACTION_WIDTH, REFRACTION_WIDTH);
        }
        public void bindReflectionFrameBuffer()
        {
            bindFrameBuffer(reflectionFrameBuffer, REFLECTION_WIDTH, REFLECTION_WIDTH);
        }
    }
}
