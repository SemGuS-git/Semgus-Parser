using System;

namespace Semgus.Util {
    /// <summary>
    /// One-time callback.
    /// </summary>
    internal class ActionDisposable : IDisposable {
        private bool _called = false;
        private readonly Action _action;

        public ActionDisposable(Action action) {
            this._action = action;
        }

        public void Dispose() {
            if (_called) return;
            _called = true;
            _action?.Invoke();
        }
    }
}
