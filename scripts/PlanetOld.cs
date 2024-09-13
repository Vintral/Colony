using System.Collections.Generic;
using System.Linq;
using Godot;

public struct TriIndices
{
  public int v1;
  public int v2;
  public int v3;

  public TriIndices(int v1, int v2, int v3)
  {
    this.v1 = v1;
    this.v2 = v2;
    this.v3 = v3;
  }
}

public partial class PlanetOld : Node3D
{
  MeshInstance3D _root;
  Camera _camera;
  List<Vector3> _vertices;
  List<TriIndices> _faces;
  List<Vector2> _uvs;
  List<Vector3> _normals;
  List<int> _indexes;
  ArrayMesh _mesh;
  List<Vertex> _points;

  public override void _Ready()
  {
    Position = new Vector3(0, 0, 0);

    _root = new MeshInstance3D();
    AddChild(_root);

    var mesh = new SphereMesh
    {
      Height = 5f,
      Radius = 2.5f,
      Rings = 10,
      RadialSegments = 10,
    };
    _root.Mesh = mesh;

    Create(0, 0);
  }

  List<Vector3> buildVertices()
  {
    GD.Print("buildVertices");

    var x = .5f;
    var ret = new List<Vector3>
    {
      new(-x,-x,-x),
      new(-x,x,-x),
      new(x,x,-x),
      new(x,-x,-x),
      new(-x,-x,x),
      new(-x,x,x),
      new(x,x,x),
      new(x,-x,x),
    };

    return ret;
  }

  List<Vector3> buildNormals()
  {
    GD.Print("buildNormals");

    var ret = new List<Vector3>();

    foreach (var v in _vertices)
    {
      ret.Add(v.Normalized());
    }

    return ret;
  }

  List<Vector2> buildUVs()
  {
    GD.Print("buildUVs");

    var v = new Vector2[] {
      new(0, 0),
      new(1, 0),
      new(1, 1),
      new(0, 1),
    };

    var ret = new List<Vector2>
    {
      v[0], v[1], v[2], v[3],
      v[0], v[1], v[2], v[3],
      v[0], v[1], v[2], v[3],
      v[0], v[1], v[2], v[3],
      v[0], v[1], v[2], v[3],
      v[0], v[1], v[2], v[3],
    };

    return ret;
  }

  int[] addQuad(int a, int b, int c, int d)
  {
    return new int[] { a, b, c, a, c, d };
  }

  List<int> buildIndexes()
  {
    GD.Print("buildIndexes");

    var ret = new List<int>();

    ret.AddRange(addQuad(3, 2, 1, 0));
    ret.AddRange(addQuad(4, 5, 6, 7));
    ret.AddRange(addQuad(0, 4, 7, 3));
    ret.AddRange(addQuad(5, 1, 2, 6));
    ret.AddRange(addQuad(0, 1, 5, 4));
    ret.AddRange(addQuad(7, 6, 2, 3));

    return ret;
  }

  int getMiddlePoint(int p1, int p2)// ref Dictionary<long, int> cache )
  {
    // first check if we have it already
    bool firstIsSmaller = p1 < p2;
    long smallerIndex = firstIsSmaller ? p1 : p2;
    long greaterIndex = firstIsSmaller ? p2 : p1;
    long key = (smallerIndex << 32) + greaterIndex;

    // int ret;
    // if (cache.TryGetValue(key, out ret))
    // {
    //   return ret;
    // }

    // not in cache, calculate it
    Vector3 point1 = _points[p1].Vector;
    Vector3 point2 = _points[p2].Vector;
    Vector3 middle = new(
        (point1.X + point2.X) / 2f,
        (point1.Y + point2.Y) / 2f,
        (point1.Z + point2.Z) / 2f
    );

    // add vertex makes sure point is on unit sphere
    int i = _points.Count;
    var position = middle.Normalized();
    _points.Add(new(position.X, position.Y, position.Z));

    // store it, return index
    //cache.Add(key, i);

    return i;
  }

