using System;
using System.Collections.Generic;
using System.Text;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;

namespace PreviewMaker3D
{
    static class Textures
    {
        public static List<MyTexture> textures = new List<MyTexture>();
        static int ID = 1;
        public static MyTexture None = new MyTexture() { id = 0, name = "Default", bitmap = new Bitmap(16, 16) };

        static Textures()
        {
            textures.Add(None);
        }

        public static void AddTexture(OpenGL GL, string filePath)
        {
            new Texture().Create(GL, filePath);
            GL.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            GL.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            textures.Add(new MyTexture() { bitmap = new Bitmap(filePath), id = ID, name = Path.GetFileNameWithoutExtension(filePath) });
            ID++;
        }
    }

    public class MyTexture
    {
        [JsonIgnore]
        public Bitmap bitmap;
        public int id;
        public string name;

        public override string ToString()
        {
            return name;
        }
    }
}
