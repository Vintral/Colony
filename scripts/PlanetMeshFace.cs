using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

enum BufferType
{
  DATA = 0,
  PARAMS = 1,
  VERTICES = 2,
  UVS = 3,
  NORMALS = 4,
  INDEXES = 5,
  // TEXTURE_1 = 6,
  // TEXTURE_2 = 7,
}

public enum Direction
{
  NONE, LEFT, RIGHT, UP, DOWN
}

public static class TypeSize<T>
{
  private static int _size;
  public static uint Size
  {
    get => (uint)_size;
  }

  static TypeSize()
  {
    var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
    ILGenerator il = dm.GetILGenerator();
    il.Emit(OpCodes.Sizeof, typeof(T));
    il.Emit(OpCodes.Ret);
    _size = (int)dm.Invoke(null, null);
  }
}

[Tool]
public partial class PlanetMeshFace : MeshInstance3D
{
  [Signal]
  public delegate void MeshGeneratedEventHandler();

  [Signal]
  public delegate void TilesGeneratedEventHandler();

  [Signal]
  public delegate void NeighborsSetEventHandler();

  [Export]
  public PlanetMeshFace Left;

  [Export]
  public PlanetMeshFace Right;

  [Export]
  public PlanetMeshFace Up;

  [Export]
  public PlanetMeshFace Down;

  [Export]
  public Vector3 Normal
  {
    get => _normal;
    set
    {
      _normal = value;
      GD.Print("Set Normal");
    }
  }

  PlanetData _planetData;

  public Tile[,] Tiles { get => _tiles; }
  Tile[,] _tiles;

  Vector3 _normal;

  Godot.Collections.Array _arrays;

  RenderingDevice _rd;
  Rid _shader;
  int _counter = 0;
  Dictionary<BufferType, Rid> _buffers = new();
  Dictionary<BufferType, uint> _bufferSizes = new();

  int _resolution;
  public int Resolution { get => _resolution; }

  Vector3[] _normals;

  public override void _Ready()
  {
    base._Ready();

    setupRenderingDevice();
  }

  public override void _ExitTree()
  {
    base._ExitTree();

    GD.Print("Exiting Tree");
  }

  public override void _Notification(int what)
  {
    base._Notification(what);

    if (what == NotificationPredelete)
      GD.Print("CLEAN UP");
  }

  int getHitFace(Vector3 position, Camera3D camera, out Vector2 coords)
  {
    var vertices = Mesh.GetFaces();
    coords = Vector2.Inf;

    var arrayMesh = new ArrayMesh();
    var arrays = new Godot.Collections.Array();
    arrays.Resize((int)Mesh.ArrayType.Max);

    arrays[(int)Mesh.ArrayType.Vertex] = vertices;
    arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

    var mdt = new MeshDataTool();
    mdt.CreateFromSurface(Mesh as ArrayMesh, 0);

    var origin = camera.GlobalTransform.Origin;
    var purpleArrow = position - origin;

    var i = 0;
    while (i < vertices.Length)
    {
      var faceIndex = i / 3;
      var a = ToGlobal(vertices[i]);
      var b = ToGlobal(vertices[i + 1]);
      var c = ToGlobal(vertices[i + 2]);

      if (a.Z < 0 && b.Z < 0 && c.Z < 0)
      {
        i += 3;
        continue;
      }

      var intersection = Geometry3D.RayIntersectsTriangle(origin, purpleArrow, a, b, c).AsVector3();
      if (intersection != Vector3.Zero)
      {
        var angle = Mathf.RadToDeg(purpleArrow.AngleTo(mdt.GetFaceNormal(faceIndex)));
        // GD.Print("ANGLE:::" + angle);
        // GD.Print(mdt.GetFaceNormal(faceIndex));
        // GD.Print("NORMAL: " + Mathf.RadToDeg(purpleArrow.AngleTo((b - a).Cross(c - a).Normalized())));
        // GD.Print("==========================");
        // GD.Print(a);
        // GD.Print(b);
        // GD.Print(c);
        // GD.Print("==========================");
        // if (angle > 90 && angle < 180)
        if (Mathf.RadToDeg(purpleArrow.AngleTo((b - a).Cross(c - a).Normalized())) < 90)
        {
          arrays = Mesh.SurfaceGetArrays(0);

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

          var faces = _resolution - 1;
          var x = faceIndex / 2 / faces;
          var y = faceIndex / 2 % faces;
          GD.Print("Coords @ (" + x + "," + y + ") --- " + "(" + (Resolution - 2 - x) + "," + (_resolution - 2 - y) + ") --- " + _resolution + " ||| " + faceIndex);
          // GD.Print("Indexes: " + arrays[(int)Mesh.ArrayType.Index].AsVector3Array().Length);
          // GD.Print("UVs: " + arrays[(int)Mesh.ArrayType.TexUV2].AsVector2Array().Length);
          // GD.Print("Normals: " + arrays[(int)Mesh.ArrayType.Normal].AsVector3Array().Length);

          // var m = new ArrayMesh();
          // m.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
          // Mesh = m;

          // root.Mesh = new SphereMesh();

          coords = new Vector2(x, y);
          return faceIndex;
        }
      }

      i += 3;
    }

    return -1;
  }

