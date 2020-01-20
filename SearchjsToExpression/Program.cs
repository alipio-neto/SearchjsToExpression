using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SearchjsToExpression
{
    public static class Help
    {
        public static IEnumerable<TSource> WhereHelp<TSource>( this IEnumerable<TSource> list, Expression<Func<TSource, bool>> action = null )
        {
            LambdaExpression lambda = action;

            return list.Where( action.Compile( ) );
        }
    }

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
        public Car PersonCar { get; set; }
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
                    Cars = null, //new List<Car>( ){ new Car(){ Brand = "BMW", HP = 500 } },
                    Cares = new Car[] { new Car(){ Brand = "BMW", HP = 500 } },
                    Lotery  = new int[] { 1, 2, 3 }
                },
                new Person( ){
                    Name = "Maria de Oliveira",
                    Detail = null,
                    //new PersonDetail( ){ 
                    //    Age = 30, Gender = "F", 
                    //    Dogs = new List<Dog>( ) { new Dog( ) { Name = "M1", Race = "H1" } },
                    //    Doges = new Dog[] { new Dog( ) { Name = "M1", Race = "H1" } },
                    //    Days  = new int[] { 4, 5 }
                    //},
                    Cars = new List<Car>( ){ new Car(){ Brand = "Aud", HP = 400 } },
                    Cares = new Car[] { new Car(){ Brand = "Aud", HP = 400 } },
                    Lotery  = new int[] { 4, 5 }
                },
                new Person( ){ 
                    Name = "Joana",
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

            //string json = "{ \"Cars.HP\" : 300 }";
            string json = "{ \"Lotery\" : 3 }";
            //string json = "{ \"Detail.Age\" : 18 }";
            //string json = "{ \"name\":[ \"John\", \"Joana\" ]}";
            //string json = "{ \"deTail.doGs.racE\": \"H2\" }";
            //string json = "{ \"Detail.age\" : { \"from\" : 30, \"to\": 35 } }";
            //string json = "{ \"_not\": true, \"name\":[ \"John\", \"Joana\" ]}";
            //string json = "{ \"name\":\"Joan\", \"_start\": true }";
            //string json = "{ \"name\":\"de\", \"_word\": true }";
            //string json = "{ \"name\":\"de\", \"_word\": false }";

            //string json = "{ \"_not\": true, \"Detail.age\" : { \"from\" : 30, \"to\": 35 } }";
            //string json = "{ \"name\": \"Joana\",\"detail.age\": 30,\"_join\": \"OR\"}";
            //string json = " { \"name\": \"Joana\", \"Detail.age\" : { \"from\" : 25 , \"to\" : 40 }, \"_join\": \"OR\" }";

            //string json = "{ \"terms\" :[ { \"name\": \"Joana\", \"Detail.age\": 30},{ \"name\": \"Jill\",\"Detail.age\": 18 } ], \"_join\": \"OR\" }";

            //string json = "{ \"terms\" :[ { \"name\": \"Joana\", \"Detail.age\": 30, \"_join\": \"OR\"},{ \"name\": \"Jill\",\"Detail.age\": 18, \"_join\": \"OR\" } ], \"_join\": \"AND\" }";
            //string json = "{ \"terms\" :[ { \"name\": \"Joana\", \"Detail.age\": 30, \"_not\": true } ], \"_not\": true }";

            //string json = "{ \"terms\" :[ { \"name\": \"Joana\", \"Detail.age\": 30} ], \"_not\": true }";
            //string json = "{ \"terms\" :[ { \"name\": \"Joana\", \"Detail.age\" : { \"from\" : 30, \"to\": 35 } } ], \"_not\": true }";

            //string json = "{ \"terms\" :[ { \"name\": \"Joana\"}, {\"Detail.age\" : { \"from\" : 30, \"to\": 35 }, \"_not\": true } ], \"_not\": true }";
            //string json = "{ \"terms\" :[ { \"name\": \"Joana\", \"_not\": true}, {\"Detail.age\" : { \"from\" : 30, \"to\": 35 } } ], \"_not\": true }";

            //string json = "{ \"terms\" :[ { \"name\": \"Joana\", \"Detail.age\": 30, \"_not\": true }, { \"name\": \"Jill\", \"Detail.age\": 18 } ] }";
            //string json = "{ \"terms\" :[ { \"name\":\"Lara\", \"_text\": true }, { \"name\": \"Joana\" } ]}";
            //string json = "{ \"_text\": true, \"terms\" :[ { \"name\":\"Lara\" }, { \"name\": \"Joana\" } ]}";

            //var a = JsonConvert.SerializeObject( people );
            //var b = JsonConvert.DeserializeObject<JArray>( a );

            JToken node = JToken.Parse( json );
            var exp = ExtractJToken.Extract<Person>( node );
            //var exp = ExtractJToken.Extract<JToken>( node );

            //var result = people.Where( x => x.Detail?.Age == 30 );
            //var result1 = people.WhereHelp( x => x.Detail == null ? x.Detail.Age == 30 : false);
            //var result = people.WhereHelp( x => (x.Detail ?? new PersonDetail()).Age == 30 );
            //var result = people.WhereHelp( x => x.Detail != null && x.Detail.Age == 30 );
            //var result = people.Where( x => x.Detail.NullSafeEval( x => x.PersonCar.HP, 30) == 30 );

            var result = people.Where( exp.Compile( ) );
            //var result = people.Where( x => x.Cars != null && x.Cars.Any(x => x.HP == 300 ) );

            //var result = b.Where( a => a[ "Name" ].ToString() == "Joana" );
            //var result = b.Where( a => a.Value<string>( "Name" ) == "Joana" );
            //var result = b.WhereHelp( a => a[ "Detail" ].Value<JObject>( ).Value<int>( "Age" ) == 30);
            //var result = b.WhereHelp( a => a.Value<JObject>( "Detail" ).Value<int>( "Age" ) == 30 );
            //var result = b.WhereHelp( a => a[ "Detail" ].Value<JObject>( )[ "Age" ].Value<int> == 30 );

            var ab = result.ToList( );

            //Console.ReadKey( );
        }
    }
}
