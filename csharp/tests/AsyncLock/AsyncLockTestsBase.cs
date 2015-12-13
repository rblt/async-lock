using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rblt.Threading;

namespace rblt.Tests.Threading
{
    public class AsyncLockTestBase
    {
        public class Variable
        {
            private int _value = 0;
            public int MaxValue = 0;
            private static readonly object _syncRoot = new object();

            public virtual int Value { get { return this._value; } }

            public virtual void Inc()
            {
                lock (_syncRoot)
                {
                    int val = ++this._value;
                    if (val > this.MaxValue) this.MaxValue = val;
                }
            }

            public virtual void Dec() { this._value--; }
        }

        public static void SetupMocks()
        {
            Var = new Variable();

            SampleTaskForToken = async (token) =>
            {
                using (await AsyncLock.CreateAsync(token))
                {
                    Var.Inc();
                    await Task.Delay(5);
                    Var.Dec();
                }
            };
        }

        protected static Variable Var;
        protected static Func<object, Task> SampleTaskForToken;
    }
}
