using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Godot;

public struct QuadIndices
{
  public int v1;
  public int v2;
  public int v3;
  public int v4;

  public QuadIndices(int v1, int v2, int v3, int v4)
  {
    this.v1 = v1;
    this.v2 = v2;
    this.v3 = v3;
    this.v4 = v4;
  }
}

struct FaceDefinition
{
  public Side Side;
  public Vector3 TopLeft;
  public Vector3 BottomRight;

  public FaceDefinition(Side side, Vector3 topLeft, Vector3 bottomRight)
  {
    Side = side;
    TopLeft = topLeft;
    BottomRight = bottomRight;
  }
}

public enum Side
{
  Front = 0, Left = 1, Back = 2, Right = 3, Up = 4, Down = 5
}

[Tool]
public partial class Planet : Node3D
{
  Side[] _sides = new Side[] { Side.Front, Side.Left, Side.Back, Side.Right, Side.Up, Side.Down };
  MeshInstance3D _root;
  Camera _camera;
  List<Vector3> _vertices;
  // List<QuadIndices> _faces;
  Tile[] _tiles;
  List<Vector2> _uvs;
  List<Vector2> _uvs2;
  List<Vector3> _normals;
  List<int> _indexes;
  ArrayMesh _mesh;
  List<Vertex> _points = new();

  Vertex[] _pointsArray;
  Godot.Collections.Array _arrays;
  float _minHeight;
  float _maxHeight;
  float _landHeight = .1f;
  static readonly object _pointLock = new object();

  List<FaceDefinition> _faceDefinitions = new() {
    new FaceDefinition(Side.Front, new Vector3(-1, 1, 1), new Vector3(1, -1, 1)),
    new FaceDefinition(Side.Left, new Vector3(-1, 1, -1), new Vector3(-1, -1, 1)),
    new FaceDefinition(Side.Back, new Vector3(1, 1, -1), new Vector3(-1, -1, -1)),
    new FaceDefinition(Side.Right, new Vector3(1, 1, 1), new Vector3(1, -1, -1)),
    new FaceDefinition(Side.Up, new Vector3(-1, 1, -1), new Vector3(1, 1, 1)),
    new FaceDefinition(Side.Down, new Vector3(-1, -1, 1), new Vector3(1, -1, -1)),
  };

  Collider _collider;

  [Export]
  public GradientTexture1D PlanetColor
  {
    get => _planetColor;
    set
    {
      if (_planetColor != null)
      {
        _planetColor.Changed -= onPlanetColorChanged;
      }

      _planetColor = value;
      if (_planetColor != null)
      {
        _planetColor.Changed += onPlanetColorChanged;
      }

      onDataChanged();
    }
  }
  GradientTexture1D _planetColor;

  // Dictionary <Side, 

  [Export]
  public float Radius
  {
    get => _radius;
    set
    {
      _radius = value;
      onDataChanged();
    }
  }
  float _radius = 1;

  [Export]
  public int Size
  {
    get => _size;
    set
    {
      _size = value;
      onDataChanged();
    }
  }
  int _size = 1;

  [Export]
  public PlanetNoise[] Noise
  {
    get => _noise;
    set
    {
      if (value != null)
      {
        foreach (var noise in value)
        {
          // if (noise != null)
          //   noise.Changed += onNoiseChanged;
        }

        _noise = value;
      }
    }
  }
  PlanetNoise[] _noise;
  // List<PlanetNoise> _noise;
  // PlanetNoise[] _noise;

  void onNoiseChanged()
  {
    GD.Print("onNoiseChanged");
    if (Engine.IsEditorHint())
    {
      Create();
    }
  }

  void onPlanetColorChanged()
  {
    // GD.Print("onPlanetColorChanged");
    if (Engine.IsEditorHint())
    {
      Create();
    }
  }

