
namespace SqlCollegeTranscripts
{
	partial class frmCaptions
	{
		#region "Windows Form Designer generated code "

		private string[] visualControls = new string[]{"components", "ToolTipMain", "_label_0", "label"};
		//Required by the Windows Form Designer
		private System.ComponentModel.IContainer components;
		internal System.Windows.Forms.ToolTip ToolTipMain;
		internal System.Windows.Forms.Label[] label = new System.Windows.Forms.Label[1];
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.ToolTipMain = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // frmCaptions
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(310, 36);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Location = new System.Drawing.Point(4, 30);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCaptions";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form1";
            this.Closed += new System.EventHandler(this.Form_Closed);
            this.Load += new System.EventHandler(this.frmCaptions_Load);
            this.ResumeLayout(false);

		}

		#endregion
	}
}