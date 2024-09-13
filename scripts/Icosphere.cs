using Godot;
using System;

// private struct TriangleIndices
// {
//   public int v1;
//   public int v2;
//   public int v3;

//   public TriangleIndices(int v1, int v2, int v3)
//   {
//     this.v1 = v1;
//     this.v2 = v2;
//     this.v3 = v3;
//   }
// }

public partial class Icosphere : Node3D
{
  MeshInstance3D _meshInstance;

  public override void _Ready()
  {
    _meshInstance = new MeshInstance3D();
    AddChild(_meshInstance);
  }

}

// using System;
// using System.Collections.Generic;
// using UnityEngine;

// public static class IcoSphere
// {
//   private struct TriangleIndices
//   {
//     public int v1;
//     public int v2;
//     public int v3;

//     public TriangleIndices(int v1, int v2, int v3)
//     {
//       this.v1 = v1;
//       this.v2 = v2;
//       this.v3 = v3;
//     }
//   }

//   // return index of point in the middle of p1 and p2
//   private static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
//   {
//     // first check if we have it already
//     bool firstIsSmaller = p1 < p2;
//     long smallerIndex = firstIsSmaller ? p1 : p2;
//     long greaterIndex = firstIsSmaller ? p2 : p1;
//     long key = (smallerIndex << 32) + greaterIndex;

//     int ret;
//     if (cache.TryGetValue(key, out ret))
//     {
//       return ret;
//     }

//     // not in cache, calculate it
//     Vector3 point1 = vertices[p1];
//     Vector3 point2 = vertices[p2];
//     Vector3 middle = new Vector3
//     (
//         (point1.x + point2.x) / 2f,
//         (point1.y + point2.y) / 2f,
//         (point1.z + point2.z) / 2f
//     );

//     // add vertex makes sure point is on unit sphere
//     int i = vertices.Count;
//     vertices.Add(middle.normalized * radius);

//     // store it, return index
//     cache.Add(key, i);

//     return i;
//   }

//   private static float i = 0f;
//   private static float random()
//   {
//     /*i += 0.01f;
//     if( i > 3 ) i = 0f;
//     return 0f;
//     return i;*/
//     return UnityEngine.Random.Range(-10, 10) / 10f;
//   }

//   private static int FindIndex(List<IcoPoint> vertices, float x, float y, float z)
//   {
//     return vertices.FindIndex(vertex => vertex.X.CompareTo(x) == 0 && vertex.Y.CompareTo(y) == 0 && vertex.Z.CompareTo(z) == 0);
//   }
//   private static System.Predicate<IcoPoint> IsEqualPoint = delegate (IcoPoint vertex)
//   {
//     return vertex.X.CompareTo(-1f) == 0 && vertex.Y.CompareTo(1.618034f) == 0 && vertex.Z.CompareTo(0f) == 0;
//   };

//   private static float Perlin3D(float x, float y, float z)
//   {
//     float AB = Mathf.PerlinNoise(x, y);
//     float BC = Mathf.PerlinNoise(y, z);
//     float AC = Mathf.PerlinNoise(x, z);

//     float BA = Mathf.PerlinNoise(y, x);
//     float CB = Mathf.PerlinNoise(z, y);
//     float CA = Mathf.PerlinNoise(z, x);

//     return (AB + BC + AC + BA + CB + CA) / 6f;
//   }

//   private static int AddPoint(ref List<IcoPoint> vertices, IcoPoint point)
//   {
//     var index = FindIndex(vertices, point.X, point.Y, point.Z);
//     if (index != -1) return index;

//     vertices.Add(point);
//     point.id = vertices.Count - 1;
//     return vertices.Count - 1;
//   }

//   private static bool AuditPoints(List<IcoPoint> vertices)
//   {
//     if (vertices.Count == 0) return true;

//     int i = 0;
//     Debug.Log("AuditPoints: " + ++i);

//     // Set all found flags to false
//     vertices.ForEach(vertex1 => vertex1.Found = false);
//     Debug.Log("AuditPoints: " + ++i);

//     // Validate amount of neighbors
//     var invalid = vertices.FindAll(vertex2 =>
//     {
//       //Debug.Log( "POINT COUNT: " + vertex2.Points.Count );
//       return vertex2.Points.Count < 5;
//     });

//     if (invalid.Count > 0)
//     {
//       invalid.ForEach(v1 =>
//       {
//         string neighbors = "";

