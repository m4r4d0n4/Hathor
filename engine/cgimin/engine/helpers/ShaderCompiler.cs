using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace cgimin.engine.helpers
{
    public class ShaderCompiler
    {
        public static int CreateShaderProgram(string pathVS, string pathFS)
        {
            // shader files are read (text)
            string vs = File.ReadAllText(pathVS);
            string fs = File.ReadAllText(pathFS);

            int status_code;
            string info;

            // vertex and fragment shaders are created
            int vertexObject = GL.CreateShader(ShaderType.VertexShader);
            int fragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            // compiling vertex-shader 
            GL.ShaderSource(vertexObject, vs);
            GL.CompileShader(vertexObject);
            GL.GetShaderInfoLog(vertexObject, out info);
            GL.GetShader(vertexObject, ShaderParameter.CompileStatus, out status_code);

            if (status_code != 1)
                throw new ApplicationException(info);

            // compiling fragment shader
            GL.ShaderSource(fragmentObject, fs);
            GL.CompileShader(fragmentObject);
            GL.GetShaderInfoLog(fragmentObject, out info);
            GL.GetShader(fragmentObject, ShaderParameter.CompileStatus, out status_code);

            if (status_code != 1)
                throw new ApplicationException(info);

            // final shader program is created using rhw fragment and the vertex program
            int retProgram = GL.CreateProgram();
            GL.AttachShader(retProgram, fragmentObject);
            GL.AttachShader(retProgram, vertexObject);

            return retProgram;
        }

    }
}