  int FindIndex(float x, float y, float z)
  {
    return _points.FindIndex(vertex => vertex.X.CompareTo(x) == 0 && vertex.Y.CompareTo(y) == 0 && vertex.Z.CompareTo(z) == 0);
  }
  readonly System.Predicate<Vertex> IsEqualPoint = delegate (Vertex vertex)
  {
    return vertex.X.CompareTo(-1f) == 0 && vertex.Y.CompareTo(1.618034f) == 0 && vertex.Z.CompareTo(0f) == 0;
  };

  int AddPoint(Vertex point)
  {
    var index = FindIndex(point.X, point.Y, point.Z);
    if (index != -1) return index;

    GD.Print("Adding Point");
    _points.Add(point);
    // point.ID = _points.Count - 1;
    return _points.Count - 1;
  }

  void drawSphere(Vector3 position)
  {
    var instance = new MeshInstance3D();
    AddChild(instance);

    var mesh = new SphereMesh
    {
      Height = .5f,
      Radius = .5f,
      Rings = 10,
      RadialSegments = 10,
    };

    instance.Mesh = mesh;
    instance.Position = position;
  }

  void drawPoints()
  {
    GD.Print("drawPoints");

    _points.ForEach(point =>
    {
      var text = new DebugLabel()
      {
        Text = (point.ID - 1).ToString(),
        Position = point.Vector,
        FontSize = 100,
      };

      _root.AddChild(text);
    });
  }

  bool AuditPoints(List<Vertex> vertices)
  {
    if (vertices.Count == 0) return true;

    int i = 0;
    GD.Print("AuditPoints: " + ++i);

    // Set all found flags to false
    vertices.ForEach(vertex1 => vertex1.Found = false);
    GD.Print("AuditPoints: " + ++i);

    // Validate amount of neighbors
    var invalid = vertices.FindAll(vertex2 =>
    {
      //Debug.Log( "POINT COUNT: " + vertex2.Points.Count );
      return vertex2.Points.Count < 5;
    });

    if (invalid.Count > 0)
    {
      invalid.ForEach(v1 =>
      {
        string neighbors = "";

        v1.Points.ForEach(v2 =>
        {
          neighbors += FindIndex(v2.X, v2.Y, v2.Z) + ",";
        });

        if (neighbors.Length > 0) neighbors = neighbors.Remove(neighbors.Length - 1);
        GD.Print("Neighbors for " + (FindIndex(v1.X, v1.Y, v1.Z)) + ": " + neighbors);
      });
      return false;
    }
    GD.Print("AuditPoints: " + ++i);

    if (vertices.Count > 0)
    {
      // Build the seed list
      Vertex vertex;
      var search = new List<Vertex>();
      search.Add(vertices[0]);
      GD.Print("AuditPoints: " + ++i);

      int n = 0;

      // Loop while the list isn't empty
      while (search.Count >= 1 && n++ < vertices.Count * 2)
      {
        // Pull off the first index
        vertex = search[0];
        search.RemoveAt(0);

        vertex.Points.ForEach(point =>
        {
          if (!point.Found)
          {
            search.Add(point);
            point.Found = true;
          }
        });
      }
      GD.Print("AuditPoints: " + ++i);
    }

    // Look for any not marked Found
    invalid = vertices.FindAll(vertex3 => !vertex3.Found);
    if (invalid.Count > 0) return false;
    GD.Print("AuditPoints: " + ++i);

    return true;
  }

  public List<Vertex> buildIcoPoints()
  {
    var ret = new List<Vertex>();

    float t = (1f + Mathf.Sqrt(1f)) / 2f;

    GD.Print("T: " + t);

    ret.Add(new Vertex(-1f, t, 0f));
    ret.Add(new Vertex(1f, t, 0f));
    ret.Add(new Vertex(-1f, -t, 0f));
    ret.Add(new Vertex(1f, -t, 0f));

    ret.Add(new Vertex(0f, -1f, t));
    ret.Add(new Vertex(0f, 1f, t));
    ret.Add(new Vertex(0f, -1f, -t));
    ret.Add(new Vertex(0f, 1f, -t));

    ret.Add(new Vertex(t, 0f, -1f));
    ret.Add(new Vertex(t, 0f, 1f));
    ret.Add(new Vertex(-t, 0f, -1f));
    ret.Add(new Vertex(-t, 0f, 1f));

    return ret;
  }

