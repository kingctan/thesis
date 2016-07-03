using System.IO;
using System.Windows;
using System.Windows.Xps.Packaging;

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    /// <summary>
    /// Interaction logic for InstructionsWindow.xaml
    /// </summary>
    public partial class InstructionsWindow : Window
    {
        public InstructionsWindow()
        {
            InitializeComponent();
            XpsDocument xpsDocument = new XpsDocument(@"Documents\\Instructions.xps", FileAccess.Read);
            documentViewer.Document = xpsDocument.GetFixedDocumentSequence();
            xpsDocument.Close();
        }
    }
}