  public override void _Ready()
  {
    Position = new Vector3(0, 0, 0);

    _root = GetNode<MeshInstance3D>("Root");
    if (_root == null)
    {
      GD.PrintErr("Root is null");
    }

    _collider = GetNode<Collider>("Collider");
    if (_collider == null)
    {
      GD.PrintErr("Missing Collider");
    }

    Create();
  }

  void onDataChanged()
  {
    // GD.Print("onDataChanged");
    if (Engine.IsEditorHint())
    {
      Create();
    }
  }

  int getMiddlePoint(params int[] points)// ref Dictionary<long, int> cache )
  {
    // first check if we have it already
    // bool firstIsSmaller = p1 < p2;
    // long smallerIndex = firstIsSmaller ? p1 : p2;
    // long greaterIndex = firstIsSmaller ? p2 : p1;
    // long key = (smallerIndex << 32) + greaterIndex;

    // not in cache, calculate it
    // Vector3 point1 = _points[p1].Vector;
    // Vector3 point2 = _points[p2].Vector;
    // Vector3 middle = new(
    //     (point1.X + point2.X) / 2f,
    //     (point1.Y + point2.Y) / 2f,
    //     (point1.Z + point2.Z) / 2f
    // );

    Vector3 middle = Vector3.Zero;
    points.ToList().ForEach(p =>
    {
      middle += _points[p].Vector;
    });
    middle /= points.Length;

    // add vertex makes sure point is on unit sphere
    int i = _points.Count;
    var position = middle.Normalized();

    Vertex v = new(position.X, position.Y, position.Z);
    return addPoint(v);
  }

  int FindIndex(float x, float y, float z, bool shouldLock = false)
  {
    if (_points == null)
    {
      GD.PrintErr("POINTS IS NULL");
      return -1;
    }

    if (shouldLock)
    {
      lock (_pointLock)
      {
        return _points.FindIndex(vertex =>
        {
          if (vertex == null)
          {
            throw new Exception("Vertex is null");
          }
          return vertex.X.CompareTo(x) == 0 && vertex.Y.CompareTo(y) == 0 && vertex.Z.CompareTo(z) == 0;
        });
      }
    }
    else
    {
      return _points.FindIndex(vertex =>
        {
          if (vertex == null)
          {
            throw new Exception("Vertex is null");
          }
          return vertex.X.CompareTo(x) == 0 && vertex.Y.CompareTo(y) == 0 && vertex.Z.CompareTo(z) == 0;
        });
    }
  }

  int addPoint(Vertex point, bool shouldLock = false, bool retry = true)
  {
    try
    {
      var index = FindIndex(point.X, point.Y, point.Z, shouldLock);
      if (index != -1) return index;

      _points.Add(point);
      point.ID = _points.Count - 1;
      return point.ID;
    }
    catch (Exception ex)
    {
      GD.PrintErr(ex.Message);
      if (retry)
      {
        return addPoint(point, false);
      }
    }

    return -1;
  }

  async Task buildPoints()
  {
    GD.Print("buildPoints");

    _points = new List<Vertex>();

    // List<Task> tasks = new();
    // _faceDefinitions.ForEach(face => tasks.Add(Task.Run(() => buildSideEdges(face.TopLeft, face.BottomRight))));
    // await Task.WhenAll(tasks);

    // tasks = new();
    // _faceDefinitions.ForEach(face => tasks.Add(Task.Run(() => buildSideMiddles(face.TopLeft, face.BottomRight))));
    // await Task.WhenAll(tasks);

    List<Task> tasks = new();
    // _faceDefinitions.ForEach(face => tasks.Add(Task.Run(() => buildSideMiddles(face.TopLeft, face.BottomRight))));
    _faceDefinitions.ForEach(face => buildSide(face.TopLeft, face.BottomRight, face.Side));
    await Task.WhenAll(tasks);

    // _points.ForEach(p => p.Dump());

    // return Task.CompletedTask;
  }

  int getSize()
  {
    // if (Engine.IsEditorHint())
    // {
    //   return Mathf.Min(_size, 15);
    // }


    return _size;
  }

