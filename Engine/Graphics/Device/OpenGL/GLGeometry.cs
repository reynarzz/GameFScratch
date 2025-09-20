using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;

namespace Engine.Graphics.OpenGL
{
    internal class GLGeometry : GLGfxResource<GeometryDescriptor>
    {
        private readonly GLVertexBuffer _vertBuffer;
        private readonly GLIndexBuffer _vertIndexBuffer;

        public GLGeometry() : base(glBindVertexArray, glGenVertexArray, glDeleteVertexArray)
        {
            _vertBuffer = new GLVertexBuffer();
            _vertIndexBuffer = new GLIndexBuffer();
        }

        protected override bool CreateResource(GeometryDescriptor descriptor)
        {
            Bind();
            if (!_vertBuffer.Create(descriptor.VertexDesc.BufferDesc)) 
            {
                Logger.Error("Failed to create vertex buffer.");
                UnBind();
                return false;
            }

            if (!_vertIndexBuffer.Create(descriptor.IndexBuffer)) 
            {
                Logger.Error("Failed to create index buffer.");
                UnBind();
                return false;
            }

            for (uint i = 0; i < descriptor.VertexDesc.Attribs.Count; i++)
            {
                var attrib = descriptor.VertexDesc.Attribs[(int)i];

                glEnableVertexAttribArray(i);
                glVertexAttribPointer(i, attrib.Count, attrib.Type.ToGL(), attrib.Normalized, attrib.Stride, attrib.Offset); // positions
            }

            UnBind();

            return true;
        }

        public override void Bind()
        {
            base.Bind();
            _vertIndexBuffer.Bind();

        }
        public override void UpdateResource(GeometryDescriptor descriptor)
        {
            _vertBuffer.Update(descriptor.VertexDesc.BufferDesc);
            _vertIndexBuffer.Update(descriptor.IndexBuffer);
        }

        protected override void FreeResource()
        {
            _vertBuffer.Dispose();
            _vertIndexBuffer.Dispose();

            base.FreeResource();

        }
    }
}