using System;
using System.ComponentModel;
using Godot;

public partial class Camera : Camera3D
{
  public Node3D Planet;

  [Export]
  public double Distance { get; set; }
  private double _angle = 0f;
  private double _turnSpeed = 5f;
  double _zoomSpeed = 200f;

  [Export]
  double MinDistance = 10;//120;
  [Export]
  double MaxDistance = 20;//175;
  [Export]
  double MaxRotation = 20;//175;
  double _lastDelta = 0;

  double _angleX = 0f;
  double _angleY = 0f;

  System.Timers.Timer timer = new System.Timers.Timer(1000);

  public override void _Ready()
  {
    setPosition();
    // LookAt(Vector3.Zero);

    Distance = 4;

    timer.Elapsed += onTick;
    timer.Start();
  }

  public override void _ExitTree()
  {
    base._ExitTree();

    timer.Stop();
    timer.Dispose();
  }

  void setPosition()
  {
    Rotation = new Vector3((float)Math.Pow(Mathf.DegToRad((MaxDistance - Distance) / (MaxDistance - MinDistance)), 1.1) * 1.5f * (float)MaxRotation, 0, 0);
    Position = new Vector3(0, 0, (float)Distance);
  }

  void onTick(object sender, EventArgs e)
  {
    // GD.Print("onTick");

    // GD.Print(GlobalTransform.Basis.Y);
    // GD.Print(GlobalTransform.Basis.X);
  }

  public override void _Input(InputEvent @event)
  {
    base._Input(@event);

    if (@event is InputEventMouseButton)
    {
      var button = @event as InputEventMouseButton;

      if (button.IsPressed())
      {
        if (button.ButtonIndex == MouseButton.WheelUp)
        {
          Distance = Mathf.Max(MinDistance, Distance - _zoomSpeed * _lastDelta);
        }
        if (button.ButtonIndex == MouseButton.WheelDown)
        {
          Distance = Mathf.Min(MaxDistance, Distance + _zoomSpeed * _lastDelta);
        }

        setPosition();
      }
    }
  }

  public override void _Process(double delta)
  {
    _lastDelta = delta;

    float scalar = (float)(delta * Distance / MaxDistance / 2);
    var changed = false;

    if (Input.IsKeyPressed(Key.A))
    {
      // GD.Print("|||||||||||||||||||||||||||||||||||||");
      // GD.Print(GlobalTransform.Basis.X);
      // GD.Print(GlobalTransform.Basis.Y);
      // GD.Print(GlobalTransform.Basis.Z);
      // GD.Print(GlobalTransform.Basis.Y.Cross(GlobalTransform.Basis.Z));
      // GD.Print(GlobalTransform.Basis.Y.Cross(GlobalTransform.Basis.X));
      // GD.Print(GlobalTransform.Basis.X.Cross(GlobalTransform.Basis.Z));
      // GD.Print("|||||||||||||||||||||||||||||||||||||");
      // Position -= GlobalTransform.Basis.X * scalar * 15;
      Planet?.RotateY((float)(-scalar * _turnSpeed));
      changed = true;
    }
    if (Input.IsKeyPressed(Key.D))
    {
      Planet?.RotateY((float)(scalar * _turnSpeed));
      // Position += GlobalTransform.Basis.X * new Vector3(scalar, scalar, scalar) * 15;
      changed = true;
    }
    if (Input.IsKeyPressed(Key.W))
    {
      Planet?.RotateX((float)(scalar * _turnSpeed));
      // Position += GlobalTransform.Basis.Y * new Vector3(scalar, scalar, scalar) * 15;
      // _angleY += _turnSpeed * delta;
      changed = true;
    }
    if (Input.IsKeyPressed(Key.S))
    {
      Planet?.RotateX((float)(-scalar * _turnSpeed));
      // Position -= GlobalTransform.Basis.Y * new Vector3(scalar, scalar, scalar) * 15;
      // _angleY -= _turnSpeed * delta;
      changed = true;
    }
    if (Input.IsKeyPressed(Key.Space))
    {
      Planet.Rotation = Vector3.Zero;
      changed = true;
    }

    if (changed)
    {
      // Position = Position.Normalized() * new Vector3((float)_distance, (float)_distance, (float)_distance);
      // GD.Print(Position);
      // var radX = (float)Mathf.DegToRad(_angleX);
      // var radY = (float)Mathf.DegToRad(_angleY);
      // var pos = (new Vector3(Mathf.Sin(radX), 0, Mathf.Cos(radX)) + new Vector3(0, Mathf.Sin(radY), Mathf.Cos(radY))).Normalized() * (float)_distance;
      // Position = pos;

      // try
      // {
      //   LookAt(new Vector3(0, 0, 0));
      // }
      // catch { }
    }
  }

  public void Test()
  {
    GD.Print("WORKED");
  }
}
