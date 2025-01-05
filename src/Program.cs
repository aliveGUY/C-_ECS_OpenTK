using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using StbImageSharp;

public class Program
{
  private static GameWindow? _window;
  private static int _vao, _vbo, _ebo, _shaderProgram, _texture;
  private static List<float> _vertices = new();
  private static List<uint> _indices = new();
  private static Matrix4 _model, _view, _projection;
  private static float _rotationAngle = 0.0f;

  public static void Main(string[] args)
  {
    var gameWindowSettings = GameWindowSettings.Default;
    var nativeWindowSettings = new NativeWindowSettings
    {
      ClientSize = new Vector2i(800, 600),
      Title = "Textured OBJ Mesh Loader"
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

    LoadOBJ("static/meshes/Cat.obj");

    _vao = GL.GenVertexArray();
    GL.BindVertexArray(_vao);

    _vbo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * sizeof(float), _vertices.ToArray(), BufferUsageHint.StaticDraw);

    _ebo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
    GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(uint), _indices.ToArray(), BufferUsageHint.StaticDraw);

    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0); // Position
    GL.EnableVertexAttribArray(0);

    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float)); // Texture coordinates
    GL.EnableVertexAttribArray(1);

    _shaderProgram = CreateShaderProgram();
    GL.UseProgram(_shaderProgram);

    _texture = LoadTexture("static/images/Cat_diffuse.jpg"); // Load the texture

    // Set up matrices
    _view = Matrix4.LookAt(new Vector3(4.0f, 4.0f, 4.0f), Vector3.Zero, Vector3.UnitY);
    _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 800f / 600f, 0.1f, 100f);
    GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view"), false, ref _view);
    GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref _projection);
  }

  private static void LoadOBJ(string filePath)
  {
    using var importer = new AssimpContext();
    var scene = importer.ImportFile(filePath, PostProcessSteps.Triangulate | PostProcessSteps.GenerateUVCoords);

    foreach (var mesh in scene.Meshes)
    {
      for (int i = 0; i < mesh.Vertices.Count; i++)
      {
        var vertex = mesh.Vertices[i];
        _vertices.Add(vertex.X);
        _vertices.Add(vertex.Y);
        _vertices.Add(vertex.Z);

        var uv = mesh.TextureCoordinateChannels[0][i];
        _vertices.Add(uv.X);
        _vertices.Add(uv.Y);
      }

      foreach (var face in mesh.Faces)
      {
        foreach (var index in face.Indices)
        {
          _indices.Add((uint)index);
        }
      }
    }
  }

  private static void OnRenderFrame(FrameEventArgs e)
  {
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    _model = Matrix4.CreateScale(0.01f) * Matrix4.CreateRotationY(_rotationAngle);
    GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref _model);

    GL.ActiveTexture(TextureUnit.Texture0);
    GL.BindTexture(TextureTarget.Texture2D, _texture);

    GL.BindVertexArray(_vao);
    GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);

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

  private static int LoadTexture(string filePath)
  {
    if (!File.Exists(filePath))
      throw new FileNotFoundException("Texture file not found.", filePath);

    int texture = GL.GenTexture();
    GL.BindTexture(TextureTarget.Texture2D, texture);

    using var stream = File.OpenRead(filePath);
    var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

    GL.BindTexture(TextureTarget.Texture2D, 0);
    return texture;
  }


  private static int CreateShaderProgram()
  {
    string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec2 aTexCoord;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            out vec2 texCoord;

            void main()
            {
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
                texCoord = aTexCoord;
            }";

    string fragmentShaderSource = @"
            #version 330 core
            in vec2 texCoord;
            out vec4 fragColor;

            uniform sampler2D texture0;

            void main()
            {
                fragColor = texture(texture0, texCoord);
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
