using Godot;

[GlobalClass]
[Tool]
public partial class PlanetNoise : Resource
{
  [Signal]
  public delegate void OnChangedEventHandler();

  [Export]
  public FastNoiseLite NoiseMap
  {
    get => _noiseMap;
    set
    {
      if (_noiseMap != null)
      {
        GD.Print("Disconnecting Listener(3)");
        _noiseMap.Changed -= onNoiseMapChanged;
      }

      _noiseMap = value;
      if (_noiseMap != null)
      {
        _noiseMap.Changed += onNoiseMapChanged;
      }

      EmitChanged();
    }
  }
  FastNoiseLite _noiseMap;

  [Export]
  public float MinHeight
  {
    get => _minHeight;
    set
    {
      _minHeight = value;
      EmitChanged();
    }
  }
  float _minHeight;

  [Export]
  public float Amplitude
  {
    get => _amplitude;
    set
    {
      _amplitude = value;
      EmitChanged();
    }
  }
  float _amplitude = 1f;

  [Export]
  public bool UseFirstLayerAsMask
  {
    get => _useFirstLayerAsMask;
    set
    {
      _useFirstLayerAsMask = value;
      EmitChanged();
    }
  }
  bool _useFirstLayerAsMask;

  void onNoiseMapChanged()
  {
    EmitChanged();
  }
}