  void buildSide(Vector3 TopLeft, Vector3 BottomRight, Side side)
  {
    GD.Print("buildSide: Started");

    var offset = (int)side * _pointsArray.Length / _faceDefinitions.Count;
    var delta = (BottomRight - TopLeft) / getSize();
    for (var n = 0; n <= getSize(); n++)
    {
      for (var i = 0; i <= getSize(); i++)
      {
        Vertex vertex;
        if (delta.X == 0)
        {
          // addPoint(new Vertex(TopLeft.X, TopLeft.Y + delta.Y * i, TopLeft.Z + delta.Z * n));
          vertex = new Vertex(TopLeft.X, TopLeft.Y + delta.Y * n, TopLeft.Z + delta.Z * i);
        }
        else if (delta.Y == 0)
        {
          // addPoint(new Vertex(TopLeft.X + delta.X * n, TopLeft.Y, TopLeft.Z + delta.Z * i));
          vertex = new Vertex(TopLeft.X + delta.X * i, TopLeft.Y, TopLeft.Z + delta.Z * n);
        }
        else
        {
          // addPoint(new Vertex(TopLeft.X + delta.X * n, TopLeft.Y + delta.Y * i, TopLeft.Z));
          vertex = new Vertex(TopLeft.X + delta.X * i, TopLeft.Y + delta.Y * n, TopLeft.Z);
        }

        if (vertex != null)
        {
          // GD.Print("Added Vertex: " + (offset + n * (getSize() + 1) + i));
          _pointsArray[offset + n * (getSize() + 1) + i] = vertex;
        }
        else GD.Print("Vertex is null");
      }
    }
    GD.Print("buildSide: Finished");
  }

  void buildSideEdges(Vector3 TopLeft, Vector3 BottomRight)
  {
    GD.Print("buildSideEdges: Started");
    var delta = (BottomRight - TopLeft) / getSize();

    for (var i = 0; i <= getSize(); i++)
    {
      if (delta.X == 0)
      {
        addPoint(new Vertex(TopLeft.X, TopLeft.Y, TopLeft.Z + delta.Z * i), true);
        addPoint(new Vertex(TopLeft.X, BottomRight.Y, TopLeft.Z + delta.Z * i), true);
      }
      else if (delta.Y == 0)
      {
        addPoint(new Vertex(TopLeft.X + delta.X * i, TopLeft.Y, TopLeft.Z), true);
        addPoint(new Vertex(TopLeft.X + delta.X * i, TopLeft.Y, BottomRight.Z), true);
      }
      else
      {
        addPoint(new Vertex(TopLeft.X + delta.X * i, TopLeft.Y, TopLeft.Z), true);
        addPoint(new Vertex(TopLeft.X + delta.X * i, BottomRight.Y, TopLeft.Z), true);
      }
    }

    for (var i = 1; i < getSize(); i++)
    {
      if (delta.X == 0)
      {
        addPoint(new Vertex(TopLeft.X, TopLeft.Y + delta.Y * i, TopLeft.Z), true);
        addPoint(new Vertex(TopLeft.X, TopLeft.Y + delta.Y * i, BottomRight.Z), true);
      }
      else if (delta.Y == 0)
      {
        addPoint(new Vertex(TopLeft.X, TopLeft.Y, TopLeft.Z + delta.Z * i), true);
        addPoint(new Vertex(BottomRight.X, TopLeft.Y, TopLeft.Z + delta.Z * i), true);
      }
      else
      {
        addPoint(new Vertex(TopLeft.X, TopLeft.Y + delta.Y * i, TopLeft.Z), true);
        addPoint(new Vertex(BottomRight.X, TopLeft.Y + delta.Y * i, TopLeft.Z), true);
      }
    }
    GD.Print("buildSideEdges: Finished");
  }

