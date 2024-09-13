using Godot;
using System;

public partial class FPS : Label
{
  double accumulator;

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    accumulator += delta;
    if (accumulator > 1.0)
    {
      accumulator -= 1.0;

      Text = "FPS: " + Engine.GetFramesPerSecond().ToString();
    }
  }
}
