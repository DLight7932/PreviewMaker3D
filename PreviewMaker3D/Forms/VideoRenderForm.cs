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
    public partial class VideoRenderForm : Form
    {
        public VideoRenderForm()
        {
            InitializeComponent();
        }

        public IntModifier IntModifier1 => intModifier1;
        public IntModifier IntModifier2 => intModifier2;
        public IntModifier IntModifier3 => intModifier3;
        public Button Button => button1;
    }
}
