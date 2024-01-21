using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveSplit.EMUHELP
{
    public class FakeMemoryWatcherList : List<FakeMemoryWatcher>
    {
        public void UpdateAll()
        {
            foreach (var watcher in this)
                watcher.Update();
        }

        public void ResetAll()
        {
            foreach (var watcher in this)
                watcher.Reset();
        }

        public FakeMemoryWatcher this[string index] => this.First(w => w.Name == index);
    }

    public abstract class FakeMemoryWatcher
    {
        protected object _func;
        public string Name { get; set; }
        public object Current { get; protected set; } = default;
        public object Old { get; protected set; } = default;
        public bool Changed { get; protected set; } = default;
        public abstract void Update();
        public abstract void Reset();
        public void SetFunc(object func)
        {
            _func = func;
        }
    }

    public class FakeMemoryWatcher<T> : FakeMemoryWatcher
    {
        public new T Current { get => (T)base.Current ?? default; protected set => base.Current = value; }
        public new T Old { get => (T)base.Old ?? default; protected set => base.Old = value; }

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
            base._func = Func;
        }

        /// <summary>
        /// Moves .Current to .Old and runs a previously defined Func to get the new .Current value
        /// </summary>
        public override void Update()
        {
            base.Old = base.Current;

            if (base._func != null)
                base.Current = ((Func<T>)base._func).Invoke();

            Changed = base.Old != null && !Old.Equals(Current);
        }

        /// <summary>
        /// Moves .Current to .Old and manually sets a new value for .Current
        /// </summary>
        public void Update(T newValue)
        {
            base.Old = base.Current;
            base.Current = newValue;

            Changed = base.Old == null ? false : !Old.Equals(Current);
        }

        /// <summary>
        /// Resets the values stored in current FakeMemoryWatcher, including the
        /// associated Func&lt;<typeparamref name="T"/>&gt; if defined.
        /// </summary>
        public override void Reset()
        {
            base.Current = default;
            base.Old = default;
            base.Changed = default;
            base._func = null;
        }

        /// <summary>
        /// Sets a new Func&lt;<typeparamref name="T"/>&gt; to be invoked when calling Update().
        /// </summary>
        /// <param name="func"></param>
        public void SetFunc(Func<T> func) => base.SetFunc(func);
    }
}