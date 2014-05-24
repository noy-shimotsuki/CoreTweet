// The MIT License (MIT)
//
// CoreTweet - A .NET Twitter Library supporting Twitter API 1.1
// Copyright (c) 2014 lambdalice
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;

#if !NET35
using System.Threading;
using System.Threading.Tasks;
#endif

namespace CoreTweet.Core
{
    internal static class InternalUtils
    {
        /// <summary>
        /// Object to Dictionary
        /// </summary>
        internal static IEnumerable<KeyValuePair<string, object>> ResolveObject<T>(T t)
        {
            if(t == null)
                return new Dictionary<string, object>();
            if(t is IEnumerable<KeyValuePair<string,object>>)
                return (t as IEnumerable<KeyValuePair<string,object>>);

#if WIN_RT
            var type = typeof(T).GetTypeInfo();
#else
            var type = typeof(T);
#endif
            if(type.GetCustomAttributes(typeof(TwitterParametersAttribute), false).Any())
            {
                var d = new Dictionary<string,object>();

#if WIN_RT
                foreach(var f in type.DeclaredFields.Where(x => x.IsPublic && !x.IsStatic))
#else
                foreach(var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
#endif
                {
                    var attr = (TwitterParameterAttribute)f.GetCustomAttributes(true).FirstOrDefault(y => y is TwitterParameterAttribute);
                    var value = f.GetValue(t);
                    if(attr.DefaultValue == null)
                        attr.DefaultValue = GetDefaultValue(t.GetType());

                    if(attr != null && value != null && !value.Equals(attr.DefaultValue))
                    {
                        var name = attr.Name;
                        d.Add(name != null ? name : f.Name, value);
                    }
                }

#if WIN_RT
                foreach(var p in type.DeclaredProperties.Where(x => x.CanRead && x.GetMethod.IsPublic && !x.GetMethod.IsStatic))
#else
                foreach(var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.CanRead))
#endif
                {
                    var attr = (TwitterParameterAttribute)p.GetCustomAttributes(true).FirstOrDefault(y => y is TwitterParameterAttribute);
                    var value = p.GetValue(t, null);
                    if(attr.DefaultValue == null)
                        attr.DefaultValue = GetDefaultValue(t.GetType());

                    if(attr != null && value != null && !value.Equals(attr.DefaultValue))
                    {
                        var name = attr.Name;
                        d.Add(name != null ? name : p.Name, value);
                    }
                }

                return d;
            }

            else
                return AnnoToDictionary(t);

            // throw new InvalidDataException("the object " + t.ToString() + " can not be used as parameters.");
        }

        /// <summary>
        /// Anonymous type object to Dictionary
        /// </summary>
        private static IDictionary<string,object> AnnoToDictionary<T>(T f)
        {
#if WIN_RT
            return typeof(T).GetRuntimeProperties()
                .Where(x => x.CanRead && x.GetMethod.IsPublic && !x.GetMethod.IsStatic && x.GetIndexParameters().Length == 0)
#else
            return typeof(T).GetProperties()
                .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
#endif
                .ToDictionary(x => x.Name, x => x.GetValue(f, null));
        }

        /// <summary>
        /// Gets the expression value.
        /// </summary>
        private static object GetExpressionValue(Expression<Func<string,object>> expr)
        {
            var constExpr = expr.Body as ConstantExpression;
            return constExpr != null ? constExpr.Value : expr.Compile()("");
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        private static object GetDefaultValue(Type type)
        {
            return type
#if WIN_RT
                .GetTypeInfo()
#endif
                .IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// Expressions to dictionary.
        /// </summary>
        internal static IEnumerable<KeyValuePair<string, object>> ExpressionsToDictionary(IEnumerable<Expression<Func<string,object>>> exprs)
        {
            return exprs.Select(x => new KeyValuePair<string, object>(x.Parameters[0].Name, GetExpressionValue(x)));
        }

        /// <summary>
        /// Gets the url of the specified api's name.
        /// </summary>
        /// <returns>The url.</returns>
        internal static string GetUrl(string apiName)
        {
            return string.Format("https://api.twitter.com/{0}/{1}.json", Property.ApiVersion, apiName);
        }

        internal static DateTimeOffset GetUnixTime(double seconds)
        {
            return new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero).AddSeconds(seconds);
        }

