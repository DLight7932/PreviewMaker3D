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
    public partial class TimeLine : UserControl
    {
        TreeNode selectedNode;
        public TreeNode selectedObject;

        bool mouseDown;
        bool movingKey = false;
        ITime SelectedKey;

        int shift = 0;

        int MouseTime(int MouseX) => (MouseX + treeView.ItemHeight / 2) / treeView.ItemHeight;

        public TimeLine()
        {
            InitializeComponent();

            MouseWheel += (object sender, MouseEventArgs e) =>
            {
                if (e.Delta > 0 && PropertyBase.Time > 0)
                {
                    if (PropertyBase.Time - shift > pictureBox.Width / treeView.ItemHeight * 0.25f || shift == 0)
                        PropertyBase.Time--;
                    else
                        shift--;
                }
                else if (e.Delta < 0)
                {
                    if (PropertyBase.Time - shift < pictureBox.Width / treeView.ItemHeight * 0.75f)
                        PropertyBase.Time++;
                    else
                        shift++;
                }
                MainForm.RefreshModifiers();
            };
        }

        public TreeView TreeView => treeView;
        public PictureBox PictureBox => pictureBox;

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectObject(e.Node);

            if (e.Node.Tag is Animated)
                selectedObject = e.Node;
            selectedNode = e.Node;
            e.Node.Expand();
        }

        public void SelectObject(TreeNode newNode)
        {
            if (newNode.Tag is Animated animated)
            {
                if (selectedObject != null)
                {
                    selectedObject.Nodes.Clear();
                    if (selectedNode.Nodes.Count == 0)
                        selectedNode.Collapse();
                }
                foreach (PropertyBase property in animated.Properties)
                    if (property is IAnimatedProperty)
                        newNode.Nodes.Add(new TreeNode(property.Name) { Tag = property });
            }
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
            {
                if (selectedNode.Tag is Animated animated)
                {
                    foreach (PropertyBase property in animated.Properties)
                        if (property is IAnimatedProperty animatedProperty)
                            animatedProperty.AddKey(PropertyBase.Time);
                }
                else if (selectedNode.Tag is IAnimatedProperty animatedProperty)
                {
                    animatedProperty.AddKey(PropertyBase.Time);
                }
            }
            else if (e.KeyCode == Keys.Back)
            {
                MainForm.mainForm.scene.Remove(selectedNode.Tag as Animated);
                treeView.Nodes.Remove(selectedNode);
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (selectedNode.Tag is Animated animated)
                {
                    foreach (PropertyBase property in animated.Properties)
                        if (property is IAnimatedProperty animatedProperty)
                            animatedProperty.RemoveKey(PropertyBase.Time);
                }
                else if (selectedNode.Tag is IAnimatedProperty animatedProperty)
                {
                    animatedProperty.RemoveKey(PropertyBase.Time);
                }
            }
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            Pen pen = Pens.Blue;
            Brush brush = new SolidBrush(pen.Color);

            int y = treeView.ItemHeight;

            Tree(treeView.Nodes);

            void Tree(TreeNodeCollection nodes)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].IsVisible)
                    {
                        g.DrawLine(pen, 0, y, pictureBox.Width, y);

                        if (nodes[i] == selectedNode)
                            g.DrawLine(new Pen(Color.White), 0, y - treeView.ItemHeight / 2, pictureBox.Width, y - treeView.ItemHeight / 2);

                        if (nodes[i].Tag is Animated)
                        {
                            foreach (PropertyBase property in (nodes[i].Tag as Animated).Properties)
                            {
                                if (property is IAnimatedProperty animatedProperty)
                                {
                                    foreach (ITime Key in animatedProperty.Keys)
                                        g.FillPolygon(Key.time == PropertyBase.Time ? new SolidBrush(Color.White) : new SolidBrush(Color.Blue), new PointF[] {
                                new Point((Key.time - shift) * treeView.ItemHeight - treeView.ItemHeight / 2, y - treeView.ItemHeight / 2),
                                new Point((Key.time - shift) * treeView.ItemHeight, y - treeView.ItemHeight),
                                new Point((Key.time - shift) * treeView.ItemHeight + treeView.ItemHeight / 2, y - treeView.ItemHeight / 2),
                                new Point((Key.time - shift) * treeView.ItemHeight, y)});
                                }
                            }
                        }
                        else if (nodes[i].Tag is IAnimatedProperty animatedProperty)
                        {
                            foreach (ITime Key in animatedProperty.Keys)
                                g.FillPolygon(Key == SelectedKey ? new SolidBrush(Color.White) : new SolidBrush(Color.Green), new PointF[] {
                                new Point((Key.time - shift) * treeView.ItemHeight - treeView.ItemHeight / 2, y - treeView.ItemHeight / 2),
                                new Point((Key.time - shift) * treeView.ItemHeight, y - treeView.ItemHeight),
                                new Point((Key.time - shift) * treeView.ItemHeight + treeView.ItemHeight / 2, y - treeView.ItemHeight / 2),
                                new Point((Key.time - shift) * treeView.ItemHeight, y)});
                        }
                        y += treeView.ItemHeight;
                    }

                    if (y > pictureBox.Height) break;

                    if (nodes[i].Nodes.Count > 0 && nodes[i].IsExpanded)
                        Tree(nodes[i].Nodes);
                }
            }

            g.DrawLine(pen, (PropertyBase.Time - shift) * treeView.ItemHeight, 0, (PropertyBase.Time - shift) * treeView.ItemHeight, pictureBox.Height);
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;

            PropertyBase.Time = MouseTime(e.X) + shift;

            int y = treeView.ItemHeight;

            movingKey = false;

            Tree(treeView.Nodes);

            void Tree(TreeNodeCollection nodes)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].IsVisible)
                    {
                        if (e.Y > y - treeView.ItemHeight && e.Y < y)
                            selectedNode = nodes[i];
                        if (nodes[i] == selectedNode && nodes[i].Tag is IAnimatedProperty animatedProperty)
                        {
                            foreach (ITime Key in animatedProperty.Keys)
                                if (Key.time == MouseTime(e.X) + shift)
                                {
                                    movingKey = true;
                                    SelectedKey = Key;
                                }
                        }
                        y += treeView.ItemHeight;
                    }

                    if (y > pictureBox.Height) break;

                    if (nodes[i].Nodes.Count > 0 && nodes[i].IsExpanded)
                        Tree(nodes[i].Nodes);
                }
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            movingKey = false;
            SelectedKey = null;
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
                PropertyBase.Time = MouseTime(e.X) + shift < 0 ? 0 : MouseTime(e.X) + shift;

            if (movingKey && selectedNode.Tag is IAnimatedProperty animatedProperty)
            {
                foreach (ITime Key in animatedProperty.Keys)
                    if (Key.time == PropertyBase.Time)
                        return;

                animatedProperty.MoveKey(SelectedKey.time, PropertyBase.Time);
            }
        }

        private void pictureBox_DoubleClick(object sender, EventArgs e)
        {
            if (selectedNode.Tag is Animated animated)
            {
                foreach (PropertyBase property in animated.Properties)
                    if (property is IAnimatedProperty animatedProperty)
                        animatedProperty.AddKey(PropertyBase.Time);
            }
            else if (selectedNode.Tag is IAnimatedProperty animatedProperty)
            {
                animatedProperty.AddKey(PropertyBase.Time);
            }
        }
    }
}
