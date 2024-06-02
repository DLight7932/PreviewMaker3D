using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PreviewMaker3D
{
    public partial class PropertySelecter : Form
    {
        public static object SelectedProperty;
        PropertyCreator.TypesEnum type;

        public PropertySelecter(PropertyCreator.TypesEnum type_)
        {
            InitializeComponent();

            type = type_;

            comboBox1.SelectedIndexChanged -= comboBox1_SelectedIndexChanged;
            comboBox1.DataSource = MainForm.mainForm.scene;
            comboBox1.SelectedItem = -1;
            comboBox1.SelectedItem = null;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            comboBox2.Format += (sender, e) =>
            {
                if (e.ListItem is PropertyBase item)
                    e.Value = item.Name;
            };
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = true;
            comboBox2.SelectedIndexChanged -= comboBox2_SelectedIndexChanged;
            foreach (PropertyBase propertyBase in (comboBox1.SelectedItem as Animated).Properties)
                if (type == PropertyCreator.TypesEnum.VectorFloat3D && propertyBase is Property<VectorFloat3D>) 
                    comboBox2.Items.Add(propertyBase);
                else if (type == PropertyCreator.TypesEnum.VectorFloat2D && propertyBase is Property<VectorFloat2D>)
                    comboBox2.Items.Add(propertyBase);
                else if (type == PropertyCreator.TypesEnum.VectorInt2D && propertyBase is Property<VectorInt2D>)
                    comboBox2.Items.Add(propertyBase);
                else if (type == PropertyCreator.TypesEnum.Pixel32bppRGBA && propertyBase is Property<Pixel32bppRGBA>)
                    comboBox2.Items.Add(propertyBase);
                else if (type == PropertyCreator.TypesEnum.Int && propertyBase is Property<int>)
                    comboBox2.Items.Add(propertyBase);
                else if (type == PropertyCreator.TypesEnum.Float && propertyBase is Property<float>)
                    comboBox2.Items.Add(propertyBase);
                else if (type == PropertyCreator.TypesEnum.Bool && propertyBase is Property<bool>)
                    comboBox2.Items.Add(propertyBase);
                else if (type == PropertyCreator.TypesEnum.Texture && propertyBase is Property<MyTexture>)
                    comboBox2.Items.Add(propertyBase);
            comboBox2.SelectedItem = -1;
            comboBox2.SelectedItem = null;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedProperty = comboBox2.SelectedItem;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