  void buildSideMiddles(Vector3 TopLeft, Vector3 BottomRight)
  {
    GD.Print("buildSideMiddles: Started");
    var delta = (BottomRight - TopLeft) / getSize();

    for (var i = 1; i < getSize(); i++)
    {
      for (var n = 1; n < getSize(); n++)
      {
        // var shouldLock = i == 0 || n == 0 || i == _size || n == _size;

        if (delta.X == 0)
        {
          addPoint(new Vertex(TopLeft.X, TopLeft.Y + delta.Y * i, TopLeft.Z + delta.Z * n));
        }
        else if (delta.Y == 0)
        {
          addPoint(new Vertex(TopLeft.X + delta.X * n, TopLeft.Y, TopLeft.Z + delta.Z * i));
        }
        else
        {
          addPoint(new Vertex(TopLeft.X + delta.X * n, TopLeft.Y + delta.Y * i, TopLeft.Z));
        }
      }
    }
    GD.Print("buildSideMiddles: Finished");
  }

  public int[] buildIndexesFromFaces()
  {
    var ret = new int[_tiles.Length * 3 * 2];

    GD.Print("Faces: " + _tiles.Length);
    _tiles[0].Dump();

    for (var i = 0; i < _tiles.Length; i++)
    {
      try
      {
        ret[6 * i] = _tiles[i].Indexes[0];
        ret[6 * i + 1] = _tiles[i].Indexes[1];
        ret[6 * i + 2] = _tiles[i].Indexes[3];
        ret[6 * i + 3] = _tiles[i].Indexes[0];
        ret[6 * i + 4] = _tiles[i].Indexes[3];
        ret[6 * i + 5] = _tiles[i].Indexes[2];
      }
      catch
      {
        GD.Print("RAN INTO ERROR: " + i);
      }
    }

    return ret;
  }

  void connectPoints()
  {
    var data = new System.Collections.Generic.Dictionary<int, int[]>{
      { 0, new int[] { 1, 2, 4, } },
      { 1, new int[] { 3, 5, } },
      { 2, new int[] { 6, 3, } },
      { 3, new int[] { 7, } },
      { 4, new int[] { 5, 6, } },
      { 5, new int[] { 7, } },
      { 6, new int[] { 7, } },
    };

    foreach (var point in data)
    {
      // GD.Print(point.Key + " -- " + point.Value);

      for (var i = 0; i < point.Value.Length; i++)
      {
        _points[point.Key].AddPoint(_points[point.Value[i]]);
      }
    }
  }

  float[] getVertexArray()
  {
    // GD.Print("getVertexArray");

    var ret = new float[_points.Count];
    for (var i = 0; i < _points.Count; i++)
    {
      ret[3 * i] = _points[i].X;
      ret[3 * i + 1] = _points[i].Y;
      ret[3 * i + 2] = _points[i].Z;
    }

    return ret;
  }

  int findPoint(Vector3 source, Vector3 delta, int a, int b)
  {
    if (delta.X == 0)
    {
      return FindIndex(source.X, source.Y + delta.Y * b, source.Z + delta.Z * a);
    }
    else if (delta.Y == 0)
    {
      return FindIndex(source.X + delta.X * a, source.Y, source.Z + delta.Z * b);
    }
    else
    {
      return FindIndex(source.X + delta.X * a, source.Y + delta.Y * b, source.Z);
    }
  }


  int createPoint(Vector3 source, Vector3 delta, int a, int b)
  {
    if (delta.X == 0)
    {
      return FindIndex(source.X, source.Y + delta.Y * b, source.Z + delta.Z * a);
    }
    else if (delta.Y == 0)
    {
      return FindIndex(source.X + delta.X * a, source.Y, source.Z + delta.Z * b);
    }
    else
    {
      return FindIndex(source.X + delta.X * a, source.Y + delta.Y * b, source.Z);
    }
  }

