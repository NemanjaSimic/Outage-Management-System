namespace OMS.Web.Common.Mappers
{
    using OMS.Web.UI.Models;

    public interface IMapper<TIn, TOut>
        where TIn: class
        where TOut: class, IViewModel 
    {
        TOut Map(TIn input);
    }
}
