using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Godot;

public partial class Tile
{
  static int _ID = 0;
  public int ID = _ID++;

  public Side Side;
  public Tile Left
  {
    get => _left;
    set => _left = value;
  }
  Tile _left;
  public Tile Right
  {
    get => _right;
    set => _right = value;
  }
  Tile _right;

  public Tile Up
  {
    get => _up;
    set => _up = value;
  }
  Tile _up;

  public Tile Down
  {
    get => _down;
    set => _down = value;
  }
  Tile _down;

  public int[] Indexes = new int[4];

  public List<Vertex> Vertices
  {
    get => _vertices;
  }
  List<Vertex> _vertices = new();

  public void AddVertex(Vertex vertex)
  {
    _vertices.Add(vertex);
  }

  public Tile(Vertex[] points, params int[] indexes)
  {
    // GD.Print("Tile: " + points.Count + " - " + indexes.Join("|"));

    for (var i = 0; i < indexes.Length; i++)
    {
      try
      {
        Indexes[i] = indexes[i];
        _vertices.Add(points[indexes[i]]);
      }
      catch
      {
        GD.PrintErr("I: " + i);
      }
    }
  }

  public void CreateSlopes(float baseline, float landHeight)
  {
    var threshold = 0.0001f;

    _vertices.ForEach(v =>
    {
      if (v.Height > baseline - threshold && v.Height < baseline + threshold)
      {
        v.Height = baseline + landHeight / 2;
      }
    });
  }

  public void Dump()
  {
    var vertexes = "";
    _vertices.ForEach((v) =>
    {
      vertexes += "(" + v.X + "," + v.Y + "," + v.Z + ")   ";
    });

    GD.Print("==========================");
    GD.Print("ID: " + ID);
    GD.Print("Side: " + Side);
    GD.Print("Indexes: " + Indexes.Join(","));
    GD.Print("Vertexes: " + vertexes);
    GD.Print("Up: " + (Up?.ID ?? -1));
    GD.Print("Right: " + (Right?.ID ?? -1));
    GD.Print("Down: " + (Down?.ID ?? -1));
    GD.Print("Left: " + (Left?.ID ?? -1));
    GD.Print("==========================");
  }

  public bool Adjacent(Tile tile)
  {
    int matching = 0;
    for (var i = 0; i < tile.Vertices.Count; i++)
    {
      if (_vertices.IndexOf(tile.Vertices[i]) != -1)
      {
        matching++;
      }
    }

    GD.Print("=================================");
    var output = "";
    _vertices.ForEach((v) =>
    {
      output += "(" + v.X + "," + v.Y + "," + v.Z + ")   ";
    });
    GD.Print(output);
    output = "";
    tile.Vertices.ForEach((v) =>
    {
      output += "(" + v.X + "," + v.Y + "," + v.Z + ")   ";
    });
    GD.Print(output);
    GD.Print("=================================");

    return matching == 2;
  }
}