  public Vector4 GetFaceIndexes(int x, int y, Direction dir = Direction.NONE)
  {
    // GD.Print("GetFaceIndexes: " + x + "," + y);
    return Vector4.Zero;

    var vertices = Mesh.GetFaces();

    // var face = x + y * 

    if (x < 0 || x >= Resolution || y < 0 || y >= Resolution)
    {
      GD.Print("OTHER FACE");
      if (x < 0)
      {
        return Right?.GetFaceIndexes(x + Resolution - 1, y, Direction.RIGHT) ?? Vector4.Inf;
      }
      else if (x >= Resolution)
      {
        return Left?.GetFaceIndexes(x - Resolution - 1, y, Direction.LEFT) ?? Vector4.Inf;
      }
      else if (y < 0)
      {
        return Down?.GetFaceIndexes(x, y + Resolution - 1, Direction.DOWN) ?? Vector4.Inf;
      }
      else if (y >= Resolution)
      {
        return Up?.GetFaceIndexes(x, y - Resolution - 1, Direction.UP) ?? Vector4.Inf;
      }
    }
    else
    {

      GD.Print("Vertices Length: " + vertices.Length + " ||| " + ((x + (y * (Resolution - 1))) * 6));

      var offset = (x + (y * (Resolution - 1))) * 6;

      // GD.Print(ToGlobal(vertices[offset]));
      // GD.Print(ToGlobal(vertices[offset + 1]));
      // GD.Print(ToGlobal(vertices[offset + 2]));
      // GD.Print(ToGlobal(vertices[offset + 3]));
    }

    return Vector4.Zero;
  }

  public bool CanBuildAt(Vector3 position, Camera3D camera, int size, out Vector2 center)
  {
    GD.Print("CanBuildAt");

    center = new Vector2(position.X, position.Y);

    var face = getHitFace(position, camera, out center);
    if (face != -1 && center != Vector2.Inf)
    {
      GD.Print("WE FOUND FACE: " + face + " @ (" + center.X + "," + center.Y + ")");
      var source = GetFaceIndexes((int)center.X, (int)center.Y);
      // GD.Print(source);
      for (var x = (int)center.X - size / 2; x <= center.X + size / 2; x++)
      {
        for (var y = (int)center.Y - size / 2; y <= center.Y + size / 2; y++)
        {
          // GD.Print("Check: " + x + "," + y);
          GetFaceIndexes(x, y);
        }
      }
    }

    return false;
  }

