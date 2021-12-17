using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Comparer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var list = new[] { "Foobar", "Baz", "Foo", "Foobar", "Bar", };
            Array.Sort(list, new CustomComparer());

            var fooList = new List<Foo> { new Foo { A = "A" }, new Foo { A = "Aa" }, new Foo { A = "Ab" }, new Foo { A = "Ac" }, new Foo { A = "Ad" }, new Foo { A = "AAA" } };

            var result = fooList.AsQueryable().Filter("Aa,Ab", e => e.A);
            foreach (var item in result)
            {
                Console.Write(item.A);
            }
        }
    }

    public class CustomComparer : IComparer<string>
    {
        // Compares by Height, Length, and Width.

        private readonly string[] priorityList = { "Foo", "Bar", "Baz", "Foobar" };

        public int Compare(string x, string y)
        {
            var xIx = Array.IndexOf(priorityList, x);
            var yIx = Array.IndexOf(priorityList, y);
            return xIx.CompareTo(yIx);
        }
    }

    public static class SomeExtensions
    {
        public static IQueryable<T> Filter3<T>(this IQueryable<T> query, string Field, string Value)
        {
            var param = Expression.Parameter(typeof(T), "p");
            var prop = Expression.Property(param, Field);
            var val = Expression.Constant(Convert.ToInt32(Value));
            var body = Expression.Equal(prop, val);

            var exp = Expression.Lambda<Func<T, bool>>(body, param);

            
            
            return System.Linq.Queryable.Where(query, exp);
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> query, string filter, Expression<Func<T, string>> selector)
          where T : class
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return query;
            }
            var list = new List<string>();
            list = filter.Split(',').Select(i => i).ToList();
            var containsMethod = list.GetType().GetMethod("Contains", new[] { typeof(string) });
            var parameter = selector.Parameters[0];
            var lamda = Expression.Lambda<Func<T, bool>>(Expression.Call(Expression.Constant(list), containsMethod, selector.Body), parameter);
            if (list.Any())
            {
                return query.Where(lamda);
            }
            return query;
        }
    }

    public class Foo
    {
        public string A { get; set; }
    }
}
