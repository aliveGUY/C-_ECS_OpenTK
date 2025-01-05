using Interfaces;
using Components;

namespace Systems
{
  public class MovementSystem : ISystem
  {
    public int[] Filter => [COMPONENT.POSITION];
    private PositionComponent position = new(0, 0, 0);

    public void Update(IComponent[] components)
    {
      Cast(components);

      position.X++;
      position.Y++;
      position.Z++;

      Console.WriteLine($"Updated Position: ({position.X}, {position.Y}, {position.Z})");
    }

    public void Cast(IComponent[] components)
    {
      foreach (var component in components)
      {
        if (component is PositionComponent position)
          this.position = position;
      }
    }
  }
}