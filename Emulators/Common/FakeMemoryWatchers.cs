using System;

namespace LiveSplit.EMUHELP
{
    public class FakeMemoryWatcher<T>
    {
        protected readonly Func<T> func = null;

        public T Current { get; protected set; } = default;
        public T Old { get; protected set; } = default;
        public bool Changed => !Old.Equals(Current);
        public string Name { get; set; }

        /// <summary>
        /// Create a new FakeMemoryWatcher object with default values for both .Old and .Current
        /// </summary>
        public FakeMemoryWatcher() { }

        /// <summary>
        /// Create a new FakeMemoryWatcher object and set a function to
        /// automatically get the current value when calling Update()
        /// </summary>
        /// <param name="func"></param>
        public FakeMemoryWatcher(Func<T> func)
        {
            this.func = func;
        }

        /// <summary>
        /// Moves .Current to .Old and runs a previously defined Func to get the new .Current value
        /// </summary>
        public void Update()
        {
            Old = Current;

            if (func != null)
                Current = func.Invoke();
        }

        /// <summary>
        /// Moves .Current to .Old and manually sets a new value for .Current
        /// </summary>
        public void Update(T newValue)
        {
            Old = Current;
            Current = newValue;
        }
    }
}