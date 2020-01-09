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
        public List<Car> Cars { get; set; }
    }

    public class PersonDetail
    {
        public string Gender { get; set; }
        public int Age { get; set; }
    }

    public class Car
    {
        public string Brand { get; set; }
        public int HP { get; set; }
    }

    class Program
    {
        static void Main( string[ ] args )
        {
            string str = "{Detail.Age: 30}";

            var people = new List<Person>( )
            {
                new Person( ){ Name = "Pedro", Detail = new PersonDetail( ){ Age = 50, Gender = "M" },
                    Cars = new List<Car>( ){ new Car(){ Brand = "BMW", HP = 500 } } },
                new Person( ){ Name = "Maria", Detail = new PersonDetail( ){ Age = 30, Gender = "F" },
                    Cars = new List<Car>( ){ new Car(){ Brand = "Aud", HP = 400 } } },
                new Person( ){ Name = "João", Detail = new PersonDetail( ){ Age = 18, Gender = "B" },
                    Cars = new List<Car>( ){ new Car(){ Brand = "Mer", HP = 300 } } },
            };

            List<Expression<Func<Person, bool>>> list = new List<Expression<Func<Person, bool>>>( );

            //list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "M" ) );
            //list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "F" ) );
            //list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "B" ) );
            //list.Add( Utils.CreateExpression<Person>( "Name", "João") );

            //var exp = Utils.BuildOrElse( list.ToArray( ) );
            //var exp = Utils.BuildAnd( list.ToArray( ) );

            //var exp2 = Utils.BuildOrElse( exp, Utils.CreateExpression<Person>( "Detail.Gender", "F" ) );

            var exp = Utils.CreateExpression<Person>( "Cars", "BMW" );

            var result = people.Where( exp.Compile( ) );

            var a = result.ToList( );

            Console.ReadKey( );
        }
    }
}