  Task buildFaceTiles(Vector3 TopLeft, Vector3 BottomRight, Side side)
  {
    GD.Print("buildFaceTiles -- Started");

    var offset = (int)side * _pointsArray.Length / _faceDefinitions.Count;
    var delta = (BottomRight - TopLeft) / getSize();
    for (var i = 0; i < getSize(); i++)
    {
      for (var n = 0; n < getSize(); n++)
      {
        int a, b, c, d;

        // a = findPoint(TopLeft, delta, n, i);
        // b = findPoint(TopLeft, delta, n + 1, i);
        // c = findPoint(TopLeft, delta, n, i + 1);
        // d = findPoint(TopLeft, delta, n + 1, i + 1);

        a = offset + i + n * (getSize() + 1);
        b = offset + i + 1 + n * (getSize() + 1);
        c = offset + i + (n + 1) * (getSize() + 1);
        d = offset + i + 1 + (n + 1) * (getSize() + 1);

        var tile = new Tile(_pointsArray, a, b, c, d)
        {
          Side = side
        };
        _tiles[(int)side * getSize() * getSize() + i + n * getSize()] = tile;
      }
    }

    GD.Print("buildFaceTiles -- Finished");
    return Task.CompletedTask;
  }

  async Task buildTiles()
  {
    // GD.Print("buildFaces");

    // _faces = new List<QuadIndices>();
    _tiles = new Tile[Size * getSize() * _faceDefinitions.Count];


    // await Parallel.ForEachAsync(_faceDefinitions, async( i, token ) => await buildFaceTiles)
    // await Task.WhenAll( _faceDefinitions.ForEach( face => buildFaceTiles( face.TopLeft, face.BottomRight, face.Side )));

    List<Task> tasks = new();
    _faceDefinitions.ForEach(face => tasks.Add(Task.Run(() => buildFaceTiles(face.TopLeft, face.BottomRight, face.Side))));
    await Task.WhenAll(tasks);

    // _faceDefinitions.ForEach(face => buildFaceTiles(face.TopLeft, face.BottomRight, face.Side));
    // return Task.CompletedTask;
  }

