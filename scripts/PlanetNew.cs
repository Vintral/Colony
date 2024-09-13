using System;
using System.Collections.Generic;
using Godot;

[Tool]
public partial class PlanetNew : Node3D
{
  int _meshesGenerated = 0;
  int _tilesGenerated = 0;
  int _neighborsSet = 0;

  [Export]
  public Resource PlanetData
  {
    get => _planetData;
    set
    {
      if (_planetData != null)
      {
        GD.Print("Disconnecting Listener(2)");
        _planetData.Changed -= onDataChanged;
      }

      _planetData = value;
      if (_planetData != null)
      {
        _planetData.Changed += onDataChanged;
      }

      onDataChanged();
    }
  }
  Resource _planetData;

  public override void _Ready()
  {
    foreach (var child in GetChildren())
    {
      var face = child as PlanetMeshFace;
      if (face != null)
      {
        face.MeshGenerated += onMeshGenerated;
        face.TilesGenerated += onTilesGenerated;
        face.NeighborsSet += onNeighborsSet;
      }
    }
    Generate();
  }

  void onNeighborsSet()
  {
    GD.Print("onNeighborsSet");

    _neighborsSet++;
    if (_neighborsSet == 1)
    {
      GD.Print("ALL NEIGHBORS SET");

      List<Vector3> vertexes = new List<Vector3>();
      foreach (var child in GetChildren())
      {
        var tiles = (child as PlanetMeshFace)?.Tiles ?? null;
        if (tiles != null)
        {
          var faces = (_planetData as PlanetData).Resolution - 1;
          for (var x = 0; x < faces; x++)
          {
            for (var y = 0; y < faces; y++)
            {
              var tile = tiles[x, y];
              tile.Vertices.ForEach(v =>
              {
                //if (vertexes.IndexOf(v) == -1)
                //vertexes.Add(v);
              });
            }
          }
        }

        vertexes.Add(Vector3.Inf);
      }

      // GD.Print("====================================");
      // vertexes.ForEach(v =>
      // {
      //   GD.Print("(" + v.X + "," + v.Y + "," + v.Z + ")");
      // });
      // GD.Print("====================================");
    }
  }

  void onMeshGenerated()
  {
    // GD.Print("onMeshGenerated");

    _meshesGenerated++;
    if (_meshesGenerated == 6)
    {
      foreach (var child in GetChildren())
      {
        (child as PlanetMeshFace)?.CreateTiles();
      }
    }
  }

  void onTilesGenerated()
  {
    GD.Print("onTilesGenerated");

    _tilesGenerated++;
    if (_tilesGenerated == 6)
    {
      bool first = true;
      foreach (var child in GetChildren())
      {
        if (first)
        {
          first = false;
          (child as PlanetMeshFace)?.SetNeighbors();
        }
      }
    }
  }

  void doComputeShaders()
  {
    GD.Print("doComputeShaders");
    var before = Time.GetTicksMsec();
    foreach (var child in GetChildren())
    {
      var face = child as PlanetMeshFace;
      if (face != null)
      {
        face?.RegenerateMeshUsingComputeShader(PlanetData as PlanetData);
      }
    }
    GD.Print("Compute Shader Mesh took: " + (Time.GetTicksMsec() - before));
  }

  public void Generate()
  {
    var data = PlanetData as PlanetData;
    if (data == null) return;

    data.minHeight = 99999.0f;
    data.maxHeight = 0.0f;

    var before = Time.GetTicksMsec();
    foreach (var child in GetChildren())
    {
      var face = child as PlanetMeshFace;
      if (face != null)
      {
        face?.RegenerateMesh(PlanetData as PlanetData);
      }
    }
    GD.Print("Mesh took: " + (Time.GetTicksMsec() - before));

    doComputeShaders();
  }

  void onDataChanged()
  {
    GD.Print("onDataChanged");
    if (Engine.IsEditorHint())
    {
      Generate();
    }
  }
}
