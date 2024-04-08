using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WestLakeShape.Common
{
    public static class CloneHelper
    {
        static readonly MethodInfo _deepcloneObject = typeof(CloneHelper).GetMethod(nameof(DeepClone), BindingFlags.Static | BindingFlags.Public);
        static readonly Dictionary<Type, Func<object, object>> _deepCache = new Dictionary<Type, Func<object, object>>();
        static readonly Dictionary<Type, Func<object, object>> _shallowCache = new Dictionary<Type, Func<object, object>>();

        /// <summary>
        /// <para>Clones an object along with all objects referenced directly or indirectly by its fields.</para>
        /// <para>Delegate fields are ignored.</para>
        /// <para>Only single-dimentional, zero-based arrays supported.</para>
        /// </summary>
        public static T DeepClone<T>(this T obj)
        {
            if (ReferenceEquals(obj, null))
                return default(T);

            var t = obj.GetType();
            if (t == typeof(string) || t.GetTypeInfo().IsValueType)
                return obj;

            if (t.IsArray)
                return (T)(object)CloneArray((Array)(object)obj, t.GetElementType(), true);

            var clone = GetObjectCloner(t, true);
            return (T)clone(obj);
        }

        /// <summary>
        /// <para>Clones an object.</para>
        /// <para>Delegate fields are ignored.</para>
        /// <para>Only single-dimentional, zero-based arrays supported.</para>
        /// </summary>
        public static T ShallowClone<T>(this T obj)
        {
            if (ReferenceEquals(obj, null))
                return default(T);

            var t = obj.GetType();
            if (t == typeof(string) || t.GetTypeInfo().IsValueType)
                return obj;

            if (t.IsArray)
                return (T)(object)CloneArray((Array)(object)obj, t.GetElementType(), false);

            var clone = GetObjectCloner(t, false);
            return (T)clone(obj);
        }

        /// <summary>
        /// <para>Slices an object to its base class.</para>
        /// <para>Delegate fields are ignored.</para>
        /// <para>Only single-dimentional, zero-based arrays supported.</para>
        /// </summary>
        public static TBase CloneAs<TBase>(this TBase obj, bool deepClone = false)
        {
            if (ReferenceEquals(obj, null))
                return default(TBase);

            var t = obj.GetType();
            var b = typeof(TBase);

            if (!b.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                throw new ArgumentException($"obj should be assignable to type {b.FullName}", nameof(obj));

            if (t == typeof(string) || t.GetTypeInfo().IsValueType)
                return obj;

            if (t == typeof(TBase))
                return DeepClone(obj);

            var clone = GetObjectCloner(b, deepClone);
            return (TBase)clone(obj);
        }

        private static Array CloneArray(Array array, Type elementType, bool deepClone)
        {
            var length = array.Length;
            var result = Array.CreateInstance(elementType, length);

            for (var i = 0; i < length; i++)
            {
                var element = array.GetValue(i);
                var cloned = deepClone
                    ? DeepClone(element)
                    : element;
                result.SetValue(cloned, i);
            }

            return result;
        }

        private static Func<object, object> GetObjectCloner(Type type, bool deepClone)
        {
            Func<object, object> result;
            var cache = deepClone
                ? _deepCache
                : _shallowCache;

            if (!cache.TryGetValue(type, out result))
            {
                if (type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Any(x => x.GetParameters().Length == 0))
                {
                    var param = Expression.Parameter(typeof(object), "x");

                    var bindings = new List<MemberAssignment>();
                    foreach (var field in GetFields(type))
                    {
                        var t = field.FieldType;
                        if (t.GetTypeInfo().IsSubclassOf(typeof(Delegate)))
                            continue;

                        var value = Expression.Field(Expression.Convert(param, type), field);

                        var cloned = !deepClone || t == typeof(string) || t.GetTypeInfo().IsValueType
                            ? (Expression)value
                            : Expression.Convert(Expression.Call(_deepcloneObject.MakeGenericMethod(field.FieldType), value), field.FieldType);

                        bindings.Add(Expression.Bind(field, cloned));
                    }

                    var init = Expression.MemberInit(Expression.New(type), bindings.Cast<MemberBinding>().ToArray());
                    result = Expression.Lambda<Func<object, object>>(init, param).Compile();
                }
                else
                {
                    // 没有默认构造方法，无法克隆，原样返回
                    result = x => x;
                }
                cache.Add(type, result);
            }
            return result;
        }

        private static IEnumerable<FieldInfo> GetFields(Type type)
        {
            var fields = Enumerable.Empty<FieldInfo>();
            var t = type;
            while (t != null)
            {
                var ti = t.GetTypeInfo();
                fields = fields.Concat(ti.DeclaredFields.Where(x => !x.IsStatic));
                t = ti.BaseType;
            }
            return fields;
        }
    }
}
