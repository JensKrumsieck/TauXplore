using ChemSharp.Molecules;
using System.Numerics;

namespace TauXplore.ViewModel.Extension
{
    public static class TriangulationUtil
    {
        public static List<Vector3[]> FanTriangulation(this IList<Atom> atoms)
        {
            var res = new List<Vector3[]>();
            for (var i = 2; i < atoms.Count(); i++)
            {
                res.Add(new Vector3[] { atoms[0].Location, atoms[i].Location, atoms[i - 1].Location });
            }
            return res;
        }

        public static float SignedVolumeOfTriangle(Vector3[] vertices)
        {
            //https://stackoverflow.com/questions/1406029/how-to-calculate-the-volume-of-a-3d-mesh-object-the-surface-of-which-is-made-up
            var p1 = vertices[0];
            var p2 = vertices[1];
            var p3 = vertices[2];
            var v321 = p3.X * p2.Y * p1.Z;
            var v231 = p2.X * p3.Y * p1.Z;
            var v312 = p3.X * p1.Y * p2.Z;
            var v132 = p1.X * p3.Y * p2.Z;
            var v213 = p2.X * p1.Y * p3.Z;
            var v123 = p1.X * p2.Y * p3.Z;
            return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
        }

        public static double MeshVolume(this IList<Atom> atoms)
        {
            var triangles = atoms.FanTriangulation();
            var volumes = from tri in triangles
                          select SignedVolumeOfTriangle(tri);
            return Math.Abs(volumes.Sum());
        }
    }
}