  void connectTiles()
  {
    // GD.Print("connectTiles");

    var offsets = new System.Collections.Generic.Dictionary<Side, int>();
    foreach (var side in _sides)
    {
      offsets[side] = (int)side * getSize() * getSize();
    }

    _faceDefinitions.ForEach(face =>
    {
      var offset = (int)face.Side * getSize() * getSize();
      for (var i = 0; i < getSize(); i++)
      {
        for (var n = 0; n < getSize(); n++)
        {
          if (i > 0) _tiles[offset + n + i * getSize()].Up = _tiles[offset + n + (i - 1) * getSize()];
          if (n > 0) _tiles[offset + n + i * getSize()].Left = _tiles[offset + (n - 1) + i * getSize()];
          if (i < getSize() - 1) _tiles[offset + n + i * getSize()].Down = _tiles[offset + n + (i + 1) * getSize()];
          if (n < getSize() - 1) _tiles[offset + n + i * getSize()].Right = _tiles[offset + n + 1 + i * getSize()];

          if (n == 0)
          {
            switch (face.Side)
            {
              case Side.Front:
                {
                  _tiles[offset + i * getSize()].Left = _tiles[offsets[Side.Left] + getSize() - 1 + i * getSize()];
                }
                break;
              case Side.Left:
                {
                  _tiles[offset + i * getSize()].Left = _tiles[offsets[Side.Back] + getSize() - 1 + i * getSize()];
                }
                break;
              case Side.Back:
                {
                  _tiles[offset + i * getSize()].Left = _tiles[offsets[Side.Right] + getSize() - 1 + i * getSize()];
                }
                break;
              case Side.Right:
                {
                  _tiles[offset + i * getSize()].Left = _tiles[offsets[Side.Front] + getSize() - 1 + i * getSize()];
                }
                break;
              case Side.Up:
                {
                  _tiles[offset + i * getSize()].Left = _tiles[offsets[Side.Left] + i];
                }
                break;
              case Side.Down:
                {
                  _tiles[offset + i * getSize()].Left = _tiles[offsets[Side.Left] + (Size * getSize()) - 1 - i];
                }
                break;
            }
          }
          if (i == 0)
          {
            switch (face.Side)
            {
              case Side.Front:
                {
                  _tiles[offset + n].Up = _tiles[offsets[Side.Up] + n + (Size - 1) * getSize()];
                }
                break;
              case Side.Left:
                {
                  _tiles[offset + n].Up = _tiles[offsets[Side.Up] + n * getSize()];
                }
                break;
              case Side.Back:
                {
                  _tiles[offset + n].Up = _tiles[offsets[Side.Up] + getSize() - n - 1];
                }
                break;
              case Side.Right:
                {
                  _tiles[offset + n].Up = _tiles[offsets[Side.Up] + getSize() * (Size - n) - 1];
                }
                break;
              case Side.Up:
                {
                  _tiles[offset + n].Up = _tiles[offsets[Side.Back] + getSize() - 1 - n];
                }
                break;
              case Side.Down:
                {
                  _tiles[offset + n].Up = _tiles[offsets[Side.Front] + (Size - 1) * getSize() + n];
                }
                break;
            }
          }
          if (i == getSize() - 1)
          {
            switch (face.Side)
            {
              case Side.Front:
                {
                  _tiles[offset + (getSize() - 1) * getSize() + n].Down = _tiles[offsets[Side.Down] + n];
                }
                break;
              case Side.Left:
                {
                  _tiles[offset + (getSize() - 1) * getSize() + n].Down = _tiles[offsets[Side.Down] + (getSize() - 1 - n) * getSize()];
                }
                break;
              case Side.Back:
                {
                  _tiles[offset + (getSize() - 1) * getSize() + n].Down = _tiles[offsets[Side.Down] + (getSize() - 1) * getSize() + (getSize() - 1 - n)];
                }
                break;
              case Side.Right:
                {
                  _tiles[offset + (getSize() - 1) * getSize() + n].Down = _tiles[offsets[Side.Down] + getSize() - 1 + n * getSize()];
                }
                break;
              case Side.Up:
                {
                  _tiles[offset + (getSize() - 1) * getSize() + n].Down = _tiles[offsets[Side.Front] + n];
                }
                break;
              case Side.Down:
                {
                  _tiles[offset + (getSize() - 1) * getSize() + n].Down = _tiles[offsets[Side.Back] + (getSize() * getSize()) - 1 - n];
                }
                break;
            }
          }
          if (n == getSize() - 1)
          {
            switch (face.Side)
            {
              case Side.Front:
                {
                  _tiles[offset + getSize() * (i + 1) - 1].Right = _tiles[offsets[Side.Right] + i * getSize()];
                }
                break;
              case Side.Left:
                {
                  _tiles[offset + getSize() * (i + 1) - 1].Right = _tiles[offsets[Side.Front] + i * getSize()];
                }
                break;
              case Side.Back:
                {
                  _tiles[offset + getSize() * (i + 1) - 1].Right = _tiles[offsets[Side.Left] + i * getSize()];
                }
                break;
              case Side.Right:
                {
                  _tiles[offset + getSize() * (i + 1) - 1].Right = _tiles[offsets[Side.Back] + i * getSize()];
                }
                break;
              case Side.Up:
                {
                  _tiles[offset + getSize() * (i + 1) - 1].Right = _tiles[offsets[Side.Right] + getSize() - i - 1];
                }
                break;
              case Side.Down:
                {
                  _tiles[offset + getSize() * (i + 1) - 1].Right = _tiles[offsets[Side.Right] + getSize() * (getSize() - 1) + i];
                }
                break;
            }
          }
        }
      }
    });
  }

