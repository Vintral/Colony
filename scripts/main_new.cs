using Godot;
using System;

public partial class main_new : Node3D
{
  Camera _camera;
  Planet _planet;

  public override void _Ready()
  {
    _planet = GetNode<Planet>("Planet");
    if (_planet == null)
    {
      GD.PrintErr("No Planet Found");
    }

    _camera = GetNode<Camera>("Camera");
    if (_camera == null)
    {
      GD.PrintErr("No Camera Found");
    }
    else
    {
      if (_planet != null)
      {
        _camera.Planet = _planet;
      }
    }
  }
}
