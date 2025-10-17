  #version 330 core
  layout(location = 0) in vec3 position;
  layout(location = 1) in vec2 uv;
  layout(location = 2) in vec3 normals;
  layout(location = 3) in uint color; 
  layout(location = 4) in int texIndex; 
  
  out vec2 fragUV;
  out vec2 screenUV;
  out vec2 worldUV;

  flat out int fragTexIndex;            // flat = no interpolation between vertices
  out vec4 vColor;
  uniform mat4 uVP;
  uniform mat4 uProjectionMatrix;

  vec4 unpackColor(uint c) 
  {
      float r = float((c >> 24) & 0xFFu) / 255.0;
      float g = float((c >> 16) & 0xFFu) / 255.0;
      float b = float((c >>  8) & 0xFFu) / 255.0;
      float a = float( c        & 0xFFu) / 255.0;
      return vec4(r,g,b,a);
  }
  
  void main() 
  {
      fragUV = uv;
      fragTexIndex = texIndex; 
      worldUV = position.xy * 0.1;
      vColor = unpackColor(color);

      // Transform to clip space
      vec4 posEnd = uVP * vec4(position, 1.0);
      // Convert clip space NDC (divide by w)
      vec3 ndc = posEnd.xyz / posEnd.w;

      // NDC [-1,1] screen UV [0,1]
      screenUV = ndc.xy * 0.5 + 0.5;

      gl_Position = posEnd;
  }