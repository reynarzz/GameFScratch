  #version 330 core
  layout(location = 0) in vec3 position;
  layout(location = 1) in vec2 uv;
  
  out vec2 screenUV;
  out vec4 vColor;
  
  void main() 
  {
      screenUV = uv;
      vColor = vec4(1.0);
      gl_Position = vec4(position, 1.0);
  }