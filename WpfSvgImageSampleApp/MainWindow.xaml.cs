using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfSvgImageSampleApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void AlertImage_MouseEnter(object sender, MouseEventArgs e)
    {
        // Surrounding the edit calls with BeginEdit and EndEdit is not required, but will avoid multiple Clone operations 
        // so will improve performance if more than one edit is done
        AlertImage.BeginEdit();
        AlertImage.ReplaceFillBrush(new SolidColorBrush(Color.FromArgb(76,50,50,50)), new SolidColorBrush(Color.FromRgb(255, 0, 0)));
        AlertImage.ReplaceStrokeBrush(new SolidColorBrush(Color.FromRgb(50,50,50)), new SolidColorBrush(Color.FromRgb(0, 255, 0)));
        AlertImage.EndEdit();
    }

    private void AlertImage_MouseLeave(object sender, MouseEventArgs e)
    {
        // Reset reloads the original SVG, removing any edits made
        AlertImage.Reset();
    }
}