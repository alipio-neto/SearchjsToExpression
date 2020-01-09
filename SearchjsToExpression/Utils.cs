using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SearchjsToExpression
{
    public static class Utils
    {
        //public static Expression<Func<T, bool>> CreateExpressions<T>( string propertyName, object rightValue )
        //{
        //    IQueryable<Office> officeQuery = CurrentDataSource.Offices.AsQueryable<Office>( );
        //    ParameterExpression pe = Expression.Parameter( typeof( Office ), "Office" );
        //    ParameterExpression tpe = Expression.Parameter( typeof( Trades ), "Trades" );

        //    Expression SimpleWhere = null;
        //    Expression ComplexWhere = null;
        //    foreach( ServerSideFilterObject fo in ssfo )
        //    {
        //        SimpleWhere = null;
        //        foreach( String value in fo.FilterValues )
        //        {
        //            if( !CollectionProperties.Contains( fo.PropertyName ) )
        //            {
        //                //Handle singleton lambda logic here.
        //                Expression left = Expression.Property( pe, typeof( Office ).GetProperty( fo.PropertyName ) );
        //                Expression right = Expression.Constant( value );
        //                if( SimpleWhere == null )
        //                {
        //                    SimpleWhere = Expression.Equal( left, right );
        //                }
        //                else
        //                {
        //                    Expression e1 = Expression.Equal( left, right );
        //                    SimpleWhere = Expression.Or( SimpleWhere, e1 );
        //                }
        //            }
        //            else
        //            {
        //                Expression left = Expression.Property( tpe, typeof( Trades ).GetProperty( "Name" ) );
        //                Expression right = Expression.Constant( value );
        //                Expression InnerLambda = Expression.Equal( left, right );
        //                Expression<Func<Trades, bool>> innerFunction = Expression.Lambda<Func<Trades, bool>>( InnerLambda, tpe );

        //                method = typeof( Enumerable ).GetMethods( ).Where( m => m.Name == "Any" && m.GetParameters( ).Length == 2 ).Single( ).MakeGenericMethod( typeof( Trades ) );
        //                OuterLambda = Expression.Call( method, Expression.Property( pe, typeof( Office ).GetProperty( fo.PropertyName ) ), innerFunction );

        //                if( SimpleWhere == null )
        //                    SimpleWhere = OuterLambda;
        //                else
        //                    SimpleWhere = Expression.Or( SimpleWhere, OuterLambda );
        //            }
        //        }
        //        if( ComplexWhere == null )
        //            ComplexWhere = SimpleWhere;
        //        else
        //            ComplexWhere = Expression.And( ComplexWhere, SimpleWhere );
        //    }

        //    MethodCallExpression whereCallExpression = Expression.Call( typeof( Queryable ), "Where", new Type[ ] { officeQuery.ElementType },
        //        officeQuery.Expression, Expression.Lambda<Func<Office, bool>>( ComplexWhere, new ParameterExpression[ ] { pe } ) );
        //    results = officeQuery.Provider.CreateQuery<Office>( whereCallExpression );
            
        //}

        public static Expression<Func<T, bool>> CreateExpression<T>( string propertyName, object rightValue )
        {
            var param = Expression.Parameter( typeof( T ), "x" );

            Expression left = param;
            foreach( var member in propertyName.Split( '.' ) )
            {
                left = Expression.PropertyOrField( left, member );
            }

            var paramArray = Expression.Parameter( typeof( Car ), "b" );
            Expression outerLeft = paramArray;
            outerLeft = Expression.PropertyOrField( outerLeft, "Brand" );

            var right = Expression.Constant( rightValue );

            var innerLambda = Expression.Equal( outerLeft, right );

            Expression<Func<Car, bool>> innerFunction = Expression.Lambda<Func<Car, bool>>( innerLambda, paramArray );

            var OuterLambda = Expression.Call(
                typeof( Enumerable ), "Any", new[ ] { typeof( Car ) },
                left, innerFunction);

            return Expression.Lambda<Func<T, bool>>( OuterLambda, param );
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

        public static Expression<Func<T, bool>> OrElse<T>( this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second )
        {
            return first.Compose( second, Expression.OrElse );
        }

        public static Expression<Func<T, bool>> BuildAnd<T>( params Expression<Func<T, bool>>[ ] conditions )
        {
            return conditions.Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>( null, ( current, expression ) => 
                current == null ? expression : current.And( expression ) );
        }

        public static Expression<Func<T, bool>> BuildOrElse<T>( params Expression<Func<T, bool>>[ ] conditions )
        {
            return conditions.Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>( null, ( current, expression ) => 
                current == null ? expression : current.OrElse( expression ) );
        }
    }
}
