using System.Collections.Generic;
using System;

namespace DevExpress
{
    internal class DelayedExecutor
    {
        private readonly IList<Action> _actionsToExecuteWhenLoaded = new List<Action>();
        private readonly Func<bool> _delayUntil;

        public DelayedExecutor(Func<bool> delayUntil)
        {
            _delayUntil = delayUntil;
        }

        public void ExecuteWhenReady(Action action)
        {
            if (_delayUntil())
            {
                action();

                return;
            }

            _actionsToExecuteWhenLoaded.Add(action);
        }

        public void ExecuteWaitingActions()
        {
            if (!_delayUntil())
                return;

            foreach (var action in _actionsToExecuteWhenLoaded)
            {
                action();
            }
            _actionsToExecuteWhenLoaded.Clear();
        }

        public bool IsReady()
        {
            return _delayUntil();
        }
    }
}
