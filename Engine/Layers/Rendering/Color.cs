using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public struct Color
    {
        public float R,G,B,A;
        public static Color Red => new Color(1, 0, 0, 1);
        public static Color Green => new Color(0, 1, 0, 1);
        public static Color Blue => new Color(0, 0, 1, 1);
        public static Color White => new Color(1, 1, 1, 1);
        public static Color Black => new Color(0, 0, 0, 1);

        public Color(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static implicit operator ColorPacketRGBA(Color c)
        {
            return (ColorPacketRGBA)(Color32)c;
        }

        public static implicit operator uint(Color c)
        {
            return (ColorPacketRGBA)c;
        }

        public static implicit operator Color(uint packet)
        {
            return new ColorPacketRGBA(packet);
        }
    }

    public struct Color32
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color32(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static implicit operator Color(Color32 c32)
        {
            return new Color(
                c32.R / 255.0f,
                c32.G / 255.0f,
                c32.B / 255.0f,
                c32.A / 255.0f);
        }

        public static implicit operator ColorPacketRGBA(Color32 c32)
        {
            uint packed =
                ((uint)c32.R << 24) |
                ((uint)c32.G << 16) |
                ((uint)c32.B << 8) |
                c32.A;
            return new ColorPacketRGBA(packed);
        }

        public static implicit operator Color32(ColorPacketRGBA packet)
        {
            uint v = packet.Value;
            byte r = (byte)(v >> 24);
            byte g = (byte)(v >> 16);
            byte b = (byte)(v >> 8);
            byte a = (byte)v;
            return new Color32(r, g, b, a);
        }

        public static implicit operator Color32(Color color)
        {
            return new Color32((byte)(color.R * 255.0f), (byte)(color.G * 255.0f), (byte)(color.B * 255.0f), (byte)(color.A * 255.0f));
        }
    }

    public struct ColorPacketRGBA
    {
        public uint Value;

        public ColorPacketRGBA(uint value)
        {
            Value = value;
        }

        public static implicit operator Color(ColorPacketRGBA packet)
        {
            return (Color)(Color32)packet;
        }

        public static implicit operator uint(ColorPacketRGBA packet)
        {
            return packet.Value;
        }

        public static implicit operator ColorPacketRGBA(uint packet)
        {
            return new ColorPacketRGBA(packet);
        }
    }
}
