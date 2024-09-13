#[compute]
#version 460

struct Coord {
  float x;
  float y;
  float z;
};

// Invocations in the (x, y, z) dimension
layout(local_size_x = 32, local_size_y = 32, local_size_z = 1) in;
//layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

//
// GLSL textureless classic 3D noise "cnoise",
// with an RSL-style periodic variant "pnoise".
// Author:  Stefan Gustavson (stefan.gustavson@liu.se)
// Version: 2011-10-11
//
// Many thanks to Ian McEwan of Ashima Arts for the
// ideas for permutation and gradient selection.
//
// Copyright (c) 2011 Stefan Gustavson. All rights reserved.
// Distributed under the MIT license. See LICENSE file.
// https://github.com/stegu/webgl-noise
//

vec3 mod289(vec3 x)
{
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 mod289(vec4 x)
{
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 permute(vec4 x)
{
  return mod289(((x*34.0)+10.0)*x);
}

vec4 taylorInvSqrt(vec4 r)
{
  return 1.79284291400159 - 0.85373472095314 * r;
}

vec3 fade(vec3 t) {
  return t*t*t*(t*(t*6.0-15.0)+10.0);
}

// Classic Perlin noise
float cnoise(vec3 P)
{
  vec3 Pi0 = floor(P); // Integer part for indexing
  vec3 Pi1 = Pi0 + vec3(1.0); // Integer part + 1
  Pi0 = mod289(Pi0);
  Pi1 = mod289(Pi1);
  vec3 Pf0 = fract(P); // Fractional part for interpolation
  vec3 Pf1 = Pf0 - vec3(1.0); // Fractional part - 1.0
  vec4 ix = vec4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
  vec4 iy = vec4(Pi0.yy, Pi1.yy);
  vec4 iz0 = Pi0.zzzz;
  vec4 iz1 = Pi1.zzzz;

  vec4 ixy = permute(permute(ix) + iy);
  vec4 ixy0 = permute(ixy + iz0);
  vec4 ixy1 = permute(ixy + iz1);

  vec4 gx0 = ixy0 * (1.0 / 7.0);
  vec4 gy0 = fract(floor(gx0) * (1.0 / 7.0)) - 0.5;
  gx0 = fract(gx0);
  vec4 gz0 = vec4(0.5) - abs(gx0) - abs(gy0);
  vec4 sz0 = step(gz0, vec4(0.0));
  gx0 -= sz0 * (step(0.0, gx0) - 0.5);
  gy0 -= sz0 * (step(0.0, gy0) - 0.5);

  vec4 gx1 = ixy1 * (1.0 / 7.0);
  vec4 gy1 = fract(floor(gx1) * (1.0 / 7.0)) - 0.5;
  gx1 = fract(gx1);
  vec4 gz1 = vec4(0.5) - abs(gx1) - abs(gy1);
  vec4 sz1 = step(gz1, vec4(0.0));
  gx1 -= sz1 * (step(0.0, gx1) - 0.5);
  gy1 -= sz1 * (step(0.0, gy1) - 0.5);

  vec3 g000 = vec3(gx0.x,gy0.x,gz0.x);
  vec3 g100 = vec3(gx0.y,gy0.y,gz0.y);
  vec3 g010 = vec3(gx0.z,gy0.z,gz0.z);
  vec3 g110 = vec3(gx0.w,gy0.w,gz0.w);
  vec3 g001 = vec3(gx1.x,gy1.x,gz1.x);
  vec3 g101 = vec3(gx1.y,gy1.y,gz1.y);
  vec3 g011 = vec3(gx1.z,gy1.z,gz1.z);
  vec3 g111 = vec3(gx1.w,gy1.w,gz1.w);

  vec4 norm0 = taylorInvSqrt(vec4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
  g000 *= norm0.x;
  g010 *= norm0.y;
  g100 *= norm0.z;
  g110 *= norm0.w;
  vec4 norm1 = taylorInvSqrt(vec4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
  g001 *= norm1.x;
  g011 *= norm1.y;
  g101 *= norm1.z;
  g111 *= norm1.w;

  float n000 = dot(g000, Pf0);
  float n100 = dot(g100, vec3(Pf1.x, Pf0.yz));
  float n010 = dot(g010, vec3(Pf0.x, Pf1.y, Pf0.z));
  float n110 = dot(g110, vec3(Pf1.xy, Pf0.z));
  float n001 = dot(g001, vec3(Pf0.xy, Pf1.z));
  float n101 = dot(g101, vec3(Pf1.x, Pf0.y, Pf1.z));
  float n011 = dot(g011, vec3(Pf0.x, Pf1.yz));
  float n111 = dot(g111, Pf1);

  vec3 fade_xyz = fade(Pf0);
  vec4 n_z = mix(vec4(n000, n100, n010, n110), vec4(n001, n101, n011, n111), fade_xyz.z);
  vec2 n_yz = mix(n_z.xy, n_z.zw, fade_xyz.y);
  float n_xyz = mix(n_yz.x, n_yz.y, fade_xyz.x); 
  return 2.2 * n_xyz;
}

// Classic Perlin noise, periodic variant
float pnoise(vec3 P, vec3 rep)
{
  vec3 Pi0 = mod(floor(P), rep); // Integer part, modulo period
  vec3 Pi1 = mod(Pi0 + vec3(1.0), rep); // Integer part + 1, mod period
  Pi0 = mod289(Pi0);
  Pi1 = mod289(Pi1);
  vec3 Pf0 = fract(P); // Fractional part for interpolation
  vec3 Pf1 = Pf0 - vec3(1.0); // Fractional part - 1.0
  vec4 ix = vec4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
  vec4 iy = vec4(Pi0.yy, Pi1.yy);
  vec4 iz0 = Pi0.zzzz;
  vec4 iz1 = Pi1.zzzz;

  vec4 ixy = permute(permute(ix) + iy);
  vec4 ixy0 = permute(ixy + iz0);
  vec4 ixy1 = permute(ixy + iz1);

  vec4 gx0 = ixy0 * (1.0 / 7.0);
  vec4 gy0 = fract(floor(gx0) * (1.0 / 7.0)) - 0.5;
  gx0 = fract(gx0);
  vec4 gz0 = vec4(0.5) - abs(gx0) - abs(gy0);
  vec4 sz0 = step(gz0, vec4(0.0));
  gx0 -= sz0 * (step(0.0, gx0) - 0.5);
  gy0 -= sz0 * (step(0.0, gy0) - 0.5);

  vec4 gx1 = ixy1 * (1.0 / 7.0);
  vec4 gy1 = fract(floor(gx1) * (1.0 / 7.0)) - 0.5;
  gx1 = fract(gx1);
  vec4 gz1 = vec4(0.5) - abs(gx1) - abs(gy1);
  vec4 sz1 = step(gz1, vec4(0.0));
  gx1 -= sz1 * (step(0.0, gx1) - 0.5);
  gy1 -= sz1 * (step(0.0, gy1) - 0.5);

  vec3 g000 = vec3(gx0.x,gy0.x,gz0.x);
  vec3 g100 = vec3(gx0.y,gy0.y,gz0.y);
  vec3 g010 = vec3(gx0.z,gy0.z,gz0.z);
  vec3 g110 = vec3(gx0.w,gy0.w,gz0.w);
  vec3 g001 = vec3(gx1.x,gy1.x,gz1.x);
  vec3 g101 = vec3(gx1.y,gy1.y,gz1.y);
  vec3 g011 = vec3(gx1.z,gy1.z,gz1.z);
  vec3 g111 = vec3(gx1.w,gy1.w,gz1.w);

  vec4 norm0 = taylorInvSqrt(vec4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
  g000 *= norm0.x;
  g010 *= norm0.y;
  g100 *= norm0.z;
  g110 *= norm0.w;
  vec4 norm1 = taylorInvSqrt(vec4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
  g001 *= norm1.x;
  g011 *= norm1.y;
  g101 *= norm1.z;
  g111 *= norm1.w;

  float n000 = dot(g000, Pf0);
  float n100 = dot(g100, vec3(Pf1.x, Pf0.yz));
  float n010 = dot(g010, vec3(Pf0.x, Pf1.y, Pf0.z));
  float n110 = dot(g110, vec3(Pf1.xy, Pf0.z));
  float n001 = dot(g001, vec3(Pf0.xy, Pf1.z));
  float n101 = dot(g101, vec3(Pf1.x, Pf0.y, Pf1.z));
  float n011 = dot(g011, vec3(Pf0.x, Pf1.yz));
  float n111 = dot(g111, Pf1);

  vec3 fade_xyz = fade(Pf0);
  vec4 n_z = mix(vec4(n000, n100, n010, n110), vec4(n001, n101, n011, n111), fade_xyz.z);
  vec2 n_yz = mix(n_z.xy, n_z.zw, fade_xyz.y);
  float n_xyz = mix(n_yz.x, n_yz.y, fade_xyz.x); 
  return 2.2 * n_xyz;
}

// A binding to the buffer we create in our script
layout(set = 0, binding = 0, std430) restrict buffer OurDataBuffer {
  float data[];
}
my_data_buffer;

// layout(set = 0, binding = 0 ) restrict uniform int testing;

// A binding to the buffer we create in our script
layout(set = 0, binding = 1, std430) restrict buffer ParamsBuffer {
  float resolution;
  float radius;
  float coord[3];
  float noiseTextureCount;
}
params;

layout(set = 0, binding = 2, std430) writeonly buffer VerticesBuffer {  
  float data[];
}
buffVertices;
void setVertex( int offset, vec3 vec ) {
  buffVertices.data[ offset * 3 ] = vec.x;
  buffVertices.data[ offset * 3 + 1 ] = vec.y;
  buffVertices.data[ offset * 3 + 2 ] = vec.z;
}

layout(set = 0, binding = 3, std430) writeonly buffer UVsBuffer {  
  float data[];
}
buffUVs;
void setUV( int offset, vec2 vec ) {
  buffUVs.data[ offset * 2 ] = vec.x;
  buffUVs.data[ offset * 2 + 1 ] = vec.y;
}

layout(set = 0, binding = 4, std430) buffer NormalsBuffer {  
  float data[];
}
buffNormal;
vec3 getNormal( int offset ) {
  return vec3( 
    buffNormal.data[ offset * 3 ], 
    buffNormal.data[ offset * 3 + 1 ], 
    buffNormal.data[ offset * 3 + 2 ]
  );
}
void setNormal( int offset, vec3 vec ) {
  buffNormal.data[ offset * 3 ] = vec.x;
  buffNormal.data[ offset * 3 + 1 ] = vec.y;
  buffNormal.data[ offset * 3 + 2 ] = vec.z;
}

layout(set = 0, binding = 5, std430) buffer IndexesBuffer {  
  int data[];
}
buffIndexes;
int getIndex( int offset ) {
  return buffIndexes.data[ offset ];
}
void setIndex( int offset, int val ) {
  buffIndexes.data[ offset ] = val;
}

float color(vec2 xy) { return cnoise(vec3(1.5*xy, 0.3*1)); }

// The code we want to execute in each invocation
void main() {
    if( gl_GlobalInvocationID.x >= int(params.resolution) ) return;
    if( gl_GlobalInvocationID.y >= int(params.resolution) ) return;

    vec3 normal = vec3( params.coord[ 0 ], params.coord[ 1 ], params.coord[ 2 ]);

    // gl_GlobalInvocationID.x uniquely identifies this invocation across all work groups
    my_data_buffer.data[gl_GlobalInvocationID.x] = gl_GlobalInvocationID.x;//normal.x;

    int x = int(gl_GlobalInvocationID.x);
    int y = int(gl_GlobalInvocationID.y);
    int i = x + y * int(params.resolution);    
    
    vec2 percent = vec2( x, y ) / ( params.resolution - 1 );

    vec3 axisA = vec3( normal.y, normal.z, normal.x);
    vec3 axisB = cross( normal, axisA);

    vec3 pointOnUnitCube = normal + (percent.x - 0.5f) * (2.0f * axisA) + (percent.y - 0.5f) * 2.0f * axisB;
    vec3 pointOnUnitSphere = normalize( pointOnUnitCube ) * params.radius;

    vec3 vertex = vec3( pointOnUnitSphere.x, pointOnUnitSphere.y, pointOnUnitSphere.z );
    // vec3 vertex = vec3( pointOnUnitCube.x, pointOnUnitCube.y, pointOnUnitCube.z );
    // vec3 vertex = vec3( gl_GlobalInvocationID.x, int(params.resolution), 4.5f );

    // vertex.x = pointOnUnitSphere.x;
    // vertex.y = pointOnUnitSphere.y;
    // vertex.z = pointOnUnitSphere.z;

    if (x != params.resolution - 1 && y != params.resolution - 1)
    {           
      int indexTri = ( x + ( y * int(params.resolution - 1)) ) * 6;
      setIndex(indexTri, i + int(params.resolution) );
      setIndex(indexTri + 1, i + int(params.resolution) + 1);
      setIndex(indexTri + 2, i );              
      
      setIndex(indexTri + 3, i + int(params.resolution) + 1 );
      setIndex(indexTri + 4, i + 1 );      
      setIndex(indexTri + 5, i );
    }

    setVertex( i, vertex);
    setUV( i, vec2( 0.f, 1.f ) );
    setNormal( i, normalize( vertex ) );    

    // for (var y = 0; y < resolution; y++)
    // {
    //   for (var x = 0; x < resolution; x++)
    //   {
    //     var i = x + y * (int)resolution;
    //     var percent = new Vector2(x, y) / (resolution - 1);
    //     var pointOnUnitCube = Normal + (percent.X - 0.5f) * (2.0f * axisA) + (percent.Y - 0.5f) * 2.0f * axisB;
    //     var pointOnUnitSphere = pointOnUnitCube.Normalized() * (data != null ? data.Radius : 1f);
    //     var pointOnPlanet = data.PointOnPlanet(pointOnUnitSphere);
    //     vertices[i] = pointOnPlanet;
    //     uvs[i] = new Vector2(0f, 1f);

    //     var l = pointOnPlanet.Length();
    //     if (l < data.minHeight)
    //       data.minHeight = l;
    //     if (l > data.maxHeight)
    //       data.maxHeight = l;

    //     if (x != resolution - 1 && y != resolution - 1)
    //     {
    //       indexes[indexTri + 2] = i;
    //       indexes[indexTri + 1] = (int)(i + resolution + 1);
    //       indexes[indexTri] = (int)(i + resolution);

    //       indexes[indexTri + 5] = i;
    //       indexes[indexTri + 4] = i + 1;
    //       indexes[indexTri + 3] = i + (int)resolution + 1;

    //       indexTri += 6;
    //     }
    //   }
    // }

    // testing *= 3.0;
}

// demo code:
// float color(vec2 xy) { return cnoise(vec3(1.5*xy, 0.3*1)); }
// void mainImage(out vec4 fragColor, in vec2 fragCoord) {
//     vec2 p = (fragCoord.xy/iResolution.y) * 2.0 - 1.0;

//     vec3 xyz = vec3(p, 0);

//     vec2 step = vec2(1.3, 1.7);
//     float n = color(xyz.xy);
//     n += 0.5 * color(xyz.xy * 2.0 - step);
//     n += 0.25 * color(xyz.xy * 4.0 - 2.0 * step);
//     n += 0.125 * color(xyz.xy * 8.0 - 3.0 * step);
//     n += 0.0625 * color(xyz.xy * 16.0 - 4.0 * step);
//     n += 0.03125 * color(xyz.xy * 32.0 - 5.0 * step);

//     fragColor.xyz = vec3(0.5 + 0.5 * vec3(n, n, n));

// }