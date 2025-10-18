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
        public static Color Transparent => new Color(0, 0, 0, 0);
        public static Color Red => new Color(1, 0, 0, 1);
        public static Color Green => new Color(0, 1, 0, 1);
        public static Color Blue => new Color(0, 0, 1, 1);
        public static Color White => new Color(1, 1, 1, 1);
        public static Color Black => new Color(0, 0, 0, 1);
        public static Color Yellow => new Color(1f, 1f, 0f, 1f);
        public static Color Cyan => new Color(0f, 1f, 1f, 1f);
        public static Color Magenta => new Color(1f, 0f, 1f, 1f);
        public static Color Gray => new Color(0.5f, 0.5f, 0.5f, 1f);
        public static Color LightGray => new Color(0.83f, 0.83f, 0.83f, 1f);
        public static Color DarkGray => new Color(0.33f, 0.33f, 0.33f, 1f);
        public static Color Orange => new Color(1f, 0.65f, 0f, 1f);
        public static Color Purple => new Color(0.5f, 0f, 0.5f, 1f);
        public static Color Brown => new Color(0.6f, 0.3f, 0f, 1f);
        public static Color Pink => new Color(1f, 0.75f, 0.8f, 1f);
        public static Color LightBlue => new Color(0.68f, 0.85f, 0.9f, 1f);
        public static Color DarkBlue => new Color(0f, 0f, 0.55f, 1f);
        public static Color LightGreen => new Color(0.56f, 0.93f, 0.56f, 1f);
        public static Color DarkGreen => new Color(0f, 0.39f, 0f, 1f);
        public static Color Teal => new Color(0f, 0.5f, 0.5f, 1f);
        public static Color Navy => new Color(0f, 0f, 0.5f, 1f);
        public static Color Gold => new Color(1f, 0.84f, 0f, 1f);
        public static Color Silver => new Color(0.75f, 0.75f, 0.75f, 1f);
        public static Color Olive => new Color(0.5f, 0.5f, 0f, 1f);
        public static Color Maroon => new Color(0.5f, 0f, 0f, 1f);
        public static Color Violet => new Color(0.93f, 0.51f, 0.93f, 1f);
        public static Color Coral => new Color(1f, 0.5f, 0.31f, 1f);
        public static Color Salmon => new Color(0.98f, 0.5f, 0.45f, 1f);
        public static Color Indigo => new Color(0.29f, 0f, 0.51f, 1f);

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

        public static Color Lerp(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Color(
                a.R + (b.R - a.R) * t,
                a.G + (b.G - a.G) * t,
                a.B + (b.B - a.B) * t,
                a.A + (b.A - a.A) * t
            );
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
