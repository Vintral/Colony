using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Godot;

public partial class PlanetSquare : Node3D
{
  MeshInstance3D _root;
  Camera _camera;
  List<Vector3> _vertices;
  List<Vector3> _verticesFaces;
  List<Vector2> _uvs;
  List<Vector3> _normals;
  List<int> _indexes;

  private List<Vector3> buildVertices()
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

  private List<Vector3> buildFaces()
  {
    GD.Print("buildFaces");

    var ret = new List<Vector3>
    {
      _vertices[1],
      _vertices[0],
      _vertices[3],
      _vertices[2],
      _vertices[5],
      _vertices[4],
      _vertices[6],
      _vertices[7],
      _vertices[4],
      _vertices[2],
      _vertices[3],
      _vertices[7],
      _vertices[0],
      _vertices[1],
      _vertices[4],
      _vertices[5],
      _vertices[6],
      _vertices[7],
      _vertices[2],
      _vertices[3]
    };

    return ret;
  }

  private List<Vector3> buildNormals()
  {
    GD.Print("buildNormals");

    var ret = new List<Vector3>();

    foreach (var v in _vertices)
    {
      ret.Add(v.Normalized());
    }

    return ret;
  }

  private List<Vector2> buildUVs()
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

  private int[] addQuad(int a, int b, int c, int d)
  {
    return new int[] { a, b, c, a, c, d };
  }

  private List<int> buildIndexes()
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

  public override void _Ready()
  {
    Position = new Vector3(5, 0, 0);

    _root = GetNode<MeshInstance3D>("Root");
    if (_root == null)
    {
      GD.PrintErr("Root not found");
    }

    _camera = GetNode<Camera>("Camera");
    // _camera.Planet = _root;

    _vertices = buildVertices();
    _uvs = buildUVs();
    _indexes = buildIndexes();
    _normals = buildNormals();

    var mesh = new ArrayMesh();

    var arrays = new Godot.Collections.Array();
    arrays.Resize((int)Mesh.ArrayType.Max);
    arrays[(int)Mesh.ArrayType.Vertex] = _vertices.ToArray();
    //arrays[(int)Mesh.ArrayType.TexUV] = _uvs.ToArray();
    arrays[(int)Mesh.ArrayType.Normal] = _normals.ToArray();
    arrays[(int)Mesh.ArrayType.Index] = _indexes.ToArray();

    mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

    _root.Mesh = mesh;
  }
}
