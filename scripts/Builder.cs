using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Builder : GameNode
{
  [Export]
  public Godot.Collections.Array<Structure> Structures { get; set; } = new Godot.Collections.Array<Structure>();

  [Export]
  public Node3D Selector { get; set; }

  [Export]
  public Node3D SelectorContainer { get; set; }

  [Export]
  public Camera3D ViewCamera { get; set; }

  [Export]
  public GridMap Gridmap { get; set; }

  [Export]
  public Label CashDisplay { get; set; }

  private Plane _plane;
  private DataMap _map;
  private int _index = 0;


  public override void _Ready()
  {
    // _debug = true;
    base._Ready();

    _map = new DataMap();
    _plane = new Plane(Vector3.Up, Vector3.Zero);

    var meshLibrary = new MeshLibrary();

    foreach (var structure in Structures)
    {
      var id = meshLibrary.GetLastUnusedItemId();

      meshLibrary?.CreateItem(id);
      meshLibrary?.SetItemMesh(id, GetMesh(structure.Model));
      meshLibrary?.SetItemMeshTransform(id, new Transform3D());
    }

    Gridmap.MeshLibrary = meshLibrary;

    updateStructure();
    updateCash();
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    base._Process(delta);

    // Controls
    ActionRotate();
    ActionStructureToggle();

    ActionSave();
    ActionLoad();

    var vector = _plane.IntersectsRay(
      ViewCamera.ProjectRayOrigin(GetViewport().GetMousePosition()),
      ViewCamera.ProjectRayNormal(GetViewport().GetMousePosition())
    ) ?? Vector3.Zero;

    var worldPosition = new Vector3I(
      (int)vector.X,
      (int)vector.Y,
      (int)vector.Z
    );

    var gridmapPosition = new Vector3I((int)Mathf.Round(worldPosition.X), 0, (int)Mathf.Round(worldPosition.Z));
    Selector.Position = Lerp(Selector.Position, gridmapPosition, (float)(delta * 40));

    ActionBuild(gridmapPosition);
    ActionDemolish(gridmapPosition);

    if (Input.IsActionJustPressed("ping"))
    {
      Debug("Mouse Click");
      Debug(GetViewport().GetMousePosition().ToString());
    }
  }

  private Mesh GetMesh(PackedScene scene)
  {
    Debug("GetMesh");

    if (scene == null) return null;

    SceneState sceneState = scene.GetState();
    for (int i = 0; i < sceneState.GetNodeCount(); i++)
    {
      if (sceneState.GetNodeType(i) == "MeshInstance3D")
      {
        for (int j = 0; j < sceneState.GetNodePropertyCount(i); j++)
        {
          var propName = sceneState.GetNodePropertyName(i, j);
          if (propName == "mesh")
          {
            var propValue = sceneState.GetNodePropertyValue(i, j);
            return (Mesh)propValue.Obj;
          }
        }
      }
    }

    return null;
  }

  private void updateStructure()
  {
    Debug("updateStructure");

    foreach (var child in SelectorContainer.GetChildren())
    {
      SelectorContainer.RemoveChild(child);
    }

    var _model = Structures[_index].Model.Instantiate<Node3D>();
    SelectorContainer.AddChild(_model);

    var position = _model.Position;
    position.Y += 0.25f;
    _model.Position = position;
  }

  private void updateCash()
  {
    Debug("updateCash: $" + _map.Cash.ToString());

    CashDisplay.Text = "$" + _map.Cash.ToString();
  }

  private Vector3 Lerp(Vector3 from, Vector3 to, float weight)
  {
    return new Vector3(
      Mathf.Lerp(from.X, to.X, weight),
      Mathf.Lerp(from.Y, to.Y, weight),
      Mathf.Lerp(from.Z, to.Z, weight)
    );
  }

  private void ActionBuild(Vector3I position)
  {
    if (Input.IsActionJustPressed("build"))
    {
      Debug("ActionBuild");

      var previousTile = Gridmap.GetCellItem(position);
      Gridmap.SetCellItem(position, _index, Gridmap.GetOrthogonalIndexFromBasis(Selector.Basis));

      if (previousTile != _index)
      {
        _map.Cash -= Structures[_index].Price;
        updateCash();
      }
    }
  }

  private void ActionDemolish(Vector3I position)
  {
    if (Input.IsActionJustPressed("demolish"))
    {
      Debug("ActionDemolish");

      Gridmap.SetCellItem(position, -1);
    }
  }

  private void ActionRotate()
  {
    if (Input.IsActionJustPressed("rotate"))
    {
      Debug("ActionRotate");

      Selector.RotateY(Mathf.DegToRad(90f));
    }
  }

  private void ActionStructureToggle()
  {
    if (Input.IsActionJustPressed("structure_next"))
    {
      Debug("ActionStructureToggle: Next");
      _index = Mathf.PosMod(_index + 1, Structures.Count);
    }

    if (Input.IsActionJustPressed("structure_previous"))
    {
      Debug("ActionStructureToggle: Previous");
      _index = Mathf.PosMod(_index - 1, Structures.Count);
    }

    if (Input.IsActionJustPressed("structure_next") || Input.IsActionJustPressed("structure_previous"))
    {
      Debug("ActionStructureToggle: " + _index.ToString());
      updateStructure();
    }
  }

  private void ActionSave()
  {
    if (Input.IsActionJustPressed("save"))
    {
      Debug("ActionSave");

      _map.Structures.Clear();
      foreach (var cell in Gridmap.GetUsedCells())
      {
        var dataStructure = new DataStructure
        {
          Position = new Vector2I(cell.X, cell.Y),
          Orientation = Gridmap.GetCellItemOrientation(cell),
          Structure = Gridmap.GetCellItem(cell)
        };

        _map.Structures.Append<DataStructure>(dataStructure);
      }

      ResourceSaver.Save(_map, "user://map.res");
    }
  }

  private void ActionLoad()
  {
    if (Input.IsActionJustPressed("load"))
    {
      Debug("ActionLoad");

      Gridmap.Clear();

      _map = ResourceLoader.Load<DataMap>("user://map.res");
      if (_map == null)
      {
        _map = new DataMap();
      }

      foreach (var cell in _map.Structures)
      {
        Gridmap.SetCellItem(new Vector3I(cell.Position.X, 0, cell.Position.Y), cell.Structure, cell.Orientation);
      }

      updateCash();
    }
  }
}