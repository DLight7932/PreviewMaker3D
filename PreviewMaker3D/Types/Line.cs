using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewMaker3D
{
    public struct Line
    {
        public int p1;
        public int p2;

        public Line(int p1_, int p2_)
        {
            p1 = p1_;
            p2 = p2_;
        }

        public static Line Parse(string s)
        {
            char[] charsToTrim = { '[', ']' };
            string[] S = s.Trim(charsToTrim).Split(',');
            int p1 = int.Parse(S[0]);
            int p2 = int.Parse(S[1]);
            return new Line(p1, p2);
        }
    }
}
