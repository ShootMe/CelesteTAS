using System;
using System.Windows.Forms;
namespace InputViewer {
	public partial class Viewer : Form {
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Viewer());
		}
		public Viewer() {
			InitializeComponent();
		}
	}
}
