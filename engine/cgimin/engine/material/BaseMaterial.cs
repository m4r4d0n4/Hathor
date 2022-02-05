using System;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using System.IO;

namespace cgimin.engine.material
{
    public abstract class BaseMaterial
    {

        // struct contains all possible options for each material 
        public struct MaterialSettings
        {
            public int colorTexture;
            public int normalTexture;
            public int maskTexture;

            public float roughness;
            public float metalness;

            public float emission;

            // values for blending
            public BlendingFactor SrcBlendFactor;
            public BlendingFactor DestBlendFactor;
        }

        private int VertexObject;
        private int FragmentObject;

        public int Program;

        public bool isTransparent = false;

        public void CreateShaderProgram(string pathVS, string pathFS)
        {

            // shader files are read (text)
            string vs = File.ReadAllText(pathVS);
            string fs = File.ReadAllText(pathFS);

            int status_code;
            string info;

            // vertex and fragment shaders are created
            VertexObject = GL.CreateShader(ShaderType.VertexShader);
            FragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            // compiling vertex-shader 
            GL.ShaderSource(VertexObject, vs);
            GL.CompileShader(VertexObject);
            GL.GetShaderInfoLog(VertexObject, out info);
            GL.GetShader(VertexObject, ShaderParameter.CompileStatus, out status_code);

            if (status_code != 1)
                throw new ApplicationException(info);

            // compiling fragment shader
            GL.ShaderSource(FragmentObject, fs);
            GL.CompileShader(FragmentObject);
            GL.GetShaderInfoLog(FragmentObject, out info);
            GL.GetShader(FragmentObject, ShaderParameter.CompileStatus, out status_code);

            if (status_code != 1)
                throw new ApplicationException(info);

            // final shader program is created using rhw fragment and the vertex program
            Program = GL.CreateProgram();
            GL.AttachShader(Program, FragmentObject);
            GL.AttachShader(Program, VertexObject);

            // hint: "Program" is not linked yet
        }

        // abstract, to force each material to implement
        public abstract void DrawWithSettings(BaseObject3D object3d, MaterialSettings settings);



    }
}
