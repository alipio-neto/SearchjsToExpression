using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SearchjsToExpression
{
    public static class Utils
    {
        public static readonly List<string> comparators = new List<string>( ) { "from", "to", "gte", "gt", "lte", "lt" };

        public static Expression Compare ( Expression left, object rightValue, Type rightType, string comparator = "" )
        {
            var convertedObject = Convert.ChangeType( rightValue, rightType );
            var right = Expression.Constant( convertedObject );

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
                case "_word":
                    var regex = new Regex( string.Format( @"\b{0}\b", Regex.Escape( rightValue.ToString( ) ) ), RegexOptions.Compiled );
                    var constRegex = Expression.Constant( regex );

                    var methodInfo = typeof( Regex ).GetMethod( "IsMatch", new Type[ ] { typeof( string ) } );
                    var paramsEx = new Expression[ ] { left };

                    return Expression.Call( constRegex, methodInfo, paramsEx );
                case "_start":
                    return Expression.Call( left, typeof( string ).GetMethod( "StartsWith", new[ ] { typeof( string ) } ), right );
                case "_end":
                    return Expression.Call( left, typeof( string ).GetMethod( "EndsWith", new[ ] { typeof( string ) } ), right );
                default:
                    break;
            }

            return null;
        }

        public static Expression<Func<T, bool>> CreateExpression<T>( string propertyName, object rightValue, bool not, string comparator )
        {
            var exp = BuildExpression( propertyName, rightValue, not, typeof( T ), comparator );
            return ( Expression<Func<T, bool>> ) exp;
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
                    property = type.GetProperty( member, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance );
                }
                else
                {
                    property = property.PropertyType.GetProperty( member, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance );
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
                var innerLambda = Compare( left, rightValue, property.PropertyType, comparator );

                if( not )
                    innerLambda = Expression.Not( innerLambda );

                return Expression.Lambda( innerLambda, param );
            }
        }

        public static Expression<T> Compose<T>( this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge, bool not )
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select( ( f, i ) => new { f, s = second.Parameters[ i ] } ).ToDictionary( p => p.s, p => p.f );

            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters( map, second.Body );

            var merged = merge( first.Body, secondBody );

            if( not )
                merged = Expression.Not( merged );

            // apply composition of lambda expression bodies to parameters from the first expression 
            return Expression.Lambda<T>( merged, first.Parameters );
        }

        public static Expression<Func<T, bool>> And<T>( this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second, bool not )
        {
            return first.Compose( second, Expression.And, not );
        }

        public static Expression<Func<T, bool>> OrElse<T>( this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second, bool not )
        {
            return first.Compose( second, Expression.OrElse, not );
        }

        public static Expression<Func<T, bool>> BuildAnd<T>( bool not, params Expression<Func<T, bool>>[ ] conditions )
        {
            return conditions.Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>( null, ( current, expression ) => 
                current == null ? expression : current.And( expression, not ) );
        }

        public static Expression<Func<T, bool>> BuildAnd<T>( params Expression<Func<T, bool>>[ ] conditions )
        {
            return BuildAnd( false, conditions );
        }

        public static Expression<Func<T, bool>> BuildOrElse<T>( bool not, params Expression<Func<T, bool>>[ ] conditions )
        {
            return conditions.Aggregate<Expression<Func<T, bool>>, Expression<Func<T, bool>>>( null, ( current, expression ) =>
                current == null ? expression : current.OrElse( expression, not ) );
        }

        public static Expression<Func<T, bool>> BuildOrElse<T>( params Expression<Func<T, bool>>[ ] conditions )
        {
            return BuildOrElse( false, conditions );
        }
    }
}