  void setupRenderingDevice()
  {
    // Create a local rendering device.
    _rd = RenderingServer.CreateLocalRenderingDevice();

    // Load GLSL shader
    var shaderFile = GD.Load<RDShaderFile>("res://shaders/compute_example.glsl");
    var shaderBytecode = shaderFile.GetSpirV();
    _shader = _rd.ShaderCreateFromSpirV(shaderBytecode);
  }

  Rid? bindBuffers(Dictionary<Rid, RDUniform> data)
  {
    if (data == null) return null;

    foreach (var item in data)
    {
      item.Value.AddId(item.Key);
    }

    return _rd.UniformSetCreate(new Array<RDUniform>(data.Values.ToArray()), _shader, 0);
  }

  void createEmptyBuffer<T>(BufferType type, uint size)
  {
    _bufferSizes[type] = size * TypeSize<T>.Size;
    _buffers[type] = _rd.StorageBufferCreate(_bufferSizes[type]);

    // GD.Print("Size(" + type + "): " + _bufferSizes[type]);
  }

  Dictionary<Rid, RDUniform> createBufferDefinitions()
  {
    var bufferDefinitions = new Dictionary<Rid, RDUniform>();

    foreach (BufferType value in Enum.GetValues<BufferType>())
    {
      bufferDefinitions[_buffers[value]] = new RDUniform
      {
        UniformType = RenderingDevice.UniformType.StorageBuffer,
        Binding = (int)value
      };
    }

    return bufferDefinitions;
  }

  Dictionary<Rid, RDUniform> createBuffers()
  {
    if (_rd == null) return null;

    var input = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    _bufferSizes[BufferType.DATA] = (uint)input.Length * sizeof(float);
    var inputBytes = new byte[_bufferSizes[BufferType.DATA]];
    Buffer.BlockCopy(input, 0, inputBytes, 0, inputBytes.Length);
    _buffers[BufferType.DATA] = _rd.StorageBufferCreate((uint)inputBytes.Length, inputBytes);

    var bufferParams = getParams();
    _bufferSizes[BufferType.PARAMS] = (uint)bufferParams.Length * sizeof(float);
    var inputParams = new byte[_bufferSizes[BufferType.PARAMS]];
    Buffer.BlockCopy(bufferParams, 0, inputParams, 0, inputParams.Length);
    _buffers[BufferType.PARAMS] = _rd.StorageBufferCreate((uint)inputParams.Length, inputParams);

    var resolution = _planetData.Resolution;
    var numVertices = resolution * resolution;
    var numIndices = (resolution - 1) * (resolution - 1) * 6;

    // GD.Print("Vertices: " + numVertices);
    // GD.Print("Indexes: " + numIndices);
    // GD.Print("Size of Float: " + sizeof(float));

    createEmptyBuffer<Vector3>(BufferType.VERTICES, numVertices);
    createEmptyBuffer<Vector2>(BufferType.UVS, numVertices);
    createEmptyBuffer<Vector3>(BufferType.NORMALS, numVertices);
    createEmptyBuffer<int>(BufferType.INDEXES, numIndices);

    // var noise = _planetData.PlanetNoise[0];
    // if (noise != null)
    // {
    //   //// GD.Print(noise.NoiseMap.)
    // }
    // else GD.Print("Noise Texture Null!");

    //_buffers[BufferType.TEXTURE_1] = _rd.

    return createBufferDefinitions();
  }

  void runShader(Rid uniformSet)
  {
    // GD.Print("Groups: " + (_planetData.Resolution >> 5));

    // Create a compute pipeline
    var pipeline = _rd.ComputePipelineCreate(_shader);
    var computeList = _rd.ComputeListBegin();
    _rd.ComputeListBindComputePipeline(computeList, pipeline);
    _rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
    //_rd.ComputeListDispatch(computeList, xGroups: Math.Max(1, _planetData.Resolution >> 4), yGroups: Math.Max(1, _planetData.Resolution), zGroups: 1);
    // _rd.ComputeListDispatch(computeList, xGroups: Math.Max(1, _planetData.Resolution >> 5), yGroups: Math.Max(1, _planetData.Resolution >> 5), zGroups: 1);
    // _rd.ComputeListDispatch(computeList, xGroups: Math.Max(1, _planetData.Resolution), yGroups: Math.Max(1, _planetData.Resolution), zGroups: 1);
    _rd.ComputeListDispatch(computeList, xGroups: 1, yGroups: 1, zGroups: 1);
    _rd.ComputeListEnd();

    // Submit to GPU and wait for sync
    _rd.Submit();
  }

