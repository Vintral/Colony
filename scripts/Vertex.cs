using System.Collections.Generic;
using Godot;

public class Vertex
{
  private static int _ID = 0;
  private bool _debug = true;

  public int ID { get; set; }

  public List<Vertex> Points;

  public Tile Tile { get; set; }

  public float X { get => _x; }
  float _x;

  public float Y { get => _y; }
  float _y;

  public float Z { get => _z; }
  float _z;

  public float Height
  {
    get => _height;
    set => _height = value;
  }
  float _height = 1;

  public Vector3 Vector
  {
    get
    {
      // return new Vector3(X, Y, Z) * _height;
      return new Vector3(X, Y, Z).Normalized() * _height;
    }
    set
    {
      _x = value.X;
      _y = value.Y;
      _z = value.Z;
    }
  }

  public Vector3 Normal
  {
    get => Vector.Normalized();
  }

  public bool Found;

  public Vertex Point1
  {
    get { return GetPoint(1); }
    set { SetPoint(value, 1); }
  }

  public Vertex Point2
  {
    get { return GetPoint(2); }
    set { SetPoint(value, 2); }
  }

  public Vertex Point3
  {
    get { return GetPoint(3); }
    set { SetPoint(value, 3); }
  }

  public Vertex Point4
  {
    get { return GetPoint(4); }
    set { SetPoint(value, 4); }
  }

  public Vertex Point5
  {
    get { return GetPoint(5); }
    set { SetPoint(value, 5); }
  }

  public Vertex Point6
  {
    get { return GetPoint(6); }
    set { SetPoint(value, 6); }
  }


  public Vertex(float x, float y, float z)
  {
    ID = _ID++;

    _x = x;
    _y = y;
    _z = z;

    Height = 1;

    // var height = Perlin.Noise(x, y, z);
    // _height = height == 0 ? 0 : height > 0 ? 1 : -1;

    Points = new List<Vertex>();
  }

  private Vertex GetPoint(int index)
  {
    if (index <= Points.Count) return Points[index - 1];
    return null;
  }

  private bool SetPoint(Vertex v, int index)
  {
    if (index > Points.Count) return false;

    Points[index - 1] = v;
    v.AddPoint(this);
    return true;
  }

  public bool AddPoint(Vertex v)
  {
    if (Points.FindIndex(vertex => vertex.X.CompareTo(v.X) == 0 && vertex.Y.CompareTo(v.Y) == 0 && vertex.Z.CompareTo(v.Z) == 0) != -1) return false;

    Points.Add(v);
    v.AddPoint(this);

    return true;
  }

  public bool SwapPoint(Vertex v, Vertex r)
  {

    // debug("===============Swap Point===============");
    // debug(this.ID + " ||| " + v.ID + " ||| " + r.ID);
    // v.Dump();
    // r.Dump();
    // Points.ForEach(point => point.Dump());
    // GD.Print("========================================");
    var index = Points.FindIndex(vertex => vertex.X.CompareTo(v.X) == 0 && vertex.Y.CompareTo(v.Y) == 0 && vertex.Z.CompareTo(v.Z) == 0);
    if (index == -1)
    {
      return false;
    }

    var node = Points[index];
    Points[index] = r;

    node.SwapPoint(this, r);
    r.AddPoint(v);

    return true;
  }

  public string Dump()
  {
    debug("x: " + X + ", y: " + Y + ", z: " + Z);
    return "ID: " + ID + " --- x: " + X + ", y: " + Y + ", z: " + Z;
  }

  private void debug(string msg, bool force = false, bool silence = false)
  {
    if (silence) return;
    if (_debug || force) GD.Print("IcoPoint(" + ID + "): " + msg);
  }
}