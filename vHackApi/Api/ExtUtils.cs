using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;

namespace vHackApi.Api
{
    public static class ExtUtils
    {
        /// <summary>
        /// Parse all the properties of an object and copies them to related properties of the destination object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool CopyProperties<T>(this T from, ref T to)
        {
            try
            {
                foreach (var prop in from.GetType().GetProperties())
                {
                    var val = prop.GetValue(from);
                    prop.SetValue(to, val);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }


    public static class ObjectExt
    {
        public static T Clone<T>(this T obj) where T : new()
        {
            return ObjectExtCache<T>.Clone(obj);
        }
        static class ObjectExtCache<T> where T : new()
        {
            private static readonly Func<T, T> cloner;
            static ObjectExtCache()
            {
                ParameterExpression param = Expression.Parameter(typeof(T), "in");
                var props = typeof(T).GetProperties();
                var bindings = from prop in props
                               where prop.CanRead && prop.CanWrite
                               //let column = Attribute.GetCustomAttribute(prop, typeof(ColumnAttribute)) as ColumnAttribute
                               //where column == null || !column.IsPrimaryKey
                               select (MemberBinding)Expression.Bind(prop, Expression.Property(param, prop));

                cloner = Expression.Lambda<Func<T, T>>(
                    Expression.MemberInit(
                        Expression.New(typeof(T)), bindings), param).Compile();
            }
            public static T Clone(T obj)
            {
                return cloner(obj);
            }

        }
    }

}