  float[] getParams()
  {
    var ret = new System.Collections.Generic.List<float>
    {
      _planetData.Resolution, // Resolution
      _planetData.Radius, // Radius
      Normal.X, // Normal vector for side
      Normal.Y,
      Normal.Z,
      _planetData.PlanetNoise.Length, // Texture Length
    };

    return ret.ToArray();
  }

  void callComputeShader()
  {
    // Prepare our data. We use floats in the shader, so we need 32 bit.
    var buffers = createBuffers();
    var uniformSet = bindBuffers(buffers);

    if (uniformSet != null)
      runShader((Rid)uniformSet);
  }

  RenderingDevice callComputeShaderOld(PlanetData data, out Rid bufferData, out uint bufferSize)
  {
    // Create a local rendering device.
    var rd = RenderingServer.CreateLocalRenderingDevice();

    // Load GLSL shader
    var shaderFile = GD.Load<RDShaderFile>("res://shaders/compute_example.glsl");
    var shaderBytecode = shaderFile.GetSpirV();
    var shader = rd.ShaderCreateFromSpirV(shaderBytecode);

    // Prepare our data. We use floats in the shader, so we need 32 bit.
    //const int len = 10000000;//50000000;
    // var input = new float[len];
    // for (var i = 0; i < len; i++)
    // {
    //   input[i] = i + 1;
    // }
    var input = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    // var numVertices = resolution * resolution;
    // var numIndices = (resolution - 1) * (resolution - 1) * 6;

    // var indexTri = 0;
    // var axisA = new Vector3(Normal.Y, Normal.Z, Normal.X);
    // var axisB = Normal.Cross(axisA);

    // var vertices = new Vector3[numVertices];
    // var uvs = new Vector2[numVertices];
    // var normals = new Vector3[numVertices];
    // var indexes = new int[numIndices];


    var inputBytes = new byte[input.Length * sizeof(float)];
    GD.Print("LEngth: " + input.Length + " * " + sizeof(float) + " = " + inputBytes.Length);
    Buffer.BlockCopy(input, 0, inputBytes, 0, inputBytes.Length);

    // Create a storage buffer that can hold our float values.
    // Each float has 4 bytes (32 bit) so 10 x 4 = 40 bytes

    var resolution = data.Resolution;
    if (Engine.IsEditorHint())
    {
      resolution = (uint)Mathf.Min(resolution, 50);
    }

    var numVertices = resolution * resolution;
    var numIndices = (resolution - 1) * (resolution - 1) * 6;

    bufferSize = (uint)inputBytes.Length;//(uint)((numVertices * 2 * 3 * sizeof(float)) + (numVertices * sizeof(float)) + (numIndices * sizeof(uint)));
    bufferData = rd.StorageBufferCreate((uint)bufferSize, inputBytes);

    GD.Print("Curr Buffer Size: " + bufferSize);

    var dataParams = new float[] { 4f };
    var inputParams = new byte[dataParams.Length * sizeof(float)];
    Buffer.BlockCopy(dataParams, 0, inputParams, 0, inputParams.Length);

    var bufferParams = rd.StorageBufferCreate((uint)sizeof(float), inputParams);

    var buffers = new Dictionary<Rid, RDUniform>();
    buffers[bufferData] = new RDUniform
    {
      UniformType = RenderingDevice.UniformType.StorageBuffer,
      Binding = 0
    };
    // buffers[bufferParams] = new RDUniform
    // {
    //   UniformType = RenderingDevice.UniformType.StorageBuffer,
    //   Binding = 1
    // };
    var uniformSet = rd.UniformSetCreate(new Array<RDUniform> { buffers[bufferData] }, shader, 0);

    // Create a compute pipeline
    var pipeline = rd.ComputePipelineCreate(shader);
    var computeList = rd.ComputeListBegin();
    rd.ComputeListBindComputePipeline(computeList, pipeline);
    rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
    // rd.ComputeListBindUniformSet(computeList, uniformTestingSet, 1);
    rd.ComputeListDispatch(computeList, xGroups: resolution, yGroups: resolution, zGroups: 1);
    rd.ComputeListEnd();

    GD.Print("Input: ", string.Join(", ", input));

    // Submit to GPU and wait for sync
    rd.Submit();
    rd.Sync();

    // Read back the data from the buffers
    var outputBytes = rd.BufferGetData(bufferData);
    var output = new float[bufferSize / sizeof(float)];
    Buffer.BlockCopy(outputBytes, 0, output, 0, (int)bufferSize);
    GD.Print("Output: ", string.Join(", ", output));

    return rd;
  }

