using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SearchjsToExpression
{
    public static class Utils
    {
        public static Expression Compare ( Expression left, Expression right, string comparator = "" )
        {
            switch( comparator )
            {
                case "":
                    return Expression.Equal(left, right);
                case "from":
                    return Expression.GreaterThanOrEqual( left, right ); ;
                case "to":
                    return Expression.LessThanOrEqual( left, right ); ;
                case "gte":
                    return Expression.GreaterThanOrEqual( left, right ); ;
                case "lte":
                    return Expression.LessThanOrEqual( left, right ); ;
                case "gt":
                    return Expression.GreaterThan( left, right ); ;
                case "lt":
                    return Expression.LessThan( left, right ); ;
                case "_text":
                    return Expression.Call( left, typeof( string ).GetMethod( "Contains", new[ ] { typeof( string ) } ), right );
                case "_start":
                    return Expression.Call( left, typeof( string ).GetMethod( "StartsWith", new[ ] { typeof( string ) } ), right );
                case "_end":
                    return Expression.Call( left, typeof( string ).GetMethod( "EndsWith", new[ ] { typeof( string ) } ), right );
                default:
                    break;
            }

            return null;
        }

        public static Func<T, bool> CreateExpression<T>( string propertyName, object rightValue, string comparator = "", bool not = false )
        {
            var exp = BuildExpression( propertyName, rightValue, not, typeof( T ), comparator );
            return ( Func<T, bool> ) exp.Compile( );
        }

        public static LambdaExpression BuildExpression ( string propertyName, object rightValue, bool not, Type type, string comparator )
        {
            var param = Expression.Parameter( type, "x" );
            var isCollection = false;
            PropertyInfo property = null;
            Expression left = param;
            string auxPropertyName = "";

            foreach( var member in propertyName.Split( '.' ) )
            {
                if( property == null )
                {
                    property = type.GetProperty( member );
                }
                else
                {
                    property = property.PropertyType.GetProperty( member );
                }

                if( property != null )
                {
                    if( property.PropertyType.GetInterfaces( ).Any( x =>
                    x.IsGenericType && x.GetGenericTypeDefinition( ) == typeof( ICollection<> ) ) )
                    {
                        left = Expression.PropertyOrField( left, member );
                        int length = propertyName.IndexOf( member ) + member.Length;
                        length = ( length + 1 ) > propertyName.Length ? member.Length : length + 1;
                        auxPropertyName = propertyName.Substring( length );
                        isCollection = true;
                        break;
                    }
                    else
                    {
                        left = Expression.PropertyOrField( left, member );
                    }
                }
            }

            if( isCollection )
            {
                Type typeFromList = property.PropertyType.IsArray ? property.PropertyType.GetElementType( )
                    : property.PropertyType.GetGenericArguments( )[ 0 ];

                var innerFunction = BuildExpression( auxPropertyName, rightValue, not, typeFromList, comparator );
                var OuterLambda = Expression.Call( typeof( Enumerable ), "Any", new[ ] { typeFromList }, left, innerFunction );

                return Expression.Lambda( OuterLambda, param );
            }
            else
            {
                var right = Expression.Constant( rightValue );

                var innerLambda = Compare( left, right, comparator );

                if ( not )
                    innerLambda = Expression.Not( innerLambda );

                return Expression.Lambda( innerLambda, param );
            }
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
