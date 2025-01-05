using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

public class Program
{
  private static GameWindow? _window;
  private static int _vao, _vbo, _ebo, _shaderProgram;
  private static Matrix4 _model, _view, _projection;
  private static float _rotationAngle = 0.0f;

  public static void Main(string[] args)
  {
    var gameWindowSettings = GameWindowSettings.Default;
    var nativeWindowSettings = new NativeWindowSettings
    {
      ClientSize = new Vector2i(800, 600),
      Title = "Rotating Cube"
    };

    _window = new GameWindow(gameWindowSettings, nativeWindowSettings);

    _window.Load += OnLoad;
    _window.RenderFrame += OnRenderFrame;
    _window.UpdateFrame += OnUpdateFrame;
    _window.Resize += OnResize;
    _window.Run();
  }

  private static void OnLoad()
  {
    GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    GL.Enable(EnableCap.DepthTest);

    float[] vertices = {
            // Positions          // Colors
            -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  0.0f, 0.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  0.5f, 0.5f, 0.5f,
            -0.5f,  0.5f,  0.5f,  0.2f, 0.8f, 0.6f
        };

    uint[] indices = {
            0, 1, 2, 2, 3, 0, // Back face
            4, 5, 6, 6, 7, 4, // Front face
            0, 4, 7, 7, 3, 0, // Left face
            1, 5, 6, 6, 2, 1, // Right face
            0, 1, 5, 5, 4, 0, // Bottom face
            3, 2, 6, 6, 7, 3  // Top face
        };

    _vao = GL.GenVertexArray();
    GL.BindVertexArray(_vao);

    _vbo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

    _ebo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
    GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
    GL.EnableVertexAttribArray(0);
    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
    GL.EnableVertexAttribArray(1);

    _shaderProgram = CreateShaderProgram();
    GL.UseProgram(_shaderProgram);

    _view = Matrix4.LookAt(new Vector3(2.0f, 2.0f, 2.0f), Vector3.Zero, Vector3.UnitY);
    _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 800f / 600f, 0.1f, 100f);

    GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view"), false, ref _view);
    GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref _projection);
  }

  private static void OnRenderFrame(FrameEventArgs e)
  {
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    _model = Matrix4.CreateRotationY(_rotationAngle);
    GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref _model);

    GL.BindVertexArray(_vao);
    GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

    _window?.SwapBuffers();
  }

  private static void OnUpdateFrame(FrameEventArgs e)
  {
    _rotationAngle += 0.005f;
  }

  private static void OnResize(ResizeEventArgs e)
  {
    GL.Viewport(0, 0, e.Width, e.Height);
    _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, e.Width / (float)e.Height, 0.1f, 100f);
    GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref _projection);
  }

  private static int CreateShaderProgram()
  {
    string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec3 aColor;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            out vec3 vertexColor;

            void main()
            {
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
                vertexColor = aColor;
            }";

    string fragmentShaderSource = @"
            #version 330 core
            in vec3 vertexColor;
            out vec4 fragColor;

            void main()
            {
                fragColor = vec4(vertexColor, 1.0);
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
