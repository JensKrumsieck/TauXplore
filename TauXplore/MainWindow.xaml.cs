using ChemSharp.Mathematics;
using ChemSharp.Molecules.HelixToolkit;
using HelixToolkit.Wpf;
using System.Linq;
using System.Windows;
using TauXplore.ViewModel;
using ThemeCommons.Controls;

namespace TauXplore;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : DefaultWindow
{
    public MainViewModel ViewModel { get; }
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        DataContext = ViewModel;
    }

    private void Viewport3D_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var files = e.Data.GetData(DataFormats.FileDrop, false) as string[];
        var filename = files?[0] ?? "";
        if (filename != "") ViewModel.LoadFile(filename);
        ((HelixViewport3D)sender).CameraController.CameraTarget =
            MathV.Centroid(ViewModel.Molecule.Atoms.Select(a => a.Location).ToList()).ToPoint3D();

    }
}

