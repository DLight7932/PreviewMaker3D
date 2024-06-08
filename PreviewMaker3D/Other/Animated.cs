using System;
using System.Collections.Generic;
using System.Text;
using SharpGL;
using System.Windows.Forms;
using SharpGL.SceneGraph.Assets;

namespace PreviewMaker3D
{
    public class Animated
    {
        public string name = "";
        public List<Property> Properties = new List<Property>();
        public List<Renderer> Renderers = new List<Renderer>();
        public List<Controler> Controlers = new List<Controler>();

        public Property GetProperty(string name_)
        {
            foreach (Property property in Properties)
                if (property.Name == name_)
                    return property;
            return null;
        }
        public void AddProperty(Property property) => Properties.Add(property);

        public void Render(OpenGL gl)
        {
            foreach (Renderer renderer in Renderers)
                renderer.Render(gl);
        }

        public void RenderControlers(OpenGL gl, int mouseX, int mouseY)
        {
            foreach (Controler controler in Controlers)
                controler.Render(gl, mouseX, mouseY);
        }

        //public override string ToString()
        //{
        //    return $"{name} [{id}]\n";
        //}
    }
}