  void setHeights(float radius = 3)
  {
    GD.Print("setHeights: " + radius);

    _minHeight = 99999999999;
    _maxHeight = -99999999999;

    //_pointsArray.ToList()?.ForEach(p =>
    foreach (var p in _pointsArray)
    {
      var elevation = 0f;

      if (Noise?.Length > 0)
      {
        var baseElevation = Noise[0].NoiseMap.GetNoise3Dv(p.Normal * 100.0f);
        baseElevation = (baseElevation + 1) / 2.0f * Noise[0].Amplitude;
        baseElevation = Mathf.Max(0f, baseElevation - Noise[0].MinHeight);

        foreach (var noise in Noise)
        {
          var mask = 1.0f;
          if (noise != null)
          {
            if (noise.UseFirstLayerAsMask)
            {
              mask = baseElevation;
            }

            var levelElevation = noise.NoiseMap.GetNoise3Dv(p.Normal * 100);
            levelElevation = (levelElevation + 1) / 2.0f * noise.Amplitude;
            levelElevation = Mathf.Max(0f, levelElevation - noise.MinHeight) * mask;

            elevation += levelElevation;
          }
        };
      }

      // GD.Print("Elevation: " + elevation + " ||| Height: " + (radius * (elevation + 1f)));
      if (elevation > .15f)
      {
        p.Height = radius + _landHeight;
        p?.Tile?.CreateSlopes(radius, _landHeight);
      }
      else p.Height = radius;

      //p.Height = radius + (elevation > .15f ? _landHeight : 0f);
      //p.Height = radius + (elevation > .25f ? .25f : 0f);
      // p.Height = radius * (elevation + 1f);
      //p.Height = radius;

      _minHeight = Math.Min(_minHeight, p.Height);
      _maxHeight = Math.Max(_maxHeight, p.Height);

      // p.Dump();
    };

    // if (_planetNoise.Count == 0) return pointSphere;

    // var elevation = 0f;
    // var baseElevation = _planetNoise[0].NoiseMap.GetNoise3Dv(pointSphere * 100.0f);
    // baseElevation = (baseElevation + 1) / 2.0f * _planetNoise[0].Amplitude;
    // baseElevation = Mathf.Max(0f, baseElevation - _planetNoise[0].MinHeight);

    // foreach (var noise in _planetNoise)
    // {
    //   var mask = 1.0f;
    //   if (noise != null)
    //   {
    //     if (noise.UseFirstLayerAsMask)
    //     {
    //       mask = baseElevation;
    //     }

    //     var levelElevation = noise.NoiseMap.GetNoise3Dv(pointSphere * 100);
    //     levelElevation = (levelElevation + 1) / 2.0f * noise.Amplitude;
    //     levelElevation = Mathf.Max(0f, levelElevation - noise.MinHeight) * mask;

    //     elevation += levelElevation;
    //   }
    // };

    // return pointSphere * Radius * (elevation + 1.0f);
  }

  void buildUVs()
  {
    var count = _pointsArray.Length;

    _uvs = new();
    _uvs2 = new();

    for (var i = 0; i < count; i++)
    {
      _uvs.Add(new Vector2(0, 1));
      _uvs2.Add(new Vector2(0, 1));
    }
  }

