using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

public class Program
{
  private static GameWindow? _window;
  private static int _vao, _vbo, _shaderProgram;

  public static void Main(string[] args)
  {
    var gameWindowSettings = GameWindowSettings.Default;
    var nativeWindowSettings = new NativeWindowSettings
    {
      ClientSize = new Vector2i(800, 600),
      Title = "Simple Triangle"
    };

    _window = new GameWindow(gameWindowSettings, nativeWindowSettings);

    _window.Load += OnLoad;
    _window.RenderFrame += OnRenderFrame;
    _window.Resize += OnResize;
    _window.Run();
  }

  private static void OnLoad()
  {
    GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

    GL.Enable(EnableCap.DepthTest);

    float[] vertices = {
            -0.5f, -0.5f, 0.0f,
             0.5f, -0.5f, 0.0f,
             0.0f,  0.5f, 0.0f
        };

    _vao = GL.GenVertexArray();
    GL.BindVertexArray(_vao);

    _vbo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
    GL.EnableVertexAttribArray(0);

    _shaderProgram = CreateShaderProgram();
    GL.UseProgram(_shaderProgram);
  }

  private static void OnRenderFrame(FrameEventArgs e)
  {
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    GL.BindVertexArray(_vao);
    GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

    _window?.SwapBuffers();
  }

  private static void OnResize(ResizeEventArgs e)
  {
    GL.Viewport(0, 0, e.Width, e.Height);
  }

  private static int CreateShaderProgram()
  {
    string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;

            void main()
            {
                gl_Position = vec4(aPosition, 1.0);
            }";

    string fragmentShaderSource = @"
            #version 330 core
            out vec4 fragColor;

            void main()
            {
                fragColor = vec4(1.0, 0.0, 0.0, 1.0);
            }";

    int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
    int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

    int shaderProgram = GL.CreateProgram();
    GL.AttachShader(shaderProgram, vertexShader);
    GL.AttachShader(shaderProgram, fragmentShader);
    GL.LinkProgram(shaderProgram);

    GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out int success);
    if (success == 0)
    {
      string info = GL.GetProgramInfoLog(shaderProgram);
      throw new Exception($"Shader linking failed: {info}");
    }

    GL.DeleteShader(vertexShader);
    GL.DeleteShader(fragmentShader);

    return shaderProgram;
  }

  private static int CompileShader(ShaderType type, string source)
  {
    int shader = GL.CreateShader(type);
    GL.ShaderSource(shader, source);
    GL.CompileShader(shader);

    GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
    if (success == 0)
    {
      string info = GL.GetShaderInfoLog(shader);
      throw new Exception($"{type} Compilation failed: {info}");
    }

    return shader;
  }
}
