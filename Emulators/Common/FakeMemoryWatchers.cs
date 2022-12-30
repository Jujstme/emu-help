using System;
using System.Collections.Generic;

namespace LiveSplit.EMUHELP
{
    public class FakeMemoryWatcherList : List<FakeMemoryWatcher>
    {
        public void UpdateAll()
        {
            foreach (var watcher in this)
                watcher.Update();
        }
    }

    public abstract class FakeMemoryWatcher
    {
        protected readonly object _func;
        public string Name { get; set; }
        public object Current { get; protected set; }
        public object Old { get; protected set; }
        public bool Changed { get; protected set; }
        public abstract void Update();
    }

    public class FakeMemoryWatcher<T> : FakeMemoryWatcher
    {
        protected readonly new Func<T> _func = null;
        public new T Current { get; protected set; } = default;
        public new T Old { get; protected set; } = default;

        /// <summary>
        /// Create a new FakeMemoryWatcher object with default values for both .Old and .Current
        /// </summary>
        public FakeMemoryWatcher() { }

        /// <summary>
        /// Create a new FakeMemoryWatcher object and set a function to
        /// automatically get the current value when calling Update()
        /// </summary>
        public FakeMemoryWatcher(Func<T> Func)
        {
            this._func = Func;
        }

        /// <summary>
        /// Moves .Current to .Old and runs a previously defined Func to get the new .Current value
        /// </summary>
        public override void Update()
        {
            Old = Current;

            if (_func != null)
                Current = _func.Invoke();

            Changed = !Old.Equals(Current);
        }

        /// <summary>
        /// Moves .Current to .Old and manually sets a new value for .Current
        /// </summary>
        public void Update(T newValue)
        {
            Old = Current;
            Current = newValue;

            Changed = !Old.Equals(Current);
        }
    }
}