  public async void Create()
  {
    GD.Print("Create");
    // GD.Print("Create: " + levelRecursion + " -- " + levelSmoothing);

    var before = Time.GetTicksMsec();

    _pointsArray = new Vertex[(getSize() + 1) * (getSize() + 1) * _faceDefinitions.Count];
    GD.Print("Points Array Length: " + _pointsArray.Length);

    var current = before;
    await buildPoints();
    // _pointsArray.ToList().ForEach(point => point?.Dump());
    GD.Print("Building Points took: " + (Time.GetTicksMsec() - current));

    current = Time.GetTicksMsec();
    await buildTiles();
    GD.Print("Building Tiles took: " + (Time.GetTicksMsec() - current));

    current = Time.GetTicksMsec();
    connectTiles();
    GD.Print("Connecting Tiles took: " + (Time.GetTicksMsec() - current));

    current = Time.GetTicksMsec();
    buildUVs();
    GD.Print("Building UVs took: " + (Time.GetTicksMsec() - current));

    // foreach (var t in _tiles)
    // {
    //   t.Dump();
    // }

    // var indexes = buildIndexesFromFaces();
    // for (var i = 0; i < indexes.Length - 2; i += 3)
    // {
    //   GD.Print(indexes[i] + " | " + indexes[i + 1] + " | " + indexes[i + 2]);
    // }

    current = Time.GetTicksMsec();
    setHeights(_radius);
    GD.Print("Setting Heights took: " + (Time.GetTicksMsec() - current));

    // _faces.ForEach(face => face.Dump());
    // GD.Print("Face Length: " + _tiles.Count);
    // foreach (var tile in _tiles)
    // {
    //   tile.Dump();
    // }

    // connectPoints();
    // divideFaces();    

    // _points.ForEach(p => p.Dump());
    _arrays = new Godot.Collections.Array();
    _arrays.Resize((int)Mesh.ArrayType.Max);

    current = Time.GetTicksMsec();
    GD.Print("Points Array Length: " + _pointsArray.Length);
    // _arrays[(int)Mesh.ArrayType.Vertex] = _points.Select(point => point.Vector).ToArray();
    _arrays[(int)Mesh.ArrayType.Vertex] = _pointsArray.ToList().Select(point => point.Vector).ToArray();
    GD.Print("Getting Points array took: " + (Time.GetTicksMsec() - current));

    current = Time.GetTicksMsec();
    _arrays[(int)Mesh.ArrayType.TexUV] = _uvs.ToArray();
    GD.Print("Getting UVs array took: " + (Time.GetTicksMsec() - current));

    // current = Time.GetTicksMsec();
    // _arrays[(int)Mesh.ArrayType.TexUV2] = _uvs2.ToArray();
    // GD.Print("Getting UVs2 array took: " + (Time.GetTicksMsec() - current));

    current = Time.GetTicksMsec();
    // _arrays[(int)Mesh.ArrayType.Normal] = _points.Select(point => point.Vector.Normalized()).ToArray();    
    _arrays[(int)Mesh.ArrayType.Normal] = _pointsArray.ToList().Select(point => point.Vector.Normalized()).ToArray();
    GD.Print("Getting Normals array took: " + (Time.GetTicksMsec() - current));

    current = Time.GetTicksMsec();
    _arrays[(int)Mesh.ArrayType.Index] = buildIndexesFromFaces();
    GD.Print("Building Indexes took: " + (Time.GetTicksMsec() - current));


    GD.Print("Mesh took: " + (Time.GetTicksMsec() - before));
    CallDeferred("updateMesh");
  }

  void updateMesh()
  {
    GD.Print("updateMesh");
    // if (_points != null)
    //   GD.Print("POINTS: " + _points);
    // _indexes.ForEach(index =>
    // {
    //   if (index >= _points.Count)
    //   {
    //     GD.PrintErr("Shit: " + index);
    //   }
    // });

    // GD.Print("Update Mesh");
    var mesh = new ArrayMesh();
    mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _arrays);

    using var shader = _root.MaterialOverride as ShaderMaterial;
    shader?.SetShaderParameter("min_height", _minHeight);
    shader?.SetShaderParameter("max_height", _maxHeight);
    shader?.SetShaderParameter("height_color", _planetColor);

    shader?.SetShaderParameter("base_height", _radius);
    shader?.SetShaderParameter("land_height", _landHeight);

    shader?.SetShaderParameter("water_color", new Vector3(0, 0, 1));
    shader?.SetShaderParameter("land_color", new Vector3(0, .45f, 0));
    shader?.SetShaderParameter("sand_color", new Vector3(.45f, .45f, 0));

    _root.Mesh = mesh;

    // if (mesh != null)
    // {
    //   foreach (var child in GetChildren())
    //   {
    //     child.Free();
    //   }
    //   CreateTrimeshCollision();
    // }


    // EmitSignal(SignalName.MeshGenerated);

    // _collider?.CreateShape(mesh);
  }
}