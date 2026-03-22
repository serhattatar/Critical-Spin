namespace CriticalSpin.Core
{
    /// <summary>
    /// My base interface for all UI Panels. It keeps the architecture clean!
    /// </summary>
    public interface IView
    {
        void Initialize();
        void Show();
        void Hide();
    }
}
