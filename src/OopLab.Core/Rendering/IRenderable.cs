namespace OopLab.Core.Rendering;

public interface IRenderable<T>
{
    void UseRenderer(IRenderer<T> renderer);
    void Render(IRenderer<T> renderer);
    void Render();
}
