
namespace PreviewMaker3D
{
    partial class VideoRenderForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.intModifier1 = new PreviewMaker3D.IntModifier();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.intModifier2 = new PreviewMaker3D.IntModifier();
            this.intModifier3 = new PreviewMaker3D.IntModifier();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // intModifier1
            // 
            this.intModifier1.Location = new System.Drawing.Point(57, 12);
            this.intModifier1.Name = "intModifier1";
            this.intModifier1.ShortcutsEnabled = false;
            this.intModifier1.Size = new System.Drawing.Size(54, 22);
            this.intModifier1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "FPS:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "From:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "To:";
            // 
            // intModifier2
            // 
            this.intModifier2.Location = new System.Drawing.Point(57, 40);
            this.intModifier2.Name = "intModifier2";
            this.intModifier2.ShortcutsEnabled = false;
            this.intModifier2.Size = new System.Drawing.Size(54, 22);
            this.intModifier2.TabIndex = 4;
            // 
            // intModifier3
            // 
            this.intModifier3.Location = new System.Drawing.Point(57, 68);
            this.intModifier3.Name = "intModifier3";
            this.intModifier3.ShortcutsEnabled = false;
            this.intModifier3.Size = new System.Drawing.Size(54, 22);
            this.intModifier3.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 96);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(99, 48);
            this.button1.TabIndex = 6;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // VideoRenderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(123, 156);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.intModifier3);
            this.Controls.Add(this.intModifier2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.intModifier1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "VideoRenderForm";
            this.Text = "VideoRenderForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private IntModifier intModifier1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private IntModifier intModifier2;
        private IntModifier intModifier3;
        private System.Windows.Forms.Button button1;
    }
}