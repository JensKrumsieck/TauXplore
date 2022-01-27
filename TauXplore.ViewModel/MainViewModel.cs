using ChemSharp.Molecules;
using ChemSharp.Molecules.HelixToolkit;
using HelixToolkit.Wpf;
using MIConvexHull;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using TinyMVVM;

namespace TauXplore.ViewModel;
public class MainViewModel : ListingViewModel<AnalysisViewModel>
{
    private Molecule molecule;
    private bool showPolyhedron;
    private Model3DGroup polyhedra = new();

    public Molecule Molecule { get => molecule; set => Set(ref molecule, value); }
    public bool ShowPolyhedron { get => showPolyhedron; set => Set(ref showPolyhedron, value, MeshFromHull); }

    public void LoadFile(string filename)
    {
        molecule = MoleculeFactory.Create(filename);
        Atoms3D.Clear();
        Bonds3D.Clear();
        Items.Clear();
        foreach (var atom in molecule.Atoms) Atoms3D.Add(new Atom3D(atom));
        foreach (var bond in molecule.Bonds) Bonds3D.Add(new Bond3D(bond));
        Analyze();
    }

    private void Analyze()
    {
        var metals = Molecule.Atoms.Where(s => !s.IsNonMetal).ToList();
        foreach (var metal in metals)
        {
            var analysis = new AnalysisViewModel(this, Molecule.Neighbors(metal).Append(metal).ToList());
            Items.Add(analysis);
            SelectedIndex = Items.IndexOf(analysis);
        }
        MeshFromHull();
        Validate();
    }

    private void Validate()
    {
        foreach (var a in Atoms3D)
            a.IsValid = false;
        foreach (var item in Items)
        {
            foreach (var a3d in item.CoordinationSphere.Select(a => Atoms3D.FirstOrDefault(s => s.Atom.Equals(a))).Where(a3d => a3d != null))
                a3d.IsValid = true;
            var validBonds = Bonds3D.Where(bond => item.CoordinationSphere.Count(atom => bond.Bond.Atoms.Contains(atom)) == 2);
            foreach (var bond in validBonds)
            {
                bond.Color = (Color)ColorConverter.ConvertFromString(item.Color)!;
                bond.IsValid = true;
            }
        }
    }
    public void MeshFromHull()
    {
        Polyhedra.Children.Clear();
        if (!ShowPolyhedron) return;
        foreach (var analysis in Items)
        {
            var verts = from atom in analysis.CoordinationSphere
                        select new DefaultVertex() { Position = new double[] { atom.Location.X, atom.Location.Y, atom.Location.Z } };

            var hull = ConvexHull.Create(verts.ToArray());
            var mb = new MeshBuilder();
            foreach (var face in hull.Result.Faces)
            {
                var v1 = face.Vertices[0].Position;
                var p1 = new Point3D(v1[0], v1[1], v1[2]);
                var v2 = face.Vertices[1].Position;
                var p2 = new Point3D(v2[0], v2[1], v2[2]);
                var v3 = face.Vertices[2].Position;
                var p3 = new Point3D(v3[0], v3[1], v3[2]);
                mb.AddTriangle(p1, p2, p3);
            }
            var model = new GeometryModel3D(mb.ToMesh(), MaterialHelper.CreateMaterial((Color)ColorConverter.ConvertFromString(analysis.Color)!));
            Polyhedra.Children.Add(model);
        }
    }

    /// <summary>
    /// 3D Representation of Atoms
    /// </summary>
    public ObservableCollection<Atom3D> Atoms3D { get; } = new ObservableCollection<Atom3D>();
    /// <summary>
    /// 3D Representation of Bonds
    /// </summary>
    public ObservableCollection<Bond3D> Bonds3D { get; } = new ObservableCollection<Bond3D>();

    public Model3DGroup Polyhedra { get => polyhedra; set => Set(ref polyhedra, value); }
}
