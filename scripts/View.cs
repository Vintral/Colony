using Godot;
using System;

public partial class View : GameNode
{	
  private Vector3 cameraPosition;
  private Vector3 cameraRotation;

	public override void _Ready()
	{
    _debug = true;
    base._Ready();

    cameraRotation = RotationDegrees;
	}

	public override void _Process(double delta)
	{
    base._Process(delta);

    Position = Position.Lerp(cameraPosition, (float)(delta * 8));
    RotationDegrees = RotationDegrees.Lerp(cameraRotation, (float)(delta * 6));

    HandleInput(delta);
	}

  public override void _Input(InputEvent @event)
  {
    base._Input(@event);

    if( @event is InputEventMouseMotion ) {
      if( Input.IsActionPressed("camera_rotate") ) {
        Debug( "Rotate" );
        
        cameraRotation += new Vector3( 0, -( @event as InputEventMouseMotion ).Relative.X / 10, 0 );
      }
    }
  }

  private void HandleInput(double delta)
  {
    Debug("HandleInput:" + delta, false, true);

    var input = Vector3.Zero;

    input.X = Input.GetAxis("camera_left", "camera_right");
    input.Z = Input.GetAxis("camera_forward", "camera_back");

    input = input.Rotated(Vector3.Up, Rotation.Y).Normalized();

    cameraPosition += input / 4;

    if(Input.IsActionPressed("camera_center"))
    {
      cameraPosition = Vector3.Zero;
    }
  }
}
