namespace Engine
{
    public class Engine
    {
        public Engine()
        {
            
        }

        public void Initialize(params Type[] layers)
        {
            Window win = new Window("Game", 920, 600);
        }

        public void Run() 
        {
            //while (!Window.ShouldClose)
            //{
            //    GL.glClearColor(0.2f, 0.2f, 0.2f, 1);
            //    GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);

            //    shader.Bind();
            //    geometry.Bind();
            //    texture.Bind();
            //    shader.SetUniform("uTexture", 0);

            //    unsafe
            //    {
            //        GL.glDrawElements(GL.GL_TRIANGLES, indices.Length, GL.GL_UNSIGNED_INT, null);
            //    }
            //    Glfw.SwapBuffers(NativeWindow);
            //}
        }
    }
}