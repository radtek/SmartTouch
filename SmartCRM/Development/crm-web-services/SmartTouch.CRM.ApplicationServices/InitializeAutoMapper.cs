using AutoMapper;
using SmartTouch.CRM.ApplicationServices.ObjectMappers;

namespace SmartTouch.CRM.ApplicationServices
{
    public static class InitializeAutoMapper
    {
        public static void Initialize()
        {
            Mapper.Initialize(x =>
        {
                x.AddProfile<ViewModelToEntityProfile>();
                x.AddProfile<EntityToViewModelProfile>();
                x.AddProfile<EntityToDbProfile>();
                x.AddProfile<DbToEntityProfile>();
                  });
        }
    }

    public static class MappingExpressionExtensions
    {
        public static IMappingExpression<TSource, TDest> IgnoreAllUnmapped<TSource, TDest>(this IMappingExpression<TSource, TDest> expression)
        {
            expression.ForAllMembers(opt => opt.Ignore());
            return expression;
        }
    }
}