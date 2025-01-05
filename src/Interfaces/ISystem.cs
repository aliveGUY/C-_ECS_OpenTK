
namespace Interfaces
{
  public interface ISystem
  {
    int[] Filter { get; }
    void Update(IComponent[] components);
    void Cast(IComponent[] components);
  }
}