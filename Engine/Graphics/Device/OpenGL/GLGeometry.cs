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
        private GLIndexBuffer _sharedBufferTest;

        public GLGeometry() : base(glGenVertexArray, glDeleteVertexArray, glBindVertexArray)
        {
            _vertBuffer = new GLVertexBuffer();
        }

        protected override bool CreateResource(GeometryDescriptor descriptor)
        {
            Bind();
            if (!_vertBuffer.Create(descriptor.VertexDesc.BufferDesc))
            {
                Debug.Error("Failed to create vertex buffer.");
                Unbind();
                return false;
            }

            if (descriptor.SharedIndexBuffer == null && descriptor.IndexDesc != null)
            {
                _indexBuffer = new GLIndexBuffer();

                if (!_indexBuffer.Create(descriptor.IndexDesc))
                {
                    Debug.Error("Failed to create index buffer.");
                    Unbind();
                    return false;
                }

                _indexBuffer.Bind();
            }
            else if(descriptor.SharedIndexBuffer != null)
            {
                (descriptor.SharedIndexBuffer as GLIndexBuffer).Bind();

                _sharedBufferTest = descriptor.SharedIndexBuffer as GLIndexBuffer;
            }

            _vertBuffer.Bind();

            for (uint i = 0; i < descriptor.VertexDesc.Attribs.Length; i++)
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
            Bind();
            _vertBuffer.Update(descriptor.VertexDesc.BufferDesc);
            _vertBuffer.Bind();
            if(_sharedBufferTest != null)
            {
                _sharedBufferTest.Bind();
            }
            if (descriptor.SharedIndexBuffer != null && descriptor.SharedIndexBuffer != _sharedBufferTest)
            {
                Debug.Error("Shared index buffer error");
                throw new Exception("Different shared index buffer, please handle it");
            }
            // descriptor.SharedIndexBuffer;

            if (_indexBuffer != null)
            {
                _indexBuffer.Update(descriptor.IndexDesc);
                _indexBuffer.Bind();
            }

            Unbind();
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