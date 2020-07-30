using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ReportingServiceImplementation.Queries
{
    public abstract class Specification<T> where T : class
    {
        public abstract Expression<Func<T, bool>> IsSatisfiedBy { get; }
    }
}
