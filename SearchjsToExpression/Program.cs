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
        public Car[] Cares { get; set; }
        public int[ ] Lotery { get; set; }
    }

    public class PersonDetail
    {
        public string Gender { get; set; }
        public int Age { get; set; }
        public List<Dog> Dogs { get; set; }
        public Dog[ ] Doges { get; set; }
        public int[ ] Days { get; set; }
    }

    public class Dog
    {
        public string Name { get; set; }
        public string Race { get; set; }
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
                new Person( ){ 
                    Name = "Pedro de Lara",
                    Detail = new PersonDetail( ){ 
                        Age = 50, Gender = "M",
                        Dogs = new List<Dog>( ) { new Dog( ) { Name = "Max", Race = "Hot" } },
                        Doges = new Dog[] { new Dog( ) { Name = "Max", Race = "Hot" } },
                        Days  = new int[] { 1, 2, 3 }
                    },
                    Cars = new List<Car>( ){ new Car(){ Brand = "BMW", HP = 500 } },
                    Cares = new Car[] { new Car(){ Brand = "BMW", HP = 500 } },
                    Lotery  = new int[] { 1, 2, 3 }
                },
                new Person( ){ 
                    Name = "Maria de Oliveira", 
                    Detail = new PersonDetail( ){ 
                        Age = 30, Gender = "F", 
                        Dogs = new List<Dog>( ) { new Dog( ) { Name = "M1", Race = "H1" } },
                        Doges = new Dog[] { new Dog( ) { Name = "M1", Race = "H1" } },
                        Days  = new int[] { 4, 5 }
                    },
                    Cars = new List<Car>( ){ new Car(){ Brand = "Aud", HP = 400 } },
                    Cares = new Car[] { new Car(){ Brand = "Aud", HP = 400 } },
                    Lotery  = new int[] { 4, 5 }
                },
                new Person( ){ 
                    Name = "João da silva",
                    Detail = new PersonDetail( ){ 
                        Age = 18, Gender = "B", 
                        Dogs  = new List<Dog>( ) { new Dog( ) { Name = "M2", Race = "H2" } },
                        Doges = new Dog[] { new Dog( ) { Name = "M1", Race = "H1" } },
                        Days  = new int[] { 6, 7 }
                    },
                    Cars = new List<Car>( ){ new Car(){ Brand = "Mer", HP = 300 } },
                    Cares = new Car[] { new Car(){ Brand = "Mer", HP = 300 } },
                    Lotery  = new int[] { 6, 7 }
                }
            };

            List<Expression<Func<Person, bool>>> list = new List<Expression<Func<Person, bool>>>( );

            //var a = people.Any( x => x.Detail.Dogs.Any( x => x.Name == "Max" ) );

            //list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "M" ) );
            //list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "F" ) );
            //list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "B" ) );
            //list.Add( Utils.CreateExpression<Person>( "Name", "João") );

            //var exp = Utils.BuildOrElse( list.ToArray( ) );
            //var exp = Utils.BuildAnd( list.ToArray( ) );

            //var exp2 = Utils.BuildOrElse( exp, Utils.CreateExpression<Person>( "Detail.Gender", "F" ) );

            //var exp = Utils.CreateExpression<Person>( "Cars.Brand", "BMW", "", true );
            //var exp = Utils.CreateExpression<Person>( "Detail.Dogs.Name", "Max" );
            //var exp = Utils.CreateExpression<Person>( "Detail.Dogs.Race", "H2" );
            //var exp = Utils.CreateExpression<Person>( "Cares.Brand", "BMW" );
            //var exp = Utils.CreateExpression<Person>( "Lotery", 1 );
            //var exp = Utils.CreateExpression<Person>( "Cars.HP", 400, "lte" );
            //var exp = Utils.CreateExpression<Person>( "Detail.Doges.Name", "Max" );
            //var exp = Utils.CreateExpression<Person>( "Detail.Days", 2 );
            //var exp = Utils.CreateExpression<Person>( "Name", "edr", "_text", true );
            var exp = Utils.CreateExpression<Person>( "Name", "de", "_word");

            var result = people.Where( exp );

            var ab = result.ToList( );

            Console.ReadKey( );
        }
    }
}
