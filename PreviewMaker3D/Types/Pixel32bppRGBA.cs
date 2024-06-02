using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PreviewMaker3D
{
    public struct Pixel32bppRGBA
    {
        public byte b;
        public byte g;
        public byte r;
        readonly byte a;
        public float FloatR => 0.00390625f * r;// 1.0f * r / 255.0f;
        public float FloatG => 0.00390625f * g;// 1.0f * g / 255.0f;
        public float FloatB => 0.00390625f * b;// 1.0f * b / 255.0f;

        public static Pixel32bppRGBA Interpolate(Pixel32bppRGBA p1, Pixel32bppRGBA p2, int unit, int aspect) =>
            new Pixel32bppRGBA(
                (byte)(p1.r + (p2.r - p1.r) * aspect / unit),
                (byte)(p1.g + (p2.g - p1.g) * aspect / unit),
                (byte)(p1.b + (p2.b - p1.b) * aspect / unit));

        public Pixel32bppRGBA(byte r_, byte g_, byte b_)
        {
            r = r_;
            g = g_;
            b = b_;
            a = 255;
        }

        public static Pixel32bppRGBA operator -(Pixel32bppRGBA p)
        {
            return new Pixel32bppRGBA((byte)(255 - p.r), (byte)(255 - p.g), (byte)(255 - p.b));
        }
        public static Pixel32bppRGBA operator +(Pixel32bppRGBA p1, Pixel32bppRGBA p2)
        {
            return new Pixel32bppRGBA((byte)Math.Min(p1.r + p2.r, 255), (byte)Math.Min(p1.g + p2.g, 255), (byte)Math.Min(p1.b + p2.b, 255));
        }
        public static Pixel32bppRGBA operator -(Pixel32bppRGBA p1, Pixel32bppRGBA p2)
        {
            return new Pixel32bppRGBA((byte)Math.Max(p1.r + p2.r, 0), (byte)Math.Max(p1.g + p2.g, 0), (byte)Math.Max(p1.b + p2.b, 0));
        }
        public static Pixel32bppRGBA operator /(Pixel32bppRGBA p, int i)
        {
            return new Pixel32bppRGBA((byte)(p.r / i), (byte)(p.g / i), (byte)(p.b / i));
        }
        public static Pixel32bppRGBA operator *(Pixel32bppRGBA p, int i)
        {
            return new Pixel32bppRGBA((byte)(p.r * i), (byte)(p.g * i), (byte)(p.b * i));
        }

        public override string ToString() => $"{r.ToString("X").PadLeft(2, '0')}{g.ToString("X").PadLeft(2, '0')}{b.ToString("X").PadLeft(2, '0')}";

        public bool TryParse(string s)
        {
            Pixel32bppRGBA result = default;

            if (s.Length != 6)
                return false;

            string R = s[0] + "" + s[1];
            if (!byte.TryParse(R, System.Globalization.NumberStyles.HexNumber, null, out result.r))
                return false;

            string G = s[2] + "" + s[3];
            if (!byte.TryParse(G, System.Globalization.NumberStyles.HexNumber, null, out result.g))
                return false;

            string B = s[4] + "" + s[5];
            if (!byte.TryParse(B, System.Globalization.NumberStyles.HexNumber, null, out result.b))
                return false;

            this = result;
            return true;
        }

        public static Pixel32bppRGBA Parse(string s)
        {
            Pixel32bppRGBA result = default;

            if (s.Length != 6)
                throw new Exception();

            string R = s[0] + "" + s[1];
            if (!byte.TryParse(R, System.Globalization.NumberStyles.HexNumber, null, out result.r))
                throw new Exception();

            string G = s[2] + "" + s[3];
            if (!byte.TryParse(G, System.Globalization.NumberStyles.HexNumber, null, out result.g))
                throw new Exception();

            string B = s[4] + "" + s[5];
            if (!byte.TryParse(B, System.Globalization.NumberStyles.HexNumber, null, out result.b))
                throw new Exception();

            return result;
        }
    }
}
