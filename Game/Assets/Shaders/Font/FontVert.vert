  #version 330 core
  layout(location = 0) in vec3 position;
  layout(location = 1) in vec4 color;
  layout(location = 2) in vec2 uv;
  
  out vec2 fragUV;
  out vec4 vColor;
  
  uniform mat4 uVP;

  void main() 
  {
      fragUV = uv;
      vColor = color;
      gl_Position = uVP * vec4(position, 1.0);
  }