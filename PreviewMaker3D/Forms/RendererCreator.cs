using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PreviewMaker3D
{
    public partial class RendererCreator : Form
    {
        public enum RenderersEnum
        {
            Cube,
            Cilinder,
            Cone,
            Sphere,
            Triangle,
            Point,
            Line,
            Poly
        }

        public static Renderer result;

        public RendererCreator()
        {
            InitializeComponent();
            comboBox1.DataSource = Enum.GetValues(typeof(RenderersEnum));
            comboBox1.SelectedIndex = -1;
            comboBox1.SelectedIndexChanged += (object sender, EventArgs e) =>
                {
                    switch ((RenderersEnum)comboBox1.SelectedItem)
                    {
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                };
        }
    }
}
