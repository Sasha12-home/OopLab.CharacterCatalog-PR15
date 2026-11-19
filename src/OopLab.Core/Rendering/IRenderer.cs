namespace OopLab.Core.Rendering;

public interface IRenderer<in T>
{
    void Render(T item);
    void RenderCollection(IEnumerable<T> items);
}
