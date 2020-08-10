using Common.OMS.OutageDatabaseModel;
using OMS.HistoryDBManagerServiceImplementation.Reporting.Queries.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerServiceImplementation.Reporting.Queries
{
    public class OutageElementIdQuery : OutageSpecification
    {
        private readonly long _id;

        public OutageElementIdQuery(long id)
            => _id = id;

        public override Expression<Func<OutageEntity, bool>> IsSatisfiedBy => x => x.OutageElementGid == _id;

    }
}
