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
        private readonly GLIndexBuffer _indexBuffer;

        public GLGeometry() : base(glGenVertexArray, glDeleteVertexArray, glBindVertexArray)
        {
            _vertBuffer = new GLVertexBuffer();
            _indexBuffer = new GLIndexBuffer();
        }

        protected override bool CreateResource(GeometryDescriptor descriptor)
        {
            Bind();
            if (!_vertBuffer.Create(descriptor.VertexDesc.BufferDesc)) 
            {
                Logger.Error("Failed to create vertex buffer.");
                Unbind();
                return false;
            }

            if (!_indexBuffer.Create(descriptor.IndexBuffer)) 
            {
                Logger.Error("Failed to create index buffer.");
                Unbind();
                return false;
            }
            
            _vertBuffer.Bind();
            _indexBuffer.Bind();

            for (uint i = 0; i < descriptor.VertexDesc.Attribs.Count; i++)
            {
                var attrib = descriptor.VertexDesc.Attribs[(int)i];

                glEnableVertexAttribArray(i);
                glVertexAttribPointer(i, attrib.Count, attrib.Type.ToGL(), attrib.Normalized, attrib.Stride, attrib.Offset);
            }

            Unbind();

            return true;
        }

        internal override void UpdateResource(GeometryDescriptor descriptor)
        {
            _vertBuffer.Update(descriptor.VertexDesc.BufferDesc);
            _indexBuffer.Update(descriptor.IndexBuffer);
        }

        protected override void FreeResource()
        {
            _vertBuffer.Dispose();
            _indexBuffer.Dispose();

            base.FreeResource();
        }
    }
}