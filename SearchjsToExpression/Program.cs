using Newtonsoft.Json.Linq;
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

    public enum eOrderTypes
    {
        Null,
        And,
        Or
    }

    class Program
    {
        static void Main( string[ ] args )
        {
            eOrderTypes mainOrder = eOrderTypes.Null;
            eOrderTypes currentOrder = eOrderTypes.And;

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

            //string json = "{ \"Detail.Age\" : 30 }";
            //string json = "{ \"name\":[ \"John\", \"Joana\" ]}";
            //string json = "{ \"detail.dogs.race\": \"H2\" }";
            //string json = "{ \"Detail.age\" : { \"from\" : 30 , \"to\" : 80 } }";
            //string json = "{ \"name\": \"Joana\",\"detail.age\": 30,\"_join\": \"OR\"}";
            //string json = " { \"name\": \"Joana\", \"Detail.age\" : { \"from\" : 25 , \"to\" : 40 }, \"_join\": \"OR\" }";
            //string json = "{ \"terms\" :[ { \"name\": \"Joana\", \"Detail.age\": 30},{ \"name\": \"Jill\",\"Detail.age\": 18 } ], \"_join\": \"OR\" }";
            //string json = "{ \"terms\" :[ { \"name\": \"Joana\", \"Detail.age\": 30, \"_join\": \"OR\"},{ \"name\": \"Jill\",\"Detail.age\": 18, \"_join\": \"OR\" } ], \"_join\": \"AND\" }";
            string json = "{ \"terms\" :[ { \"name\": \"Joana\", \"Detail.age\": 30},{ \"name\": \"Jill\",\"Detail.age\": 18, \"_join\": \"OR\" } ], \"_join\": \"AND\" }";

            Expression<Func<Person, bool>> exp = null;
            List<Expression<Func<Person, bool>>> list = new List<Expression<Func<Person, bool>>>( );
            JToken node = JToken.Parse( json );

            foreach( JProperty prop in node.Children<JProperty>( ).OrderBy( x => x.Name ) )
            {
                if( prop.Value.Type == JTokenType.Object )
                {
                    var auxList = new List<Expression<Func<Person, bool>>>( );
                    foreach( JProperty child in prop.Value.Children<JProperty>( ) )
                    {
                        auxList.Add( Utils.CreateExpression<Person>( prop.Name, child.Value, child.Name ) );
                    }

                    list.Add( Utils.BuildAnd( auxList.ToArray( ) ) );
                }
                else if( prop.Value.Type == JTokenType.Array )
                {
                    var auxList = new List<Expression<Func<Person, bool>>>( );
                    foreach( var item in ( JArray ) prop.Value )
                    {
                        if( item.Type == JTokenType.Object )
                        {
                            currentOrder = eOrderTypes.And;

                            foreach( JProperty child in item.Children<JProperty>( ).OrderBy( x => x.Name ) )
                            {
                                if( child.Name == "_join" )
                                {
                                    if( child.Value.ToString( ) == "OR" )
                                        currentOrder = eOrderTypes.Or;
                                }
                                else
                                {
                                    auxList.Add( Utils.CreateExpression<Person>( child.Name, child.Value ) );
                                }
                            }

                            list.Add( currentOrder == eOrderTypes.And ?  Utils.BuildAnd( auxList.ToArray( ) ) 
                                : Utils.BuildOrElse( auxList.ToArray( ) ) );
                            auxList = new List<Expression<Func<Person, bool>>>( );
                        }
                        else
                        {
                            auxList.Add( Utils.CreateExpression<Person>( prop.Name, item ) );
                        }
                    }

                    if( prop.Name != "terms" )
                    {
                        list.Add( Utils.BuildOrElse( auxList.ToArray( ) ) );
                    }
                }
                else
                {
                    if( prop.Name.StartsWith( "_" ) )
                    {
                        if( prop.Name == "_join" )
                        {
                            if( prop.Value.ToString( ) == "OR" )
                                currentOrder = eOrderTypes.Or;
                            else
                                currentOrder = eOrderTypes.And;

                            if (mainOrder == eOrderTypes.Null)
                                mainOrder = currentOrder;
                        }
                    }
                    else
                    {
                        list.Add( Utils.CreateExpression<Person>( prop.Name, prop.Value ) );
                    }
                }
            }

            if( list.Count == 1 )
            {
                exp = list.FirstOrDefault( );
            } 
            else
            {
                if( mainOrder == eOrderTypes.And )
                {
                    exp = Utils.BuildAnd( list.ToArray( ) );
                } 
                else
                {
                    exp = Utils.BuildOrElse( list.ToArray( ) );
                }
            }

            #region coments
            //List<Expression<Func<Person, bool>>> list = new List<Expression<Func<Person, bool>>>( );
            //list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "M" ) );
            //list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "F" ) );
            //list.Add( Utils.CreateExpression<Person>( "Detail.Gender", "B" ) );
            //list.Add( Utils.CreateExpression<Person>( "Name", "João") );

            //exp = Utils.BuildOrElse( list.ToArray( ) );
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
            //var exp = Utils.CreateExpression<Person>( "Name", "de", "_word");
            //var exp = Utils.CreateExpression<Person>( "cars.brand", "BMW" );
            //var exp = Utils.CreateExpression<Person>( "detail.dogs.race", "H2" );
            #endregion

            var result = people.Where( exp.Compile() );
            var ab = result.ToList( );

            //Console.ReadKey( );
        }
    }
}
