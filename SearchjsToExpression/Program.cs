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
        static void Main( string[ ] args )
        {
            string str = "{Detail.Age: 30}";

            var people = new List<Person>( )
            {
                new Person( ){ Name = "Pedro", Detail = new PersonDetail( ){ Age = 50, Gender = "M" } },
                new Person( ){ Name = "Maria", Detail = new PersonDetail( ){ Age = 30, Gender = "F" } },
                new Person( ){ Name = "João", Detail = new PersonDetail( ){ Age = 18, Gender = "B" } }
            };

            List<Expression<Func<Person, bool>>> list = new List<Expression<Func<Person, bool>>>( );

            //list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "M" ) );
            //list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "F" ) );
            list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "B" ) );
            list.Add( Utils.CreateExpression<Person>( "Name", "João") );

            //var exp = Utils.BuildOrElse( list.ToArray( ) );
            var exp = Utils.BuildAnd( list.ToArray( ) );

            var exp2 = Utils.BuildOrElse( exp, Utils.CreateExpression<Person>( "Detail.Gender", "F" ) );

            var result = people.Where( exp.Compile( ) );

            var a = result.ToList( );

            Console.ReadKey( );
        }
    }
}