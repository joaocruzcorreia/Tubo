﻿namespace MapNoReduce
{
    partial class Form1
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
            this.runScriptButton = new System.Windows.Forms.Button();
            this.commandButton = new System.Windows.Forms.Button();
            this.pathLabel = new System.Windows.Forms.Label();
            this.commandLabel = new System.Windows.Forms.Label();
            this.scriptTb = new System.Windows.Forms.TextBox();
            this.commandTb = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // runScriptButton
            // 
            this.runScriptButton.Location = new System.Drawing.Point(299, 30);
            this.runScriptButton.Name = "runScriptButton";
            this.runScriptButton.Size = new System.Drawing.Size(75, 23);
            this.runScriptButton.TabIndex = 0;
            this.runScriptButton.Text = "Run Script";
            this.runScriptButton.UseVisualStyleBackColor = true;
            // 
            // commandButton
            // 
            this.commandButton.Location = new System.Drawing.Point(299, 104);
            this.commandButton.Name = "commandButton";
            this.commandButton.Size = new System.Drawing.Size(75, 23);
            this.commandButton.TabIndex = 3;
            this.commandButton.Text = "Submit";
            this.commandButton.UseVisualStyleBackColor = true;
            this.commandButton.Click += new System.EventHandler(this.commandButton_Click);
            // 
            // pathLabel
            // 
            this.pathLabel.AutoSize = true;
            this.pathLabel.Location = new System.Drawing.Point(12, 17);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(103, 13);
            this.pathLabel.TabIndex = 4;
            this.pathLabel.Text = "Script Absolute Path";
            // 
            // commandLabel
            // 
            this.commandLabel.AutoSize = true;
            this.commandLabel.Location = new System.Drawing.Point(9, 88);
            this.commandLabel.Name = "commandLabel";
            this.commandLabel.Size = new System.Drawing.Size(54, 13);
            this.commandLabel.TabIndex = 5;
            this.commandLabel.Text = "Command";
            this.commandLabel.Click += new System.EventHandler(this.runScriptClick);
            // 
            // scriptTb
            // 
            this.scriptTb.Location = new System.Drawing.Point(15, 33);
            this.scriptTb.Name = "scriptTb";
            this.scriptTb.Size = new System.Drawing.Size(278, 20);
            this.scriptTb.TabIndex = 7;
            this.scriptTb.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // commandTb
            // 
            this.commandTb.Location = new System.Drawing.Point(15, 106);
            this.commandTb.Name = "commandTb";
            this.commandTb.Size = new System.Drawing.Size(278, 20);
            this.commandTb.TabIndex = 8;
            this.commandTb.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 234);
            this.Controls.Add(this.commandTb);
            this.Controls.Add(this.scriptTb);
            this.Controls.Add(this.commandLabel);
            this.Controls.Add(this.pathLabel);
            this.Controls.Add(this.commandButton);
            this.Controls.Add(this.runScriptButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button runScriptButton;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.Button commandButton;
        private System.Windows.Forms.Label commandLabel;
        private System.Windows.Forms.TextBox scriptTb;
        private System.Windows.Forms.TextBox commandTb;
    }
}

