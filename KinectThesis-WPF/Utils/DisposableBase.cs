using System;

namespace Utils
{
    /// <summary>
    /// Class to implement disposable object with there own specific dispose method.
    /// </summary>
    public abstract class DisposableBase : IDisposable
    {
        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}