//         v1.Points.ForEach(v2 =>
//         {
//           neighbors += FindIndex(vertices, v2.X, v2.Y, v2.Z) + ",";
//         });

//         if (neighbors.Length > 0) neighbors = neighbors.Remove(neighbors.Length - 1);
//         Debug.Log("Neighbors for " + (FindIndex(vertices, v1.X, v1.Y, v1.Z)) + ": " + neighbors);
//       });
//       return false;
//     }
//     Debug.Log("AuditPoints: " + ++i);

//     if (vertices.Count > 0)
//     {
//       // Build the seed list
//       IcoPoint vertex;
//       var search = new List<IcoPoint>();
//       search.Add(vertices[0]);
//       Debug.Log("AuditPoints: " + ++i);

//       int n = 0;

//       // Loop while the list isn't empty
//       while (search.Count >= 1 && n++ < vertices.Count * 2)
//       {
//         // Pull off the first index
//         vertex = search[0];
//         search.RemoveAt(0);

//         vertex.Points.ForEach(point =>
//         {
//           if (!point.Found)
//           {
//             search.Add(point);
//             point.Found = true;
//           }
//         });
//       }
//       Debug.Log("AuditPoints: " + ++i);
//     }

//     // Look for any not marked Found
//     invalid = vertices.FindAll(vertex3 => !vertex3.Found);
//     if (invalid.Count > 0) return false;
//     Debug.Log("AuditPoints: " + ++i);

//     return true;
//   }

//   public static void Create(GameObject gameObject, int recursionLevel, int smoothingLevel)
//   {
//     Debug.Log("Recursion Level: " + recursionLevel);
//     Debug.Log("Smoothing Level: " + smoothingLevel);

//     MeshFilter filter = gameObject.GetComponent<MeshFilter>();
//     Mesh mesh = filter.mesh;
//     mesh.Clear();
//     Vector3[] vertices = gameObject.GetComponent<MeshFilter>().mesh.vertices;
//     List<Vector3> vertList = new List<Vector3>();
//     Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
//     int index = 0;

//     float radius = 5f;

//     // create 12 vertices of a icosahedron
//     float t = (1f + Mathf.Sqrt(5f)) / 2f;

//     List<IcoPoint> _vertexes = new List<IcoPoint>();
//     Debug.Log("T: " + t);
//     _vertexes.Add(new IcoPoint(-1f, t, 0f));
//     _vertexes.Add(new IcoPoint(1f, t, 0f));
//     _vertexes.Add(new IcoPoint(-1f, -t, 0f));
//     _vertexes.Add(new IcoPoint(1f, -t, 0f));

//     _vertexes.Add(new IcoPoint(0f, -1f, t));
//     _vertexes.Add(new IcoPoint(0f, 1f, t));
//     _vertexes.Add(new IcoPoint(0f, -1f, -t));
//     _vertexes.Add(new IcoPoint(0f, 1f, -t));

//     _vertexes.Add(new IcoPoint(t, 0f, -1f));
//     _vertexes.Add(new IcoPoint(t, 0f, 1f));
//     _vertexes.Add(new IcoPoint(-t, 0f, -1f));
//     _vertexes.Add(new IcoPoint(-t, 0f, 1f));

//     vertList.Add(_vertexes[0].Vector.normalized * radius);
//     vertList.Add(_vertexes[1].Vector.normalized * radius);
//     vertList.Add(_vertexes[2].Vector.normalized * radius);
//     vertList.Add(_vertexes[3].Vector.normalized * radius);

//     vertList.Add(_vertexes[4].Vector.normalized * radius);
//     vertList.Add(_vertexes[5].Vector.normalized * radius);
//     vertList.Add(_vertexes[6].Vector.normalized * radius);
//     vertList.Add(_vertexes[7].Vector.normalized * radius);

//     vertList.Add(_vertexes[8].Vector.normalized * radius);
//     vertList.Add(_vertexes[9].Vector.normalized * radius);
//     vertList.Add(_vertexes[10].Vector.normalized * radius);
//     vertList.Add(_vertexes[11].Vector.normalized * radius);

//     // create 20 triangles of the icosahedron
//     List<TriangleIndices> faces = new List<TriangleIndices>();

