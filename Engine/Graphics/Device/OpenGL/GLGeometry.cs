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
        private GLIndexBuffer _indexBuffer;

        public GLGeometry() : base(glGenVertexArray, glDeleteVertexArray, glBindVertexArray)
        {
            _vertBuffer = new GLVertexBuffer();
        }

        protected override bool CreateResource(GeometryDescriptor descriptor)
        {
            Bind();
            if (!_vertBuffer.Create(descriptor.VertexDesc.BufferDesc))
            {
                Log.Error("Failed to create vertex buffer.");
                Unbind();
                return false;
            }

            if (descriptor.SharedIndexBuffer == null)
            {
                if (descriptor.IndexDesc == null)
                {
                    Log.Error("Index descriptor is null, can't create geometry.");
                    return false;
                }

                _indexBuffer = new GLIndexBuffer();

                if (!_indexBuffer.Create(descriptor.IndexDesc))
                {
                    Log.Error("Failed to create index buffer.");
                    Unbind();
                    return false;
                }

                _indexBuffer.Bind();
            }
            else
            {
                (descriptor.SharedIndexBuffer as GLIndexBuffer).Bind();
            }

            _vertBuffer.Bind();

            for (uint i = 0; i < descriptor.VertexDesc.Attribs.Count; i++)
            {
                var attrib = descriptor.VertexDesc.Attribs[(int)i];

                if(attrib.Type == GfxValueType.Int || attrib.Type == GfxValueType.Uint)
                {
                    glVertexAttribIPointer(i, attrib.Count, attrib.Type.ToGL(), attrib.Stride, attrib.Offset);
                }
                else
                {
                    glVertexAttribPointer(i, attrib.Count, attrib.Type.ToGL(), attrib.Normalized, attrib.Stride, attrib.Offset);
                }

                glEnableVertexAttribArray(i);
            }

            Unbind();

            return true;
        }

        internal override void UpdateResource(GeometryDescriptor descriptor)
        {
            _vertBuffer.Update(descriptor.VertexDesc.BufferDesc);

            if (_indexBuffer != null)
            {
                _indexBuffer.Update(descriptor.IndexDesc);
            }
        }

        protected override void FreeResource()
        {
            _vertBuffer.Dispose();

            if (_indexBuffer != null)
            {
                _indexBuffer.Dispose();
            }
            base.FreeResource();
        }
    }
}