        internal static RateLimit ReadRateLimit(HttpWebResponse response)
        {
            var limit = response.Headers["X-Rate-Limit-Limit"];
            var remaining = response.Headers["X-Rate-Limit-Remaining"];
            var reset = response.Headers["X-Rate-Limit-Reset"];
            return limit != null && remaining != null && reset != null
                ? new RateLimit()
                {
                    Limit = int.Parse(limit),
                    Remaining = int.Parse(remaining),
                    Reset = InternalUtils.GetUnixTime(double.Parse(reset))
                }
                : null;
        }

        private static KeyValuePair<string, object> GetReservedParameter(List<KeyValuePair<string, object>> parameters, string reserved)
        {
            return parameters.Single(kvp => kvp.Key == reserved);
        }

#if !(PCL || WIN_RT)
        /// <summary>
        /// id, slug, etc
        /// </summary>
        internal static T AccessParameterReservedApi<T>(this TokensBase t, MethodType m, string uri, string reserved, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if(parameters == null) throw new ArgumentNullException("parameters");
            var list = parameters.ToList();
            var kvp = GetReservedParameter(list, reserved);
            list.Remove(kvp);
            return t.AccessApiImpl<T>(m, uri.Replace(string.Format("{{{0}}}", reserved), kvp.Value.ToString()), list, "");
        }

        /// <summary>
        /// id, slug, etc
        /// </summary>
        internal static ListedResponse<T> AccessParameterReservedApiArray<T>(this TokensBase t, MethodType m, string uri, string reserved, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if(parameters == null) throw new ArgumentNullException("parameters");
            var list = parameters.ToList();
            var kvp = GetReservedParameter(list, reserved);
            list.Remove(kvp);
            return t.AccessApiArrayImpl<T>(m, uri.Replace(string.Format("{{{0}}}", reserved), kvp.Value.ToString()), list, "");
        }
#endif

#if !NET35
        /// <summary>
        /// id, slug, etc
        /// </summary>
        internal static Task<T> AccessParameterReservedApiAsync<T>(this TokensBase t, MethodType m, string uri, string reserved, IEnumerable<KeyValuePair<string, object>> parameters, CancellationToken cancellationToken)
        {
            if(parameters == null) throw new ArgumentNullException("parameters");
            var list = parameters.ToList();
            var kvp = GetReservedParameter(list, reserved);
            list.Remove(kvp);
            return t.AccessApiAsyncImpl<T>(m, uri.Replace(string.Format("{{{0}}}", reserved), kvp.Value.ToString()), list, cancellationToken, "");
        }

        /// <summary>
        /// id, slug, etc
        /// </summary>
        internal static Task<ListedResponse<T>> AccessParameterReservedApiArrayAsync<T>(this TokensBase t, MethodType m, string uri, string reserved, IEnumerable<KeyValuePair<string, object>> parameters, CancellationToken cancellationToken)
        {
            if(parameters == null) throw new ArgumentNullException("parameters");
            var list = parameters.ToList();
            var kvp = GetReservedParameter(list, reserved);
            list.Remove(kvp);
            return t.AccessApiArrayAsyncImpl<T>(m, uri.Replace(string.Format("{{{0}}}", reserved), kvp.Value.ToString()), list, cancellationToken, "");
        }

        internal static T ReadResponse<T>(Task<HttpWebResponse> t, Func<string, T> parse, CancellationToken cancellationToken)
        {
            if(t.IsFaulted)
                t.Exception.Handle(ex => false);

            using(var reg = cancellationToken.Register(
#if PCL || WIN_RT
                t.Result.Dispose
#else
                t.Result.Close
#endif
            ))
            using(var sr = new StreamReader(t.Result.GetResponseStream()))
            {
                var result = parse(sr.ReadToEnd());
                var twitterResponse = result as ITwitterResponse;
                if(twitterResponse != null)
                    twitterResponse.RateLimit = ReadRateLimit(t.Result);
                return result;
            }
        }
#endif
    }
}

