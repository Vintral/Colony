using System.Collections.Generic;
using Godot;

[GlobalClass]
[Tool]
public partial class PlanetData : Resource
{
  [Export]
  public GradientTexture1D PlanetColor
  {
    get => _planetColor;
    set
    {
      if (_planetColor != null)
      {
        _planetColor.Changed -= onPlanetColorChanged;
      }

      _planetColor = value;
      if (_planetColor != null)
      {
        _planetColor.Changed += onPlanetColorChanged;
      }

      EmitChanged();
    }
  }
  GradientTexture1D _planetColor;

  [Export]
  public float Radius
  {
    get => _radius;
    set
    {
      _radius = value;
      EmitChanged();
    }
  }
  float _radius = 1f;

  [Export]
  public uint Resolution
  {
    get => _resolution;
    set
    {
      _resolution = value;

      GD.Print("EmitChanged(1)");
      EmitChanged();
    }
  }
  uint _resolution = 10;

  public PlanetData() : this(1) { }

  public PlanetData(float radius)
  {
    Radius = radius;
  }

  [Export]
  public PlanetNoise[] PlanetNoise
  {
    get
    {
      if (_planetNoise == null)
        _planetNoise = new List<PlanetNoise>();

      return _planetNoise.ToArray();
    }
    set
    {
      if (_planetNoise == null)
        _planetNoise = new List<PlanetNoise>();
      else
      {
        _planetNoise.ForEach(n =>
        {
          GD.Print("Disconnecting Listener(1)");
          if (n != null)
            n.Changed -= onNoiseChanged;
        });
      }

      _planetNoise.Clear();
      _planetNoise.AddRange(value);
      _planetNoise.ForEach(n =>
      {
        if (n != null)
          n.Changed += onNoiseChanged;
      });

      EmitChanged();
    }
  }
  List<PlanetNoise> _planetNoise;

  public float minHeight = 99999.0f;
  public float maxHeight = 0.0f;

  public Vector3 PointOnPlanet(Vector3 pointSphere)
  {
    if (_planetNoise.Count == 0) return pointSphere;

    var elevation = 0f;
    var baseElevation = _planetNoise[0].NoiseMap.GetNoise3Dv(pointSphere * 100.0f);
    baseElevation = (baseElevation + 1) / 2.0f * _planetNoise[0].Amplitude;
    baseElevation = Mathf.Max(0f, baseElevation - _planetNoise[0].MinHeight);

    foreach (var noise in _planetNoise)
    {
      var mask = 1.0f;
      if (noise != null)
      {
        if (noise.UseFirstLayerAsMask)
        {
          mask = baseElevation;
        }

        var levelElevation = noise.NoiseMap.GetNoise3Dv(pointSphere * 100);
        levelElevation = (levelElevation + 1) / 2.0f * noise.Amplitude;
        levelElevation = Mathf.Max(0f, levelElevation - noise.MinHeight) * mask;

        elevation += levelElevation;
      }
    };

    return pointSphere * Radius * (elevation + 1.0f);
  }

  void onNoiseChanged()
  {
    EmitChanged();
  }

  void onPlanetColorChanged()
  {
    EmitChanged();
  }
}
