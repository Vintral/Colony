using System.Collections.Generic;
using System.Linq;

using Godot;

public partial class Collider : StaticBody3D
{
  bool _active = true;
  Dictionary<MouseButtonMask, bool> _pressed = new Dictionary<MouseButtonMask, bool>();

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
  }

  public override void _PhysicsProcess(double delta)
  {
    base._PhysicsProcess(delta);

    if (_active)
    {
      //GD.Print("Physics Tick");
    }
  }

  int getHitFace(Vector3 hit, Camera3D camera, Mesh mesh, PlanetMeshFace root)
  {
    var vertices = mesh.GetFaces();

    var arrayMesh = new ArrayMesh();
    var arrays = new Godot.Collections.Array();
    arrays.Resize((int)Mesh.ArrayType.Max);

    arrays[(int)Mesh.ArrayType.Vertex] = vertices;
    arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

    var mdt = new MeshDataTool();
    mdt.CreateFromSurface(mesh as ArrayMesh, 0);

    var origin = camera.GlobalTransform.Origin;
    var purpleArrow = hit - origin;

    var i = 0;
    while (i < vertices.Length)
    {
      var faceIndex = i / 3;
      var a = root.ToGlobal(vertices[i]);
      var b = root.ToGlobal(vertices[i + 1]);
      var c = root.ToGlobal(vertices[i + 2]);

      var intersection = Geometry3D.RayIntersectsTriangle(origin, purpleArrow, a, b, c).AsVector3();
      if (intersection != Vector3.Zero)
      {
        var angle = Mathf.RadToDeg(purpleArrow.AngleTo(mdt.GetFaceNormal(faceIndex)));
        if (angle > 90 && angle < 180)
        {
          arrays = mesh.SurfaceGetArrays(0);

          // GD.Print("WE HIT A FACE: " + faceIndex + " of " + (vertices.Length / 3) + " @ [" + vertices[i] + "," + vertices[i + 1] + "," + vertices[i + 2] + "]");

          // GD.Print("I: " + i + " --- " + vertices.Length);

          var v = vertices[i] + vertices[i + 1] + vertices[i + 2];

          for (var n = 0; n < 3; n++)
          {
            // GD.Print("Magnatitude of Vector " + (n + 1) + ": " + vertices[i + n].Length());
          }
          //return faceIndex;

          if (i % 2 == 0)
          {
            // GD.Print("OTHER FACE: " + (faceIndex + 1) + " ||| " + vertices[i + 3] + "," + vertices[i + 4] + "," + vertices[i + 5]);
          }
          else
          {
            // GD.Print("OTHER FACE: " + (faceIndex - 1) + " ||| " + vertices[i - 3] + "," + vertices[i - 2] + "," + vertices[i - 1]);
          }

          // GD.Print("BEFORE: " + vertices.Length);
          var list = vertices.ToList();
          list.RemoveRange(faceIndex * 3, 3);
          vertices = list.ToArray();
          GD.Print("AFTER: " + vertices.Length);

          var indexes = arrays[(int)Mesh.ArrayType.Index].AsInt32Array().ToList();
          if (indexes.Count > 0)
          {
            // GD.Print("indexes.Count: " + indexes.Count + " i = " + i);
            // GD.Print(indexes[i] + " - " + indexes[i + 1] + " - " + indexes[i + 2]);
            // GD.Print(vertices[indexes[i]] + " - " + vertices[indexes[i + 1]] + " - " + vertices[indexes[i + 2]]);
          }

          var faces = root.Resolution - 1;
          var x = faceIndex / 2 / faces;
          var y = faceIndex / 2 % faces;
          GD.Print("Coords @ (" + x + "," + y + ") --- " + "(" + (root.Resolution - 2 - x) + "," + (root.Resolution - 2 - y) + ") --- " + root.Resolution + " ||| " + faceIndex);
          // GD.Print("Indexes: " + arrays[(int)Mesh.ArrayType.Index].AsVector3Array().Length);
          // GD.Print("UVs: " + arrays[(int)Mesh.ArrayType.TexUV2].AsVector2Array().Length);
          // GD.Print("Normals: " + arrays[(int)Mesh.ArrayType.Normal].AsVector3Array().Length);

          var m = new ArrayMesh();
          m.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
          root.Mesh = m;

          // root.Mesh = new SphereMesh();

          return faceIndex;
        }
      }

      i += 3;
    }

    return -1;
  }

  void castRay(Vector2 position)
  {
    // GD.Print("LEFT CLICK");
    var camera = GetViewport().GetCamera3D();
    var from = camera.ProjectRayOrigin(position);
    var to = from + camera.ProjectRayNormal(position) * 1000f;

    GD.Print("From: " + from + " - To: " + to);

    var spaceState = GetWorld3D().DirectSpaceState;

    var query = PhysicsRayQueryParameters3D.Create(from, to);
    var result = spaceState.IntersectRay(query);

    if (result.Count > 0)
    {
      GD.Print("Hit something at: " + result["position"]);
      GD.Print(result);

      var collidedWith = result["collider"].As<StaticBody3D>();// as StaticBody3D;
                                                               //collidedWith.AsGodotObject().GetType().FullName);
      var planet = collidedWith.GetParent<Planet>();
      GD.Print(planet);

      foreach (var child in planet.GetChildren())
      {
        if (child is PlanetMeshFace)
        {
          var center = Vector2.Zero;
          var buildable = (child as PlanetMeshFace).CanBuildAt(result["position"].AsVector3(), camera, 3, out center);
          // var mesh = (child as PlanetMeshFace).Mesh;
          // var face = getHitFace(result["position"].AsVector3(), camera, mesh, child as PlanetMeshFace);
          // if (face != -1)
          // {
          //   // GD.Print("WE HIT FACE: " + face);
          // }
        }
      }
      // var mesh = root?.Mesh;
      // GD.Print(mesh);
      // if (mesh != null)
      // {
      //   // GD.Print(mesh.GetFaces().Length + " faces");
      //   // GD.Print(mesh.GetFaces()[result["face_index"].AsInt32()]);                 
      //   getHitFace(result["position"].AsVector3(), camera, mesh, root);
      // }
    }
    else
    {
      // GD.Print("NO HITS: " + from + " to " + to);
    }
  }

  public override void _Input(InputEvent @event)
  {
    base._Input(@event);

    if (_active)
    {
      var evt = @event as InputEventMouse;
      if (evt != null)
      {
        // GD.Print(evt.ButtonMask);
        if ((evt.ButtonMask & MouseButtonMask.Left) == MouseButtonMask.Left)
        {
          // var before = Time.GetTicksMsec();
          // for (var i = 0; i < 100; i++)
          // {
          //   castRay(evt.Position);
          // }
          // GD.Print("Ray casts took: " + (Time.GetTicksMsec() - before));
          if (!_pressed[MouseButtonMask.Left])
          {
            _pressed[MouseButtonMask.Left] = true;
            castRay(evt.Position);
          }
        }
        else _pressed[MouseButtonMask.Left] = false;

        // if ((evt.ButtonMask & MouseButtonMask.Right) == MouseButtonMask.Right)
        // {
        //   GD.Print("RIGHT CLICK");
        // }
        // GD.Print("Mouse Event!");
        // GD.Print(evt.GlobalPosition + " -- " + evt.Position);
      }
    }
  }

  public override void _MouseEnter()
  {
    base._MouseEnter();

    _active = true;
    GD.Print("Mouse Enter");
  }

  public override void _MouseExit()
  {
    base._MouseExit();

    _active = true;
    GD.Print("Mouse Left");
  }

  public void CreateShape(ArrayMesh mesh)
  {
    // if (mesh != null)
    // {
    //   foreach (var child in GetChildren())
    //   {
    //     child.Free();
    //   }
    //   CreateTrimeshCollision();
    // }

    GD.Print("CreateShape");
    mesh.CreateTrimeshShape();

    var definition = mesh.CreateTrimeshShape();

    var shape = GetNode<CollisionShape3D>("Shape");
    if (shape == null)
    {
      GD.PrintErr("Shape not found");
    }
    else shape.Shape = definition;
  }
}
