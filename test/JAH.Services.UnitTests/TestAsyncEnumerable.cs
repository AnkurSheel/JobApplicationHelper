using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

// https://stackoverflow.com/questions/40476233/how-to-mock-an-async-repository-with-entity-framework-core/40491640#40491640
namespace JAH.Services.UnitTests
{
    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        {
        }

        IQueryProvider IQueryable.Provider
        {
            get
            {
                return new TestAsyncQueryProvider<T>(this);
            }
        }

        public IAsyncEnumerator<T> GetEnumerator()
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
    }
}