  Godot.Collections.Array getComputeResults()
  {
    if (_rd == null)
    {
      return null;
    }

    _rd.Sync();

    var arrays = new Godot.Collections.Array();
    arrays.Resize((int)Mesh.ArrayType.Max);

    foreach (var buffer in _buffers)
    {
      var size = _bufferSizes[buffer.Key];
      var outputBytes = _rd.BufferGetData(buffer.Value);

      switch (buffer.Key)
      {
        case BufferType.NORMALS:
          {
            var normalBuffer = new float[size / TypeSize<float>.Size];
            Buffer.BlockCopy(outputBytes, 0, normalBuffer, 0, (int)size);

            var normals = new System.Collections.Generic.List<Vector3>();
            for (var i = 0; i < normalBuffer.Length; i += 3)
            {
              normals.Add(new Vector3(
                normalBuffer[i],
                normalBuffer[i + 1],
                normalBuffer[i + 2]
              ));
            }
            arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            // GD.Print("Normals Length: " + normals.Count);
            // GD.Print("Normals: ", string.Join(", ", normalBuffer.Take(16)));
          }
          break;
        case BufferType.UVS:
          {
            var uvBuffer = new float[size / TypeSize<float>.Size];
            Buffer.BlockCopy(outputBytes, 0, uvBuffer, 0, (int)size);

            var uvs = new System.Collections.Generic.List<Vector2>();
            for (var i = 0; i < uvBuffer.Length; i += 2)
            {
              uvs.Add(new Vector2(
                uvBuffer[i],
                uvBuffer[i + 1]
              ));
            }
            arrays[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
            // GD.Print("UVs Length: " + uvs.Count);

            // GD.Print("UVs: ", string.Join(", ", uvs.Take(16)));
          }
          break;
        case BufferType.INDEXES:
          {
            var indexes = new int[size / TypeSize<int>.Size];
            Buffer.BlockCopy(outputBytes, 0, indexes, 0, (int)size);
            arrays[(int)Mesh.ArrayType.Index] = indexes;

            // GD.Print("Indexes Length: " + indexes.Length);

            // GD.Print("Indexes: ", string.Join(", ", indexes.Take(24)));
          }
          break;
        case BufferType.VERTICES:
          {
            // GD.Print("VERTICES SIZE: " + (size / TypeSize<float>.Size));
            var verticesBuffer = new float[size / TypeSize<float>.Size];
            Buffer.BlockCopy(outputBytes, 0, verticesBuffer, 0, (int)size);

            var vertices = new System.Collections.Generic.List<Vector3>();
            for (var i = 0; i < verticesBuffer.Length; i += 3)
            {
              vertices.Add(new Vector3(
                verticesBuffer[i],
                verticesBuffer[i + 1],
                verticesBuffer[i + 2]
              ));
            }
            arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
            // GD.Print("Vertices Length: " + vertices.Count);

            // GD.Print("Vertices: ", string.Join(", ", vertices.Take(16)));
            // GD.Print("Vertices: ", string.Join(", ", vertices));
          }
          break;
      }
    }

    return arrays;
  }

  public void RegenerateMeshUsingComputeShader(PlanetData data)
  {
    if (data == null) return;

    GD.Print("REGENERATE");

    _planetData = data;
    _resolution = (int)data.Resolution;
    callComputeShader();
    // await Task.Delay(100);
    // var rd = callComputeShader(data, out buffer, out bufferSize);
    //var arrays = getComputeResults(rd, buffer, bufferSize);

    var arrays = getComputeResults();
    if (arrays == null)
    {
      GD.Print("Empty arrays");
      return;
    }
    // CallDeferred("updateMesh", arrays, data);
    if (!Engine.IsEditorHint())
    {
      CallDeferred("updateMesh", arrays, data);
    }
  }

  public void RegenerateMesh(PlanetData data)
  {
    if (data == null) return;

    var arrays = new Godot.Collections.Array();
    arrays.Resize((int)Mesh.ArrayType.Max);

    _resolution = (int)data.Resolution;
    if (Engine.IsEditorHint())
    {
      _resolution = Mathf.Min(_resolution, 25);
    }

    var numVertices = _resolution * _resolution;
    var numIndices = (_resolution - 1) * (_resolution - 1) * 6;

    // GD.Print("Normal", Normal);
    var indexTri = 0;
    var axisA = new Vector3(Normal.Y, Normal.Z, Normal.X);
    var axisB = Normal.Cross(axisA);

    var vertices = new Vector3[numVertices];
    var uvs = new Vector2[numVertices];
    var normals = new Vector3[numVertices];
    var indexes = new int[numIndices];

    // GD.Print("RADIUS: " + data.Radius);

    int count = 0;
    for (var y = 0; y < _resolution; y++)
    {
      for (var x = 0; x < _resolution; x++)
      {
        var i = x + y * (int)_resolution;
        var percent = new Vector2(x, y) / (_resolution - 1);
        var pointOnUnitCube = Normal + (percent.X - 0.5f) * (2.0f * axisA) + (percent.Y - 0.5f) * 2.0f * axisB;
        var pointOnUnitSphere = pointOnUnitCube.Normalized() * (data != null ? data.Radius : 1f);
        //var pointOnPlanet = data.PointOnPlanet(pointOnUnitSphere);
        //vertices[i] = pointOnPlanet;
        vertices[i] = pointOnUnitSphere;
        // vertices[i] = pointOnUnitCube;
        uvs[i] = new Vector2(0f, 1f);

        // var l = pointOnPlanet.Length();
        // if (l < data.minHeight)
        //   data.minHeight = l;
        // if (l > data.maxHeight)
        //   data.maxHeight = l;

        if (x != _resolution - 1 && y != _resolution - 1)
        {
          // if (count++ < 10)
          //   GD.Print("TRI: " + (int)(i + resolution + 1) + " == (" + x + "," + y + ") ||| " + resolution);

          indexes[indexTri + 2] = i;
          indexes[indexTri + 1] = (int)(i + _resolution + 1);
          indexes[indexTri] = (int)(i + _resolution);

          indexes[indexTri + 5] = i;
          indexes[indexTri + 4] = i + 1;
          indexes[indexTri + 3] = i + (int)_resolution + 1;

          indexTri += 6;
        }
      }
    }

    for (var a = 0; a < indexes.Length; a += 3)
    {
      var b = a + 1;
      var c = a + 2;
      var ab = vertices[indexes[b]] - vertices[indexes[a]];
      var bc = vertices[indexes[c]] - vertices[indexes[b]];
      var ca = vertices[indexes[a]] - vertices[indexes[c]];
      var cross_ab_bc = ab.Cross(bc) * -1;
      var cross_bc_ca = bc.Cross(ca) * -1;
      var cross_ca_ab = ca.Cross(ab) * -1;

      normals[indexes[a]] += cross_ab_bc + cross_bc_ca + cross_ca_ab;
      normals[indexes[b]] += cross_ab_bc + cross_bc_ca + cross_ca_ab;
      normals[indexes[c]] += cross_ab_bc + cross_bc_ca + cross_ca_ab;

      normals[indexes[a]].Normalized();
      normals[indexes[b]].Normalized();
      normals[indexes[c]].Normalized();
    }

    // for (var a = 0; a < normals.Length; a++)
    // {
    //   normals[a] = normals[a].Normalized();
    // }

    _normals = normals;
    arrays[(int)Mesh.ArrayType.Vertex] = vertices;
    arrays[(int)Mesh.ArrayType.TexUV] = uvs;
    arrays[(int)Mesh.ArrayType.Normal] = normals;
    arrays[(int)Mesh.ArrayType.Index] = indexes;

    // GD.Print("Vertices:" + string.Join(",", vertices.Take(16).ToArray()));
    // GD.Print("Vertices:" + string.Join(",", vertices.ToArray()));
    // GD.Print("UVs:" + string.Join(",", uvs.Take(10).ToArray()));
    // GD.Print("Normals:" + string.Join(",", normals.Take(10).ToArray()));
    // GD.Print("Indexes:" + string.Join(",", indexes.Take(10).ToArray()));

    if (Engine.IsEditorHint())
    {
      CallDeferred("updateMesh", arrays, data);
    }
  }

  Tile getTileAt(int x, int y)
  {
    GD.Print("getTileAt: " + x + "," + y);

    if (x < 0 || y < 0 || x > _planetData.Resolution - 2 || y > _planetData.Resolution - 2)
    {
      if (x < 0)
      {
        GD.Print("Go Right");
        return Right?.GetTileMatching((int)_planetData.Resolution - 1, y, this, getTileAt(0, y));
      }
      else if (y < 0)
      {
        GD.Print("Go Down");
        return Down?.GetTileMatching(x, (int)_planetData.Resolution - 1, this, getTileAt(x, 0));
      }
      else if (x >= _planetData.Resolution - 1)
      {
        GD.Print("Go Left");
        GD.Print(_planetData.Resolution - 1 + "|" + x + "|" + y);
        getTileAt(0, 0).Dump();
        return Left?.GetTileMatching(0, y, this, getTileAt((int)_planetData.Resolution - 2, y));
      }
      else if (y >= _planetData.Resolution - 1)
      {
        GD.Print("Go Up");
        GD.Print(_planetData.Resolution - 1 + "|" + y + "|" + x);
        return Up?.GetTileMatching(x, 0, this, getTileAt(x, (int)_planetData.Resolution - 2));
      }

      return null;
    }
    try
    {
      return _tiles[x, y];
    }
    catch (Exception ex)
    {
      GD.Print("ERROR: " + x + "," + y);
      GD.Print("Length: " + _tiles.Length);
      GD.Print(ex.Message);
      return null;
    }
  }

  public Tile GetTileMatching(int x, int y, PlanetMeshFace face, Tile other)
  {
    GD.Print("GetTileMatching: " + x + ", " + y + " ||| " + other.Vertices);

    var offset = (x == 0 || x == _planetData.Resolution - 1) ? y : x;
    var max = _planetData.Resolution - 1;

    if (face == Left)
    {
      GD.Print("from LEFT face");

      if (_tiles[max, max - offset].Adjacent(other))
        GD.Print("FOUND ADJACENT FACE");
      else if (_tiles[max, offset].Adjacent(other))
        GD.Print("FOUND ADJACENT FACE");
      else GD.Print("FOUND NO MATCHING FACE ::: " + max + "|" + (max - offset) + "|" + offset);
    }
    if (face == Right) GD.Print("from RIGHT face");
    if (face == Down) GD.Print("from DOWN face");
    if (face == Up)
    {
      GD.Print("from UP face");

      // if (_tiles[offset, max].Adjacent(other))
      //   GD.Print("FOUND ADJACENT FACE");
      // else if (_tiles[max - offset, max].Adjacent(other))
      //   GD.Print("FOUND ADJACENT FACE");
      // else 
      // GD.Print("FOUND NO MATCHING FACE ::: " + max + "|" + (max - offset) + "|" + offset);

      GD.Print("Length: " + _tiles.Length);
      var sides = (int)Mathf.Sqrt(_tiles.Length);

      for (var i = 0; i < sides; i++)
      {
        for (var n = 0; n < sides; n++)
        {
          if (_tiles[i, n].Adjacent(other))
          {
            GD.Print("FOUND MATCHING TILE: " + i + "," + n);
          }
          else GD.Print("No match: " + i + "," + n);
        }
      }
    }

    return other;
  }

  public void SetNeighbors()
  {
    var vertexes = _arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
    var indexes = _arrays[(int)Mesh.ArrayType.Index].AsInt32Array();

    for (var x = 0; x < _planetData.Resolution - 1; x++)
    {
      for (var y = 0; y < _planetData.Resolution - 1; y++)
      {
        GD.Print("SetNeighbors: " + x + "," + y);
        // if (_tiles != null && _tiles[x, y] != null)
        // {
        //   GD.Print("Tiles Exist");
        // }
        // else GD.Print("Tiles Don't Exist: " + x + "," + y);
        _tiles[x, y].Left = getTileAt(x + 1, y);
        // _tiles[x, y].Right = getTileAt(x - 1, y);
        // _tiles[x, y].Up = getTileAt(x, y + 1);
        // _tiles[x, y].Down = getTileAt(x, y - 1);
      }
    }

    EmitSignal(SignalName.NeighborsSet);
  }

  public void CreateTiles()
  {
    var vertexes = _arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
    var indexes = _arrays[(int)Mesh.ArrayType.Index].AsInt32Array();

    _tiles = new Tile[_planetData.Resolution - 1, _planetData.Resolution - 1];

    for (var x = 0; x < _planetData.Resolution - 1; x++)
    {
      for (var y = 0; y < _planetData.Resolution - 1; y++)
      {
        var origin = (x + (y * (_planetData.Resolution - 1))) * 6;
        // GD.Print("origin: " + origin + " --- " + x + "|" + y + "|" + (_planetData.Resolution - 1));

        for (var n = 0; n < 6; n++)
        {
          if (vertexes[indexes[origin + 1]] == vertexes[indexes[origin + n]])
          {
            // var tile = new Tile();
            // tile.AddVertex(vertexes[indexes[origin]]);
            // tile.AddVertex(vertexes[indexes[origin + 1]]);
            // tile.AddVertex(vertexes[indexes[origin + 2]]);
            // tile.AddVertex(vertexes[indexes[origin + 4]]);
            // tile.Dump();
            // _tiles[x, y] = tile;
          }
        }
      }
    }

    EmitSignal(SignalName.TilesGenerated);
  }

  void updateMesh(Godot.Collections.Array arrays, PlanetData data)
  {
    // GD.Print("updateMesh");

    var mesh = new ArrayMesh();
    mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
    Mesh = mesh;

    // if (mesh != null)
    // {
    //   foreach (var child in GetChildren())
    //   {
    //     child.Free();
    //   }
    //   CreateTrimeshCollision();
    // }

    _arrays = arrays;

    using var shader = MaterialOverride as ShaderMaterial;
    shader?.SetShaderParameter("min_height", data.minHeight);
    shader?.SetShaderParameter("max_height", data.maxHeight);
    shader?.SetShaderParameter("height_color", data.PlanetColor);

    EmitSignal(SignalName.MeshGenerated);
  }
}
