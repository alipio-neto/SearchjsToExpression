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
        private enum eOrderTypes
        {
            Null,
            And,
            Or
        }

        private class Modifiers
        {
            public eOrderTypes mainOrder { get; set; }
            public eOrderTypes currentOrder { get; set; }
            public bool mainNot { get; set; }
            public bool currentNot { get; set; }
            public string comparator { get; set; }
        }

        private static void SetModifiers( JProperty prop, bool setMain, ref Modifiers mf )
        {
            if( prop.Name == "_join" )
            {
                if( prop.Value.ToString( ) == "OR" )
                    mf.currentOrder = eOrderTypes.Or;
                else
                    mf.currentOrder = eOrderTypes.And;

                if( mf.mainOrder == eOrderTypes.Null && setMain )
                    mf.mainOrder = mf.currentOrder;
            }
            else if( prop.Name == "_not" )
            {
                if( ( bool ) prop.Value )
                {
                    if( setMain )
                        mf.mainNot = !mf.mainNot;
                    else
                        mf.currentNot = !mf.currentNot;
                }
            }
            else
            {
                if( ( bool ) prop.Value )
                    mf.comparator = prop.Name;
            }
        }

        private static Expression<Func<T, bool>> GetFromObject<T>( JProperty prop, Modifiers mf )
        {
            //{ \"Detail.age\" : { \"from\" : 30, \"to\": 35 } }
            var auxList = new List<Expression<Func<T, bool>>>( );
            foreach( JProperty child in prop.Value.Children<JProperty>( ) )
            {
                auxList.Add( Utils.CreateExpression<T>( prop.Name, child.Value, false, child.Name ) );
            }

            return Utils.BuildAnd( mf.mainNot, auxList.ToArray( ) );
        }

        private static Expression<Func<T, bool>> ProcessTerms<T>( JToken item, ref Modifiers mf )
        {
            var auxList = new List<Expression<Func<T, bool>>>( );

            mf.currentOrder = eOrderTypes.And;
            mf.currentNot = mf.mainNot;
            mf.comparator = "";

            foreach( JProperty child in item.Children<JProperty>( ).OrderBy( x => x.Name ) )
            {
                if( child.Name.StartsWith( "_" ) )
                {
                    SetModifiers( child, false, ref mf );
                }
                else
                {
                    if( child.Value.Type == JTokenType.Object )
                    {
                        auxList.Add( GetFromObject<T>( child, mf ) );
                    }
                    else
                    {
                        auxList.Add( Utils.CreateExpression<T>( child.Name, child.Value, mf.currentNot, mf.comparator ) );
                    }
                }
            }

            return ( mf.currentOrder == eOrderTypes.And )
                        ? Utils.BuildAnd( auxList.ToArray( ) )
                        : Utils.BuildOrElse( auxList.ToArray( ) );
        }

        private static List<Expression<Func<T, bool>>> GetFromArray<T>( JProperty prop, ref Modifiers mf )
        {
            var ret = new List<Expression<Func<T, bool>>>( );
            var auxList = new List<Expression<Func<T, bool>>>( );

            foreach( var item in ( JArray ) prop.Value )
            {
                if( item.Type == JTokenType.Object ) //terms
                {
                    ret.Add( ProcessTerms<T>( item, ref mf ) );
                }
                else
                {
                    auxList.Add( Utils.CreateExpression<T>( prop.Name, item, mf.mainNot, mf.comparator ) );
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
            var mf = new Modifiers( )
            {
                mainOrder = eOrderTypes.Null,
                currentOrder = eOrderTypes.And,
                mainNot = false,
                currentNot = false,
                comparator = ""
            };

            List<Expression<Func<T, bool>>> list = new List<Expression<Func<T, bool>>>( );
            list = ProcessJToken<T>( node, ref mf );

            if( list.Count == 1 )
            {
                return list.FirstOrDefault( );
            }
            else
            {
                if( mf.mainOrder == eOrderTypes.And || mf.mainOrder == eOrderTypes.Null )
                {
                    return Utils.BuildAnd( list.ToArray( ) );
                }
                else
                {
                    return Utils.BuildOrElse( list.ToArray( ) );
                }
            }
        }

        private static List<Expression<Func<T, bool>>> ProcessJToken<T>( JToken node, ref Modifiers mf )
        {
            List<Expression<Func<T, bool>>> list = new List<Expression<Func<T, bool>>>( );

            foreach( JProperty prop in node.Children<JProperty>( ).OrderBy( x => x.Name ) )
            {
                if( prop.Value.Type == JTokenType.Object )
                {
                    list.Add( GetFromObject<T>( prop, mf ) );
                }
                else if( prop.Value.Type == JTokenType.Array )
                {
                    list.AddRange( GetFromArray<T>( prop, ref mf ) );
                }
                else
                {
                    if( prop.Name.StartsWith( "_" ) )
                    {
                        SetModifiers( prop, true, ref mf );
                    }
                    else
                    {
                        list.Add( Utils.CreateExpression<T>( prop.Name, prop.Value, mf.mainNot, mf.comparator ) );
                    }
                }
            }

            return list;
        }
    }
}
