
namespace WimBuilder
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.isoPicker = new System.Windows.Forms.OpenFileDialog();
            this.outputBox = new System.Windows.Forms.TextBox();
            this.isoDescription = new System.Windows.Forms.Label();
            this.isoSelectButton = new System.Windows.Forms.Button();
            this.click_GetWims = new System.Windows.Forms.Button();
            this.click_MountWims = new System.Windows.Forms.Button();
            this.WimList = new System.Windows.Forms.ListBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.click_GetAppX = new System.Windows.Forms.Button();
            this.click_RemoveAppxPackages = new System.Windows.Forms.Button();
            this.Discard = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // isoPicker
            // 
            this.isoPicker.DefaultExt = "iso";
            this.isoPicker.Filter = "ISO FILES|*.iso";
            // 
            // outputBox
            // 
            this.outputBox.Location = new System.Drawing.Point(12, 60);
            this.outputBox.Multiline = true;
            this.outputBox.Name = "outputBox";
            this.outputBox.ReadOnly = true;
            this.outputBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.outputBox.Size = new System.Drawing.Size(412, 358);
            this.outputBox.TabIndex = 2;
            this.outputBox.WordWrap = false;
            // 
            // isoDescription
            // 
            this.isoDescription.AutoSize = true;
            this.isoDescription.Location = new System.Drawing.Point(187, 9);
            this.isoDescription.Name = "isoDescription";
            this.isoDescription.Size = new System.Drawing.Size(80, 15);
            this.isoDescription.TabIndex = 7;
            this.isoDescription.Text = "Mounted ISO:";
            // 
            // isoSelectButton
            // 
            this.isoSelectButton.Location = new System.Drawing.Point(12, 5);
            this.isoSelectButton.Name = "isoSelectButton";
            this.isoSelectButton.Size = new System.Drawing.Size(123, 23);
            this.isoSelectButton.TabIndex = 8;
            this.isoSelectButton.Text = "Select an iso...";
            this.isoSelectButton.UseVisualStyleBackColor = true;
            this.isoSelectButton.Click += new System.EventHandler(this.FilePickerButton_Click);
            // 
            // click_GetWims
            // 
            this.click_GetWims.Location = new System.Drawing.Point(449, 36);
            this.click_GetWims.Name = "click_GetWims";
            this.click_GetWims.Size = new System.Drawing.Size(323, 38);
            this.click_GetWims.TabIndex = 9;
            this.click_GetWims.Text = "Get Wims in ISO";
            this.click_GetWims.UseVisualStyleBackColor = true;
            this.click_GetWims.Click += new System.EventHandler(this.GetWimInfo_Click);
            // 
            // click_MountWims
            // 
            this.click_MountWims.Location = new System.Drawing.Point(449, 80);
            this.click_MountWims.Name = "click_MountWims";
            this.click_MountWims.Size = new System.Drawing.Size(323, 36);
            this.click_MountWims.TabIndex = 10;
            this.click_MountWims.Text = "Mount Selected Wim";
            this.click_MountWims.UseVisualStyleBackColor = true;
            this.click_MountWims.Click += new System.EventHandler(this.MountWim_Click);
            // 
            // WimList
            // 
            this.WimList.FormattingEnabled = true;
            this.WimList.ItemHeight = 15;
            this.WimList.Location = new System.Drawing.Point(449, 296);
            this.WimList.Name = "WimList";
            this.WimList.Size = new System.Drawing.Size(281, 94);
            this.WimList.TabIndex = 11;
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(449, 422);
            this.listBox1.Name = "listBox1";
            this.listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.listBox1.Size = new System.Drawing.Size(547, 124);
            this.listBox1.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 15);
            this.label1.TabIndex = 13;
            this.label1.Text = "Log:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(449, 278);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 15);
            this.label2.TabIndex = 14;
            this.label2.Text = "EDU SKU\'s in ISO";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(449, 404);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(180, 15);
            this.label3.TabIndex = 15;
            this.label3.Text = "Appx Packages in Mounted Wim";
            // 
            // click_GetAppX
            // 
            this.click_GetAppX.Location = new System.Drawing.Point(449, 123);
            this.click_GetAppX.Name = "click_GetAppX";
            this.click_GetAppX.Size = new System.Drawing.Size(323, 38);
            this.click_GetAppX.TabIndex = 16;
            this.click_GetAppX.Text = "Get Appx Pacakges in Selected Wim";
            this.click_GetAppX.UseVisualStyleBackColor = true;
            this.click_GetAppX.Click += new System.EventHandler(this.GetAppxPackages_click);
            // 
            // click_RemoveAppxPackages
            // 
            this.click_RemoveAppxPackages.Location = new System.Drawing.Point(449, 168);
            this.click_RemoveAppxPackages.Name = "click_RemoveAppxPackages";
            this.click_RemoveAppxPackages.Size = new System.Drawing.Size(323, 38);
            this.click_RemoveAppxPackages.TabIndex = 17;
            this.click_RemoveAppxPackages.Text = "Remove Selected AppxPackages From Mounted Wim";
            this.click_RemoveAppxPackages.UseVisualStyleBackColor = true;
            this.click_RemoveAppxPackages.Click += new System.EventHandler(this.RemoveAppx_click);
            // 
            // Discard
            // 
            this.Discard.Location = new System.Drawing.Point(801, 39);
            this.Discard.Name = "Discard";
            this.Discard.Size = new System.Drawing.Size(75, 23);
            this.Discard.TabIndex = 18;
            this.Discard.Text = "Discard";
            this.Discard.UseVisualStyleBackColor = true;
            this.Discard.Click += new System.EventHandler(this.Discard_Click);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(801, 69);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 19;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 661);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Discard);
            this.Controls.Add(this.click_RemoveAppxPackages);
            this.Controls.Add(this.click_GetAppX);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.WimList);
            this.Controls.Add(this.click_MountWims);
            this.Controls.Add(this.click_GetWims);
            this.Controls.Add(this.isoSelectButton);
            this.Controls.Add(this.isoDescription);
            this.Controls.Add(this.outputBox);
            this.Name = "Form1";
            this.Text = "WimBuilder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog isoPicker;
        public System.Windows.Forms.TextBox outputBox;
        private System.Windows.Forms.Label isoDescription;
        private System.Windows.Forms.Button isoSelectButton;
        private System.Windows.Forms.Button click_GetWims;
        private System.Windows.Forms.Button click_MountWims;
        private System.Windows.Forms.ListBox WimList;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button click_GetAppX;
        private System.Windows.Forms.Button click_RemoveAppxPackages;
        private System.Windows.Forms.Button Discard;
        private System.Windows.Forms.Button Save;
    }
}

