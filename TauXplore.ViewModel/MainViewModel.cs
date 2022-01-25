using ChemSharp.Molecules;
using ChemSharp.Molecules.HelixToolkit;
using System.Collections.ObjectModel;
using System.Windows.Media;
using TinyMVVM;

namespace TauXplore.ViewModel;
public class MainViewModel : ListingViewModel<AnalysisViewModel>
{
    private Molecule molecule;
    public Molecule Molecule { get => molecule; set => Set(ref molecule, value); }
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

    /// <summary>
    /// 3D Representation of Atoms
    /// </summary>
    public ObservableCollection<Atom3D> Atoms3D { get; } = new ObservableCollection<Atom3D>();
    /// <summary>
    /// 3D Representation of Bonds
    /// </summary>
    public ObservableCollection<Bond3D> Bonds3D { get; } = new ObservableCollection<Bond3D>();
}
