using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewMaker3D
{
    public struct Poly
    {
        public int p1;
        public int p2;
        public int p3;

        public Poly(int p1_, int p2_, int p3_)
        {
            p1 = p1_;
            p2 = p2_;
            p3 = p3_;
        }

        public static Poly Parse(string s)
        {
            char[] charsToTrim = { '[', ']' };
            string[] S = s.Trim(charsToTrim).Split(',');
            int p1 = int.Parse(S[0]);
            int p2 = int.Parse(S[1]);
            int p3 = int.Parse(S[2]);
            return new Poly(p1, p2, p3);
        }
    }
}