//     // 5 faces around point 0
//     faces.Add(new TriangleIndices(0, 11, 5));
//     faces.Add(new TriangleIndices(0, 5, 1));
//     faces.Add(new TriangleIndices(0, 1, 7));
//     faces.Add(new TriangleIndices(0, 7, 10));
//     faces.Add(new TriangleIndices(0, 10, 11));

//     _vertexes[0].AddPoint(_vertexes[1]);
//     _vertexes[0].AddPoint(_vertexes[5]);
//     _vertexes[0].AddPoint(_vertexes[7]);
//     _vertexes[0].AddPoint(_vertexes[10]);
//     _vertexes[0].AddPoint(_vertexes[11]);

//     _vertexes[1].AddPoint(_vertexes[5]);
//     _vertexes[1].AddPoint(_vertexes[7]);
//     _vertexes[5].AddPoint(_vertexes[11]);
//     _vertexes[7].AddPoint(_vertexes[10]);
//     _vertexes[10].AddPoint(_vertexes[11]);

//     // 5 adjacent faces
//     faces.Add(new TriangleIndices(1, 5, 9));
//     faces.Add(new TriangleIndices(5, 11, 4));
//     faces.Add(new TriangleIndices(11, 10, 2));
//     faces.Add(new TriangleIndices(10, 7, 6));
//     faces.Add(new TriangleIndices(7, 1, 8));

//     _vertexes[1].AddPoint(_vertexes[8]);
//     _vertexes[1].AddPoint(_vertexes[9]);
//     _vertexes[2].AddPoint(_vertexes[10]);
//     _vertexes[2].AddPoint(_vertexes[11]);
//     _vertexes[4].AddPoint(_vertexes[5]);
//     _vertexes[4].AddPoint(_vertexes[11]);
//     _vertexes[5].AddPoint(_vertexes[9]);
//     _vertexes[6].AddPoint(_vertexes[7]);
//     _vertexes[6].AddPoint(_vertexes[10]);
//     _vertexes[7].AddPoint(_vertexes[8]);

//     // 5 faces around point 3
//     faces.Add(new TriangleIndices(3, 9, 4));
//     faces.Add(new TriangleIndices(3, 4, 2));
//     faces.Add(new TriangleIndices(3, 2, 6));
//     faces.Add(new TriangleIndices(3, 6, 8));
//     faces.Add(new TriangleIndices(3, 8, 9));

//     _vertexes[2].AddPoint(_vertexes[3]);
//     _vertexes[2].AddPoint(_vertexes[4]);
//     _vertexes[2].AddPoint(_vertexes[6]);
//     _vertexes[3].AddPoint(_vertexes[4]);
//     _vertexes[3].AddPoint(_vertexes[6]);
//     _vertexes[3].AddPoint(_vertexes[8]);
//     _vertexes[3].AddPoint(_vertexes[9]);
//     _vertexes[4].AddPoint(_vertexes[9]);
//     _vertexes[6].AddPoint(_vertexes[8]);

//     // 5 adjacent faces
//     faces.Add(new TriangleIndices(4, 9, 5));
//     faces.Add(new TriangleIndices(2, 4, 11));
//     faces.Add(new TriangleIndices(6, 2, 10));
//     faces.Add(new TriangleIndices(8, 6, 7));
//     faces.Add(new TriangleIndices(9, 8, 1));

//     _vertexes[8].AddPoint(_vertexes[9]);

//     //if( !AuditPoints( _vertexes ) ) Debug.Log( "INVALID" );

//     // refine triangles
//     int i = 0;
//     for (i = 0; i < recursionLevel; i++)
//     {
//       List<TriangleIndices> faces2 = new List<TriangleIndices>();
//       foreach (var tri in faces)
//       {
//         float rand = 0f;//random();
//                         //rand = Perlin.Noise( vertex.X, vertex.Y, vertex.Z );
//                         //Debug.Log( "RAND: " + rand );

//         /*if( _vertexes[ tri.v1 ].Height <= 0 && _vertexes[ tri.v2 ].Height <= 0 && _vertexes[ tri.v3 ].Height <= 0 ) {
//             faces2.Add( new TriangleIndices( tri.v1, tri.v2, tri.v3 ) );
//             continue;
//         }*/

//         // replace triangle by 4 triangles
//         int a = getMiddlePoint(tri.v1, tri.v2, ref vertList, ref middlePointIndexCache, radius + rand);
//         int b = getMiddlePoint(tri.v2, tri.v3, ref vertList, ref middlePointIndexCache, radius + rand);
//         int c = getMiddlePoint(tri.v3, tri.v1, ref vertList, ref middlePointIndexCache, radius + rand);

