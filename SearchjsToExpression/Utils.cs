using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SearchjsToExpression
{
    public static class Utils
    {
        public static Expression<Func<T, bool>> CreateExpression<T>( string propertyName, object rightValue )
        {
            var param = Expression.Parameter( typeof( T ), "x" );
            Expression left = param;
            foreach( var member in propertyName.Split( '.' ) )
            {
                left = Expression.PropertyOrField( left, member );
            }

            var right = Expression.Constant( rightValue );

            var innerLambda = Expression.Equal( left, right );

            return Expression.Lambda<Func<T, bool>>( innerLambda, param );
        }

        public static Expression<T> Compose<T>( this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge )
        {
            // build parameter map (from parameters of second to parameters of first)

            var map = first.Parameters.Select( ( f, i ) => new { f, s = second.Parameters[ i ] } ).ToDictionary( p => p.s, p => p.f );

            // replace parameters in the second lambda expression with parameters from the first

            var secondBody = ParameterRebinder.ReplaceParameters( map, second.Body );

            // apply composition of lambda expression bodies to parameters from the first expression 

            return Expression.Lambda<T>( merge( first.Body, secondBody ), first.Parameters );

        }

        public static Expression<Func<T, bool>> And<T>( this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second )
        {
            return first.Compose( second, Expression.And );
        }

        public static Expression<Func<T, bool>> Or<T>( this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second )
        {
            return first.Compose( second, Expression.Or );

        }

        public static Expression<Func<T, bool>> OrElse<T>( this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second )
        {
            return first.Compose( second, Expression.Or );
        }

        public static Expression<Func<T, bool>> BuildAnd<T>( params Expression<Func<T, bool>>[ ] conditions )
        {
            return conditions.Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>( null, ( current, expression ) => 
                current == null ? expression : current.And( expression ) );
        }

        public static Expression<Func<T, bool>> BuildOr<T>( params Expression<Func<T, bool>>[ ] conditions )
        {
            return conditions.Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>( null, ( current, expression ) => 
                current == null ? expression : current.Or( expression ) );
        }

        public static Expression<Func<T, bool>> BuildOrElse<T>( params Expression<Func<T, bool>>[ ] conditions )
        {
            return conditions.Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>( null, ( current, expression ) => 
                current == null ? expression : current.OrElse( expression ) );
        }
    }
}
