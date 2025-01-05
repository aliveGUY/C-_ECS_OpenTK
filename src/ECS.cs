
using Components;
using Interfaces;
using Systems;

public class ECS
{
  private readonly Dictionary<int, IComponent[]> _entities = [];
  private readonly Dictionary<int, int> _entityMasks = [];
  private readonly ISystem[] _systems = [new MovementSystem()];

  public void Init()
  {
    _entities[1] = [new PositionComponent(0, 0, 0)];
    _entityMasks[1] = 0;
    _entityMasks[1] |= COMPONENT.POSITION;

    _entities[2] = [new PositionComponent(1, 1, 1)];
    _entityMasks[2] = 0;
    _entityMasks[2] |= COMPONENT.POSITION;
  }

  public void Update()
  {
    foreach (var system in _systems)
    {
      foreach (var entityMask in _entityMasks)
      {
        if (!MatchesFilter(system.Filter, entityMask.Value)) continue;
        if (!_entities.TryGetValue(entityMask.Key, out IComponent[]? components)) continue;

        system.Update(components);
      }
    }
  }

  private static bool MatchesFilter(int[] filter, int entityMask)
  {
    foreach (var component in filter)
    {
      if ((component & entityMask) == 0)
        return false;
    }
    return true;
  }
}