//         a = AddPoint(ref _vertexes, new IcoPoint(vertList[a].x, vertList[a].y, vertList[a].z));
//         b = AddPoint(ref _vertexes, new IcoPoint(vertList[b].x, vertList[b].y, vertList[b].z));
//         c = AddPoint(ref _vertexes, new IcoPoint(vertList[c].x, vertList[c].y, vertList[c].z));

//         _vertexes[tri.v1].SwapPoint(_vertexes[tri.v2], _vertexes[a]);
//         _vertexes[tri.v2].SwapPoint(_vertexes[tri.v3], _vertexes[b]);
//         _vertexes[tri.v3].SwapPoint(_vertexes[tri.v1], _vertexes[c]);

//         faces2.Add(new TriangleIndices(tri.v1, a, c));
//         faces2.Add(new TriangleIndices(tri.v2, b, a));
//         faces2.Add(new TriangleIndices(tri.v3, c, b));
//         faces2.Add(new TriangleIndices(a, b, c));

//         _vertexes[a].AddPoint(_vertexes[c]);
//         _vertexes[a].AddPoint(_vertexes[b]);
//         _vertexes[b].AddPoint(_vertexes[c]);
//       }
//       faces = faces2;
//     }

//     if (!AuditPoints(_vertexes)) Debug.Log("INVALID");
//     else Debug.Log("VALID POINTS");

//     Debug.Log("POINTS: " + _vertexes.Count);

//     IcoPoint node;
//     List<IcoPoint> nodes = new List<IcoPoint>();
//     /*nodes.Add( _vertexes[ 0 ] );

//     _vertexes.ForEach( vertex => vertex.Found = false );
//     while( nodes.Count > 0 ) {
//         node = nodes[ 0 ];
//         nodes.RemoveAt( 0 );

//         node.Height = random();
//         vertList[ node.id ] = node.Vector.normalized * ( radius + node.Height );
//         node.Points.FindAll( vertex => vertex.Found == false ).ForEach( v => {
//             v.Found = true;
//             nodes.Add( v );
//         } );
//     }*/

//     //_vertexes.ForEach( vertex => vertex.Height = random() );
//     _vertexes.ForEach(vertex => vertex.Height = Math.Max(0, (Perlin.Noise(vertex.X, vertex.Y, vertex.Z) * 6) - 2));

//     int n = 0;
//     int max = 5;
//     for (i = 0; i < smoothingLevel; i++)
//     {
//       n = 0;
//       _vertexes.ForEach(vertex =>
//       {
//         var oh = vertex.Height;
//         var height = vertex.Height;
//         int l = 0;
//         vertex.Points.ForEach(p =>
//         {
//           height += p.Height;
//           if (p.Height > 0) l++;
//         });
//         if (n++ <= max)
//         {
//           Debug.Log("Sum: " + height);
//         }

//         vertex.Height = (height / (l + 1));

//         if (n++ <= max)
//         {
//           Debug.Log("Original: " + oh + " --- Height: " + vertex.Height);
//         }
//       });
//     }

//     /*_vertexes.ForEach( vertex => {
//         bool shore = false;
//         vertex.Points.ForEach( v => {
//             if( v.Height <= 0 ) shore = true;
//         } );

//         if( shore ) vertex.Height = 0;
//         else vertex.Height = 0;
//     } );*/

//     _vertexes.ForEach(vertex => vertList[vertex.id] = vertex.Vector.normalized * (radius + vertex.Height));

//     mesh.vertices = vertList.ToArray();

//     List<int> triList = new List<int>();
//     for (i = 0; i < faces.Count; i++)
//     {
//       triList.Add(faces[i].v1);
//       triList.Add(faces[i].v2);
//       triList.Add(faces[i].v3);
//     }
//     mesh.triangles = triList.ToArray();
//     mesh.uv = new Vector2[vertices.Length];

//     Vector3[] normales = new Vector3[vertList.Count];
//     for (i = 0; i < normales.Length; i++)
//       normales[i] = vertList[i].normalized;


//     mesh.normals = normales;

//     mesh.RecalculateBounds();
//     mesh.RecalculateTangents();
//     mesh.RecalculateNormals();
//     mesh.Optimize();
//   }
// }