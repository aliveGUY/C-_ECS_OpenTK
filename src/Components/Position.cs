using Interfaces;

namespace Components
{
  public class PositionComponent : IComponent
  {
    public float X, Y, Z;

    public PositionComponent(float x, float y, float z)
    {
      X = x;
      Y = y;
      Z = z;
    }
  }
}