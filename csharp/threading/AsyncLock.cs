using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace rblt.Threading
{
    public class AsyncLock
    {
        #region AsyncLock

        #region Types

        private struct Releaser : IDisposable
        {
            #region Releaser members

            private readonly object _token;
            private readonly ConcurrentDictionary<object, SemaphoreSlimTracker> _semaphores;

            public Releaser(ConcurrentDictionary<object, SemaphoreSlimTracker> semaphores, object token)
            {
                this._token = token;
                this._semaphores = semaphores;
            }

            public void Dispose()
            {
                AsyncLock.Release(this._semaphores, this._token);

                GC.SuppressFinalize(this);
            }

            #endregion
        }

        private class SemaphoreSlimTracker : IDisposable
        {
            #region SemaphoreSlimTracker members

            private readonly object _syncRoot;
            private readonly SemaphoreSlim _semaphore;
            private int _refCount;

            public int ReferenceCount
            {
                get
                {
                    lock (_syncRoot)
                    {
                        return this._refCount;
                    }
                }
            }

            public SemaphoreSlimTracker()
            {
                this._semaphore = new SemaphoreSlim(1);
                this._refCount = 1;
                this._syncRoot = new object();
            }

            public void Acquire()
            {
                if (!this._semaphore.Wait(TimeSpan.FromMinutes(3)))
                    throw new TimeoutException("Possible deadlock.");
            }

            public async Task AcquireAsync()
            {
                if (!(await this._semaphore.WaitAsync(TimeSpan.FromMinutes(3))))
                    throw new TimeoutException("Possible deadlock.");
            }

            public SemaphoreSlimTracker IncRefCounter()
            {
                lock (this._syncRoot)
                {
                    this._refCount++;
                    return this;
                }
            }

            public void Release()
            {
                this._semaphore.Release();

                lock (this._syncRoot)
                {
                    this._refCount--;
                }
            }

            public void Dispose()
            {
                this._semaphore.Dispose();

                GC.SuppressFinalize(this);
            }

            #endregion
        }

        #endregion

        #region Membervariables

        private static readonly ConcurrentDictionary<object, SemaphoreSlimTracker> _g_semaphores = new ConcurrentDictionary<object, SemaphoreSlimTracker>();
        private readonly ConcurrentDictionary<object, SemaphoreSlimTracker> _semaphores;

        #endregion

        #region Internal

        internal static int GlobalCount
        {
            get { return _g_semaphores.Count; }
        }

        internal int Count
        {
            get { return this._semaphores.Count; }
        }

        #endregion

        #region Factory methods

        public static async Task<IDisposable> CreateAsync(object syncRoot)
        {
            return await CreateAsync(_g_semaphores, syncRoot);
        }

        public static IDisposable Create(object syncRoot)
        {
            return Create(_g_semaphores, syncRoot);
        }

        public AsyncLock()
        {
            this._semaphores = new ConcurrentDictionary<object, SemaphoreSlimTracker>();
        }

        public IDisposable Acquire(object syncRoot)
        {
            return Create(this._semaphores, syncRoot);
        }

        public async Task<IDisposable> AcquireAsync(object syncRoot)
        {
            return await CreateAsync(this._semaphores, syncRoot);
        }

        #endregion

        #region Helper methods

        private static async Task<IDisposable> CreateAsync(ConcurrentDictionary<object, SemaphoreSlimTracker> semaphores, object syncRoot)
        {
            var tracker = EnsureInstanceLock(semaphores, syncRoot);
            var releaser = new Releaser(semaphores, syncRoot);
            await tracker.AcquireAsync();
            return releaser;
        }

        private static IDisposable Create(ConcurrentDictionary<object, SemaphoreSlimTracker> semaphores, object syncRoot)
        {
            var tracker = EnsureInstanceLock(semaphores, syncRoot);
            var releaser = new Releaser(semaphores, syncRoot);
            tracker.Acquire();
            return releaser;
        }

        private static SemaphoreSlimTracker EnsureInstanceLock(ConcurrentDictionary<object, SemaphoreSlimTracker> semaphores, object syncRoot)
        {
            SemaphoreSlimTracker tracker = null;
            semaphores.AddOrUpdate(syncRoot,
                (_) => tracker = new SemaphoreSlimTracker(),
                (_, oldVal) => tracker = oldVal.IncRefCounter());

            return tracker;
        }

        private static void Release(ConcurrentDictionary<object, SemaphoreSlimTracker> semaphores, object token)
        {
            SemaphoreSlimTracker tracker = null;
            if (semaphores.TryGetValue(token, out tracker))
            {
                tracker.Release();
                if (tracker.ReferenceCount == 0)
                {
                    lock (tracker)
                    {
                        SemaphoreSlimTracker tmp;
                        if (tracker.ReferenceCount == 0 && semaphores.TryRemove(token, out tmp))
                            tracker.Dispose();
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