  public int[] buildIndexesFromFaces()
  {
    var ret = new int[_faces.Count * 3];

    for (var i = 0; i < _faces.Count; i++)
    {
      ret[3 * i] = _faces[i].v1;
      ret[3 * i + 1] = _faces[i].v2;
      ret[3 * i + 2] = _faces[i].v3;
    }

    return ret;
  }

  void connectPoints(Dictionary<int, int[]> pointData)
  {
    foreach (var point in pointData)
    {
      GD.Print(point.Key + " -- " + point.Value);

      for (var i = 0; i < point.Value.Length; i++)
      {
        _points[point.Key].AddPoint(_points[point.Value[i]]);
      }
    }
  }

  void setHeights(float radius)
  {
    _points.ForEach(_point => _point.Height = radius);
  }

  float[] getVertexArray()
  {
    GD.Print("getVertexArray");

    var ret = new float[_points.Count];
    for (var i = 0; i < _points.Count; i++)
    {
      ret[3 * i] = _points[i].X;
      ret[3 * i + 1] = _points[i].Y;
      ret[3 * i + 2] = _points[i].Z;
    }

    return ret;
  }

  List<TriIndices> buildFaces()
  {
    GD.Print("buildFaces");

    return new List<TriIndices>
    {
      new(0, 1, 5),
      new(0, 5, 11),
      new(0, 7, 1 ),
      new(0, 10, 7 ),
      new(0, 11, 10),
      new(1, 9, 5),
      new(5, 4, 11),
      new(11, 2, 10),
      new(10, 6, 7),
      new(7, 8, 1),
      new(5,9,4),
      new(11,4,2),
      new(10, 2, 6),
      new(7,6,8),
      new(1,8,9),
      new(3,4,9),
      new(3,9,8),
      new(3,8,6),
      new(3,6,2),
      new(3,2,4),
    };
  }

  void divideFaces(int levelRecursion)
  {
    // refine triangles
    int i = 0;
    for (i = 0; i < levelRecursion; i++)
    {
      List<TriIndices> faces = new List<TriIndices>();
      foreach (var tri in _faces)
      {
        //float rand = 0f;//random();
        //rand = Perlin.Noise( vertex.X, vertex.Y, vertex.Z );
        //Debug.Log( "RAND: " + rand );

        /*if( _points[ tri.v1 ].Height <= 0 && _points[ tri.v2 ].Height <= 0 && _points[ tri.v3 ].Height <= 0 ) {
            faces2.Add( new TriangleIndices( tri.v1, tri.v2, tri.v3 ) );
            continue;
        }*/

        // replace triangle by 4 triangles
        int a = getMiddlePoint(tri.v1, tri.v2);//, ref vertList, ref middlePointIndexCache, radius + rand);
        int b = getMiddlePoint(tri.v2, tri.v3);//, ref vertList, ref middlePointIndexCache, radius + rand);
        int c = getMiddlePoint(tri.v3, tri.v1);//, ref vertList, ref middlePointIndexCache, radius + rand);

        _points[tri.v1].SwapPoint(_points[tri.v2], _points[a]);
        _points[tri.v2].SwapPoint(_points[tri.v3], _points[b]);
        _points[tri.v3].SwapPoint(_points[tri.v1], _points[c]);

        faces.Add(new TriIndices(tri.v1, a, c));
        faces.Add(new TriIndices(tri.v2, b, a));
        faces.Add(new TriIndices(tri.v3, c, b));
        faces.Add(new TriIndices(a, b, c));

        _points[a].AddPoint(_points[c]);
        _points[a].AddPoint(_points[b]);
        _points[b].AddPoint(_points[c]);
      }
      _faces = faces;
    }

  }

