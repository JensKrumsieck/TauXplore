using ChemSharp.Molecules;
using ChemSharp.Molecules.Properties;
using TauXplore.ViewModel.Extension;
using TinyMVVM;

namespace TauXplore.ViewModel;
public class AnalysisViewModel : ListItemViewModel<MainViewModel, AnalysisViewModel>
{
    private readonly Guid _guid;
    public List<Atom> CoordinationSphere { get; }

    public Atom Metal { get; }

    public int CoordinationNumber => CoordinationSphere.Count - 1;

    public override string Title => Metal.Title;

    public string Color => _guid.HexStringFromGuid();

    public AnalysisViewModel(MainViewModel parent, List<Atom> atoms) : base(parent)
    {
        _guid = Guid.NewGuid();
        CoordinationSphere = atoms;
        Metal = atoms.Last();
        CalculateProperties();
    }

    private void CalculateProperties()
    {
        var list = CoordinationSphere.Where(a => a != Metal).ToList(); //precalculate to save energy
        Distances.AddRange(from atom in list
                           select new Distance(atom, Metal));
        Angles.AddRange(from item in list.KCombs<Atom>(2)
                        select new Angle(item.First(), Metal, item.Last()));
        CalculateGeometryIndex();
    }
    private void CalculateGeometryIndex()
    {
        var ordered = from angle in Angles
                      orderby angle.Value descending
                      select angle;
        var a = ordered.ElementAt(0).Value;
        var b = ordered.ElementAt(1).Value;
        var theta = Math.Acos(-1d / 3d) * 180d / Math.PI;
        if (CoordinationNumber == 4)
        {
            var tau = (360 - (a + b)) / (360 - 2 * theta);
            var tau2 = (a - b) / (360 - theta) + (180 - a) / (180 - theta);
            var sigma2 = Angles.Sum(s => Math.Pow(s.Value - theta, 2));// σ²           
            StructuralParameters.Add(new KeyValueProperty() { Key = "Geometry Index τ₄", Value = tau });
            StructuralParameters.Add(new KeyValueProperty() { Key = "Geometry Index τ₄'", Value = tau2 });
            StructuralParameters.Add(new KeyValueProperty() { Key = "Bond Angle Variance: σ²(tet)", Value = sigma2 / (Angles.Count - 1), Unit = "°²" });
        }
        if (CoordinationNumber == 5)
        {
            var tau = (a - b) / 60;
            StructuralParameters.Add(new KeyValueProperty() { Key = "Geometry Index τ₅", Value = tau });
        }
        var vol = CoordinationSphere.MeshVolume();
        StructuralParameters.Add(new KeyValueProperty() { Key = "Polyhedral Volume", Value = vol, Unit = "Å³" });

        var dmean = Distances.Sum(d => d.Value) / Distances.Count;
        StructuralParameters.Add(new KeyValueProperty() { Key = "Mean Distance: <D>", Value = dmean, Unit = "Å" });
        var zeta = Distances.Sum(d => Math.Abs(d.Value - dmean)); //ζ
        var delta = Distances.Sum(d => Math.Pow((d.Value - dmean) / dmean, 2)) / Distances.Count; //Δ
        var dindex = Distances.Sum(d => Math.Abs((d.Value - dmean) / dmean)) / Distances.Count; //Δ
        StructuralParameters.Add(new KeyValueProperty() { Key = "Length Distortion: ζ", Value = zeta, Unit = "Å" });
        StructuralParameters.Add(new KeyValueProperty() { Key = "Baur's Distortion Index: D", Value = dindex, Unit = "Å" });
        StructuralParameters.Add(new KeyValueProperty() { Key = "Rel. Length Distortion: Δ", Value = delta });
        if (CoordinationNumber == 6)
        {
            var nonCis = ordered.Skip(3);
            var sigma = nonCis.Sum(s => Math.Abs(s.Value - 90d)); //do not select 3 biggest (non cis-) angles, Σ     
            var sigma2 = nonCis.Sum(s => Math.Pow(s.Value - 90d, 2)); //do not select 3 biggest (non cis-) angles, σ²
            StructuralParameters.Add(new KeyValueProperty() { Key = "Octahedral Distortion: Σ", Value = sigma, Unit = "°" });
            StructuralParameters.Add(new KeyValueProperty() { Key = "Bond Angle Variance: σ²(oct)", Value = sigma2 / (nonCis.Count() - 1), Unit = "°²" });
        }
    }

    public List<Distance> Distances { get; } = new List<Distance>();
    public List<Angle> Angles { get; } = new List<Angle>();
    public List<KeyValueProperty> StructuralParameters { get; } = new List<KeyValueProperty>();

}
