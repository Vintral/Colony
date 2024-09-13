using Godot;

public partial class DebugLabel : Label3D
{
  public override void _Process(double delta)
  {
    LookAt(GetViewport().GetCamera3D().Position, null, true);
  }
}
