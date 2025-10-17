  #version 330 core
  layout(location = 0) in vec3 position;
  layout(location = 1) in vec2 uv;
  layout(location = 2) in vec3 normals;
  layout(location = 3) in uint color; 
  layout(location = 4) in int texIndex; 
  
  out vec2 fragUV;
  flat out int fragTexIndex;            // flat = no interpolation between vertices
  out vec4 vColor;
  uniform mat4 uVP;
  out vec2 worldUV;

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
      worldUV = position.xy * 0.1;
      fragTexIndex = texIndex; 
      vColor = unpackColor(color);
      gl_Position = uVP * vec4(position, 1.0);
  }