namespace CriticalSpin.Core
{
    /// <summary>
    /// This is my base interface for all Logic scripts in my MVP architecture.
    /// </summary>
    public interface IPresenter
    {
        void Initialize();
        void Dispose();
    }
}
