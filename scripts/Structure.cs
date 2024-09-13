using Godot;

[GlobalClass]
public partial class Structure : Resource {
  [ExportSubgroup("Model")]
  [Export]
  public PackedScene Model { get; set; }

  [ExportSubgroup("Gameplay")]
  [Export]
  public int Price { get; set; }

  public Structure() : this(0,null) {}

  public Structure(int price, PackedScene model)
  {
    Model = model;
    Price = price;
  }
}
