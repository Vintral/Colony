using System;
using Godot;

public partial class main : Node3D
{
  [Export]
  public Planet Planet { get; set; }



  public override void _Ready()
  {
    // var planet = GD.Load<PackedScene>("res://planet.tscn");
    // var instance = planet.Instantiate<planet>();
    // instance.Position = new Vector3(3, 0, 0);
    // instance.Scale = new Vector3(0.5f, 0.5f, 0.5f);

    // instance.SetColor();
    // AddChild(instance);

    // instance = planet.Instantiate<planet>();
    // AddChild(instance);

    // GD.Print("Test: " + instance.GetType().Name);
    //instance = planet.Instantiate();


    // GD.Print("Loading Planet");
    // var planet = GD.Load<PackedScene>("res://planet_new.tscn");

    // GD.Print("Instantiating Planet");
    // var instance = planet.Instantiate<PlanetNew>();

    // GD.Print("Setting Position");
    // instance.Position = new Vector3(3, 0, 0);
    // instance.Scale = new Vector3(3, 3, 3);
    // AddChild(instance);

    // Planet.Create(2, 0);

    // var rd = RenderingServer.CreateLocalRenderingDevice();
    // var shaderFile = GD.Load<RDShaderFile>("res://shaders/compute_example.glsl");
    // var shaderBytecode = shaderFile.GetSpirV();
    // var shader = rd.ShaderCreateFromSpirV(shaderBytecode);

    // // Prepare our data. We use floats in the shader, so we need 32 bit.
    // var input = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    // var inputBytes = new byte[input.Length * sizeof(float)];
    // Buffer.BlockCopy(input, 0, inputBytes, 0, inputBytes.Length);

    // // Create a storage buffer that can hold our float values.
    // // Each float has 4 bytes (32 bit) so 10 x 4 = 40 bytes
    // var buffer = rd.StorageBufferCreate((uint)inputBytes.Length, inputBytes);

    // // Create a uniform to assign the buffer to the rendering device
    // var uniform = new RDUniform
    // {
    //   UniformType = RenderingDevice.UniformType.StorageBuffer,
    //   Binding = 0
    // };
    // uniform.AddId(buffer);
    // var uniformSet = rd.UniformSetCreate(new Array<RDUniform> { uniform }, shader, 0);
  }

  public void MouseEntered()
  {
    GD.Print("Mouse Entered");
  }

  public void MouseExited()
  {
    GD.Print("Mouse Exited");
  }
}