  public void Create(int levelRecursion, int levelSmoothing)
  {
    GD.Print("Create: " + levelRecursion + " -- " + levelSmoothing);

    // _vertices = buildVertices();
    // _uvs = buildUVs();
    // _indexes = buildIndexes();
    // _normals = buildNormals();

    // _mesh = new ArrayMesh();

    // var arrays = new Godot.Collections.Array();
    // arrays.Resize((int)Mesh.ArrayType.Max);
    // arrays[(int)Mesh.ArrayType.Vertex] = _vertices.ToArray();
    // //arrays[(int)Mesh.ArrayType.TexUV] = _uvs.ToArray();
    // arrays[(int)Mesh.ArrayType.Normal] = _normals.ToArray();
    // arrays[(int)Mesh.ArrayType.Index] = _indexes.ToArray();

    // _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

    // _root.Mesh = _mesh;

    // Vector3[] vertices = gameObject.GetComponent<MeshFilter>().mesh.vertices;
    // List<Vector3> vertList = new List<Vector3>();
    // Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
    // int index = 0;

    float radius = 5f;
    var middlePointIndexCache = new Dictionary<long, int>();

    // create 12 vertices of a icosahedron
    _points = buildIcoPoints();

    drawPoints();

    // create 20 triangles of the icosahedron
    _faces = buildFaces();

    connectPoints(new Dictionary<int, int[]>{
      { 0, new int[] { 1, 5, 7, 10, 11 } },
      { 1, new int[] { 5, 7, 8, 9 } },
      { 2, new int[] { 3, 4, 6, 10, 11 } },
      { 3, new int[] { 4, 6, 8, 9 } },
      { 4, new int[] { 5, 9, 11 } },
      { 5, new int[] { 9, 11 } },
      { 6, new int[] { 7, 8, 10 } },
      { 7, new int[] { 8, 10 } },
      { 8, new int[] { 9 } },
      { 10, new int[] { 11 } },
    });

    divideFaces(levelRecursion);

    if (!AuditPoints(_points)) GD.Print("INVALID");
    else GD.Print("VALID POINTS");

    GD.Print("POINTS: " + _points.Count);
    GD.Print("Faces: " + _faces.Count);

    _faces.ForEach(face => GD.Print(face.v1 + " - " + face.v2 + " - " + face.v3));

    Vertex node;
    List<Vertex> nodes = new List<Vertex>();

    setHeights(1f);

    _points.ForEach(point => GD.Print(point.Vector.X + "|" + point.Vector.Y + "|" + point.Vector.Z));

    // var data = _points.Select(point => point.Vector).ToArray();
    // for (var n = 0; n < data.Length; n++)
    // {
    //   GD.Print("Point: " + data[n].X + "," + data[n].Y + "," + data[n].Z);
    // }

    _mesh = new ArrayMesh();

    // var d = buildIndexesFromFaces();
    // for (var i = 0; i < d.Length; i++)
    // {
    //   GD.Print(i.ToString() + ": " + d[i]);
    // }

    var arrays = new Godot.Collections.Array();
    arrays.Resize((int)Mesh.ArrayType.Max);
    arrays[(int)Mesh.ArrayType.Vertex] = _points.Select(point => point.Vector).ToArray();
    //arrays[(int)Mesh.ArrayType.TexUV] = _uvs.ToArray();
    arrays[(int)Mesh.ArrayType.Normal] = _points.Select(point => point.Vector.Normalized()).ToArray();
    arrays[(int)Mesh.ArrayType.Index] = buildIndexesFromFaces();

    _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

    GD.Print("Mesh Faces: " + _mesh.GetFaces().Length.ToString());

    var f = _mesh.GetFaces();
    for (var i = 0; i < f.Length; i++)
    {
      GD.Print(f[i].X + ", " + f[i].Y + ", " + f[i].Z);
    }
    _root.Mesh = _mesh;
  }
}
