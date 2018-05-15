using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JAH.Services.UnitTests
{
    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current
        {
            get
            {
                return _inner.Current;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inner.MoveNext());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner?.Dispose();
            }
        }
    }
}
