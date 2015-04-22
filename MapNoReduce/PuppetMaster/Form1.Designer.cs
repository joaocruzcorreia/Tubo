namespace PuppetMaster
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
            this.scriptPathTextBox = new System.Windows.Forms.TextBox();
            this.commandButton = new System.Windows.Forms.Button();
            this.pathLabel = new System.Windows.Forms.Label();
            this.commandLabel = new System.Windows.Forms.Label();
            this.commandTextBox = new System.Windows.Forms.TextBox();
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
            // scriptPathTextBox
            // 
            this.scriptPathTextBox.Location = new System.Drawing.Point(12, 33);
            this.scriptPathTextBox.Name = "scriptPathTextBox";
            this.scriptPathTextBox.Size = new System.Drawing.Size(281, 20);
            this.scriptPathTextBox.TabIndex = 1;
            // 
            // commandButton
            // 
            this.commandButton.Location = new System.Drawing.Point(299, 104);
            this.commandButton.Name = "commandButton";
            this.commandButton.Size = new System.Drawing.Size(75, 23);
            this.commandButton.TabIndex = 3;
            this.commandButton.Text = "Submit";
            this.commandButton.UseVisualStyleBackColor = true;
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
            // commandTextBox
            // 
            this.commandTextBox.Location = new System.Drawing.Point(12, 104);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(281, 20);
            this.commandTextBox.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 139);
            this.Controls.Add(this.commandTextBox);
            this.Controls.Add(this.commandLabel);
            this.Controls.Add(this.pathLabel);
            this.Controls.Add(this.commandButton);
            this.Controls.Add(this.scriptPathTextBox);
            this.Controls.Add(this.runScriptButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button runScriptButton;
        private System.Windows.Forms.TextBox scriptPathTextBox;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.Button commandButton;
        private System.Windows.Forms.Label commandLabel;
        private System.Windows.Forms.TextBox commandTextBox;
    }
}

