using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;

namespace Engine.Graphics.OpenGL
{
    internal class GLShader : GLGfxResource<ShaderDescriptor>
    {
        public GLShader() : base(glCreateProgram, glDeleteProgram, glUseProgram) { }

        protected override bool CreateResource(ShaderDescriptor descriptor)
        {
            if (descriptor.VertexSource == null || descriptor.FragmentSource == null)
            {
                Logger.Error("Shaders sources are invalid");
                return false;
            }

            uint vertId = CompileShader(GL_VERTEX_SHADER, descriptor.VertexSource);

            if(vertId == 0)
                return false;

            uint fragId = CompileShader(GL_FRAGMENT_SHADER, descriptor.FragmentSource);

            if (fragId == 0)
                return false;

            glAttachShader(Handle, vertId);
            glAttachShader(Handle, fragId);

            glLinkProgram(Handle);

            glDeleteShader(vertId);
            glDeleteShader(fragId);


            
            return ValidateProgram(Handle);
        }

        private unsafe uint CompileShader(int shaderType, byte[] shaderSource)
        {
            uint shaderId = glCreateShader(shaderType);
            string src = Encoding.UTF8.GetString(shaderSource);

            glShaderSource(shaderId, src);
            glCompileShader(shaderId);

            int result = GL_FALSE;

            glGetShaderiv(shaderId, GL_COMPILE_STATUS, &result);

            if (result == GL_FALSE)
            {
                int length = 0;
                glGetShaderiv(shaderId, GL_INFO_LOG_LENGTH, &length);
                var message = glGetShaderInfoLog(shaderId, length);

                Logger.Error($"failed to compile '{ShaderTypeName(shaderType)}' \n{message}");
                glDeleteShader(shaderId);

                return 0;
            }
            else
            {
                Logger.Success($"Shader compilation sucess '{ShaderTypeName(shaderType)}'");
            }

            return shaderId;
        }

        private unsafe bool ValidateProgram(uint program)
        {
            int linkStatus;
            glGetProgramiv(program, GL_LINK_STATUS, &linkStatus);
            if (linkStatus == GL_FALSE)
            {
                int length;
                glGetProgramiv(program, GL_INFO_LOG_LENGTH, &length);
                var log = glGetProgramInfoLog(program, length);
                Logger.Error($"Program linking failed: {log}");
                return false;
            }
#if DEBUG
            glValidateProgram(program);
#endif
            int status;
            glGetProgramiv(program, GL_VALIDATE_STATUS, &status);
            if (status == GL_FALSE)
            {
                int logLength;
                glGetProgramiv(program, GL_INFO_LOG_LENGTH, &logLength);

                var log = glGetProgramInfoLog(program, logLength);
                Logger.Error($"Program validation failed: {log}");

                return false;
            }

            return true;
        }

        private static string ShaderTypeName(int shaderType) => shaderType switch
        {
            GL_VERTEX_SHADER => "vertex",
            GL_FRAGMENT_SHADER => "fragment",
            _ => "unknown"
        };

        internal override void UpdateResource(ShaderDescriptor descriptor) { }

        // Remove this
        internal override void Bind()
        {
            base.Bind();

            int location = glGetUniformLocation(Handle, "uTexture");
            glUniform1i(location, 0);

        }
    }
}