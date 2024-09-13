using Godot;

public partial class GameNode : Node3D
{
  protected bool _debug = false;

	public override void _Ready()
	{
    Debug("_Ready");
	}

	public override void _Process(double delta)
	{
    Debug("_Process", false, true);
	}

  protected void Debug( string msg, bool force = false, bool silence = false ) {
    if( silence ) return;
    if( _debug || force ) {
      GD.Print(this.GetType().Name + ": " + msg);
    }
  }
}
