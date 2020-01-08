using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SearchjsToExpression
{
    public class Person
    {
        public string Name { get; set; }
        public PersonDetail Detail { get; set; }
    }

    public class PersonDetail
    {
        public string Gender { get; set; }
        public int Age { get; set; }
    }

    class Program
    {
        public static Expression<Func<T, bool>> CreateExpression<T>( string propertyName, int rightValue )
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

        static void Main( string[ ] args )
        {
            string str = "{Detail.Age: 30}";
            var people = new List<Person>( )
            {
                new Person( ){ Name = "Pedro", Detail = new PersonDetail( ){ Age = 50, Gender = "M" } },
                new Person( ){ Name = "Maria", Detail = new PersonDetail( ){ Age = 30, Gender = "F" } }
            };

            var exp = CreateExpression<Person>( "Detail.Age", 30);

            //var param = LambdaExpression.Parameter( typeof( Person ), "x" );
            //var left = LambdaExpression.PropertyOrField( param, "age" );
            //var right = Expression.Constant( 30 );
            //var innerLambda = Expression.Equal( left, right );
            //var exp = Expression.Lambda<Func<Person, bool>>( innerLambda, param );

            var result = people.Where( exp.Compile( ) );

            var a = result.ToList( );

            Console.ReadKey( );
        }
    }
}
