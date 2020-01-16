using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SearchjsToExpression
{
    public static class ExtractJToken
    {
        private static eOrderTypes mainOrder = eOrderTypes.Null;
        private static eOrderTypes currentOrder = eOrderTypes.And;
        private static bool mainNot = false;
        private static bool currentNot = false;
        private static string comparator = "";

        private static void SetModifiers( JProperty prop, bool setMain )
        {
            if( prop.Name == "_join" )
            {
                if( prop.Value.ToString( ) == "OR" )
                    currentOrder = eOrderTypes.Or;
                else
                    currentOrder = eOrderTypes.And;

                if( mainOrder == eOrderTypes.Null && setMain )
                    mainOrder = currentOrder;
            }
            else if( prop.Name == "_not" )
            {
                if( ( bool ) prop.Value )
                {
                    if( setMain )
                        mainNot = !mainNot;
                    else
                        currentNot = !currentNot;
                }
            }
            else
            {
                if( ( bool ) prop.Value )
                    comparator = prop.Name;
            }
        }

        private static Expression<Func<T, bool>> GetFromObject<T>( JProperty prop )
        {
            //{ \"Detail.age\" : { \"from\" : 30, \"to\": 35 } }
            var auxList = new List<Expression<Func<T, bool>>>( );
            foreach( JProperty child in prop.Value.Children<JProperty>( ) )
            {
                auxList.Add( Utils.CreateExpression<T>( prop.Name, child.Value, false, child.Name ) );
            }

            return Utils.BuildAnd( mainNot, auxList.ToArray( ) );
        }

        private static Expression<Func<T, bool>> ProcessTerms<T>( JToken item )
        {
            var auxList = new List<Expression<Func<T, bool>>>( );

            currentOrder = eOrderTypes.And;
            currentNot = mainNot;
            comparator = "";

            foreach( JProperty child in item.Children<JProperty>( ).OrderBy( x => x.Name ) )
            {
                if( child.Name.StartsWith( "_" ) )
                {
                    SetModifiers( child, false );
                }
                else
                {
                    if( child.Value.Type == JTokenType.Object )
                    {
                        auxList.Add( GetFromObject<T>( child ) );
                    }
                    else
                    {
                        auxList.Add( Utils.CreateExpression<T>( child.Name, child.Value, currentNot, comparator ) );
                    }
                }
            }

            return ( currentOrder == eOrderTypes.And )
                        ? Utils.BuildAnd( auxList.ToArray( ) )
                        : Utils.BuildOrElse( auxList.ToArray( ) );
        }

        private static List<Expression<Func<T, bool>>> GetFromArray<T>( JProperty prop )
        {
            var ret = new List<Expression<Func<T, bool>>>( );
            var auxList = new List<Expression<Func<T, bool>>>( );

            foreach( var item in ( JArray ) prop.Value )
            {
                if( item.Type == JTokenType.Object ) //terms
                {
                    ret.Add( ProcessTerms<T>( item ) );
                }
                else
                {
                    auxList.Add( Utils.CreateExpression<T>( prop.Name, item, mainNot, comparator ) );
                }
            }

            if( auxList.Count > 0 ) //prop.Name != "terms"
            {
                //{ \"name\":[ \"John\", \"Joana\" ]}
                ret.Add( Utils.BuildOrElse( auxList.ToArray( ) ) );
            }

            return ret;
        }

        public static Expression<Func<T, bool>> Extract<T>( JToken node )
        {
            List<Expression<Func<T, bool>>> list = new List<Expression<Func<T, bool>>>( );
            list = ProcessJToken<T>( node );

            if( list.Count == 1 )
            {
                return list.FirstOrDefault( );
            }
            else
            {
                if( mainOrder == eOrderTypes.And || mainOrder == eOrderTypes.Null )
                {
                    return Utils.BuildAnd( list.ToArray( ) );
                }
                else
                {
                    return Utils.BuildOrElse( list.ToArray( ) );
                }
            }
        }

        private static List<Expression<Func<T, bool>>> ProcessJToken<T>( JToken node )
        {
            List<Expression<Func<T, bool>>> list = new List<Expression<Func<T, bool>>>( );

            foreach( JProperty prop in node.Children<JProperty>( ).OrderBy( x => x.Name ) )
            {
                if( prop.Value.Type == JTokenType.Object )
                {
                    list.Add( GetFromObject<T>( prop ) );
                }
                else if( prop.Value.Type == JTokenType.Array )
                {
                    list.AddRange( GetFromArray<T>( prop ) );
                }
                else
                {
                    if( prop.Name.StartsWith( "_" ) )
                    {
                        SetModifiers( prop, true );
                    }
                    else
                    {
                        list.Add( Utils.CreateExpression<T>( prop.Name, prop.Value, mainNot, comparator ) );
                    }
                }
            }

            return list;
        }
    }
}
