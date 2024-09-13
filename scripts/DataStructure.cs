using Godot;

public partial class DataStructure : GodotObject
{
  [Export]
  public Vector2I Position { get; set; }

  [Export]
  public int Orientation { get; set; }

  [Export]
  public int Structure { get; set; }
}
