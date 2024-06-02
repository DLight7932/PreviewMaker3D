using System;
using System.Collections.Generic;
using System.Text;
using SharpGL;
using System.Windows.Forms;
using SharpGL.SceneGraph.Assets;

namespace PreviewMaker3D
{
    public partial class Animated
    {
        public string name = "";
        public static int currentId = 0;
        public int id;
        public List<object> Properties = new List<object>();
        public List<Renderer> Renderers = new List<Renderer>();

        public PropertyBase GetProperty(string name_)
        {
            foreach (PropertyBase property in Properties)
                if (property.Name == name_)
                    return property;
            return null;
        }
        public void AddProperty(PropertyBase property) => Properties.Add(property);

        public void Render(OpenGL gl)
        {
            foreach (Renderer renderer in Renderers)
                renderer.Render(gl);
        }

        public Animated()
        {
            id = currentId;
            currentId++;
        }

        public void DisplayProperties(TabControl tabPage)
        {
            tabPage.TabPages[0].Controls.Clear();
            tabPage.TabPages[1].Controls.Clear();
            tabPage.TabPages[2].Controls.Clear();
            List<Control> controls0 = new List<Control>();
            List<Control> controls1 = new List<Control>();
            List<Control> controls2 = new List<Control>();

            MainForm.mainForm.Modifiers.Clear();
            MainForm.mainForm.Renderers.Clear();

            foreach (PropertyBase property in Properties)
            {
                PropertyModifier modifier = new PropertyModifier(property);

                if (property is Property<VectorFloat3D> vectorFloat3D)
                    modifier = new VectorFloat3DPropertyModifier(vectorFloat3D);
                else if (property is Property<VectorFloat2D> vectorFloat2D)
                    modifier = new VectorFloat2DPropertyModifier(vectorFloat2D);
                else if (property is Property<Pixel32bppRGBA> color)
                    modifier = new Pixel32bppRGBAPropertyModifier(color);
                else if (property is Property<bool> boolean)
                    modifier = new BoolPropertyModifier(boolean);
                else if (property is Property<int> integer)
                    modifier = new IntPropertyModifier(integer);
                else if (property is Property<float> floating)
                    modifier = new FloatPropertyModifier(floating);
                else if (property is Property<MyTexture> texture)
                    modifier = new TexturePropertyModifier(texture);
                else if (property is Property<VectorInt2D> vectorInt2d)
                    modifier = new VectorInt2DPropertyModifier(vectorInt2d);
                else if (property is ListOf<VectorFloat3D> vertexes)
                    modifier = new ListOfVF3PropertyModifier(vertexes);
                else if (property is ListOf<Line> lines)
                    modifier = new ListOfLinePropertyModifier(lines);
                else if (property is ListOf<Poly> polys)
                    modifier = new ListOfPolyPropertyModifier(polys);

                modifier.MouseUp += (object sender, MouseEventArgs e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        tabPage.TabPages[0].Controls.Remove(modifier);
                        tabPage.TabPages[1].Controls.Remove(modifier);
                        Properties.Remove(property);
                    }
                };

                modifier.Dock = DockStyle.Top;

                if (property is IAnimatedProperty)
                    controls0.Add(modifier);
                else
                    controls1.Add(modifier);
                MainForm.mainForm.Modifiers.Add(modifier);
            }

            for (int i = controls0.Count - 1; i >= 0; i--)
                tabPage.TabPages[0].Controls.Add(controls0[i]);
            for (int i = controls1.Count - 1; i >= 0; i--)
                tabPage.TabPages[1].Controls.Add(controls1[i]);

            foreach (Renderer renderer in Renderers)
            {
                Button modifier = new Button();

                modifier.Text = renderer.name;
                modifier.Dock = DockStyle.Top;
                modifier.Height = 30;

                modifier.MouseUp += (object sender, MouseEventArgs e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        tabPage.TabPages[2].Controls.Remove(modifier);
                        Renderers.Remove(renderer);
                    }
                };

                controls2.Add(modifier);
                MainForm.mainForm.Renderers.Add(modifier);
            }

            for (int i = controls2.Count - 1; i >= 0; i--)
                tabPage.TabPages[2].Controls.Add(controls2[i]);
        }

        public override string ToString()
        {
            return $"{name} [{id}]\n";
        }
    }
}
