#region License

/*
 * Copyright © 2010-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Runtime.Serialization;

using ProtoBuf;

namespace Spring.Http.Converters.ProtobufNet
{
    /// <summary>
    /// Implementation of <see cref="IHttpMessageConverter"/> that can read and write Google Protocol Buffers format 
    /// using the Protobuf-net library.
    /// </summary>
    /// <author>Mathias Kluba</author>
    public class ProtobufNetHttpMessageConverter : AbstractHttpMessageConverter
    {
        public static readonly MediaType PROTO_MEDIATYPE = new MediaType("application", "x-protobuf");

        /// <summary>
        /// Creates a new instance of the <see cref="ProtobufNetHttpMessageConverter"/> 
        /// with the media type 'application/x-protobuf'.
        /// </summary>
        public ProtobufNetHttpMessageConverter() :
            base(PROTO_MEDIATYPE)
        {
        }

        /// <summary>
        /// Indicates whether the given class is supported by this converter.
        /// </summary>
        /// <param name="type">The type to test for support.</param>
        /// <returns><see langword="true"/> if supported; otherwise <see langword="false"/></returns>
        protected override bool Supports(Type type)
        {
            var dataType = type;
            if (type.Name.Equals("List`1"))
            {
                dataType = type.GetGenericArguments()[0];
            }

            var isProtoContract = type.GetCustomAttributes(typeof(ProtoContractAttribute), false).Length > 0; 
            var isDataContract = type.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0;

            return isProtoContract | isDataContract;
        }

        /// <summary>
        /// Abstract template method that reads the actualy object. Invoked from <see cref="M:Read"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="message">The HTTP message to read from.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref="HttpMessageNotReadableException">In case of conversion errors</exception>
        protected override T ReadInternal<T>(IHttpInputMessage message)
        {
            // TODO : handle arrays/IEnumerable
            Type genericEnumerableType = typeof(T).GetInterface("IEnumerable`1");
            if (genericEnumerableType != null)
            {
                var dataType = genericEnumerableType.GetGenericArguments()[0];
                // do reflection to call Protobuf, because we can't call a generic method here
                var methodInfo = typeof(Serializer).GetMethod("DeserializeItems", new Type[] { typeof(Stream), typeof(PrefixStyle), typeof(int) });
                var genericMethod = methodInfo.MakeGenericMethod(dataType);
                var result = (IEnumerable)genericMethod.Invoke(null, new object[] { message.Body, PrefixStyle.Base128, 0 });
                if (typeof(T).IsArray)
                {
                    var list = new ArrayList();
                    foreach (var item in result)
                    {
                        list.Add(item);
                    }

                    var array = Array.CreateInstance(dataType, list.Count);

                    int cpt = 0;
                    foreach (var item in list)
                    {
                        array.SetValue(item, cpt);
                        cpt++;
                    }

                    return array as T;
                }
                else
                {
                    return (T)Activator.CreateInstance(typeof(T), result);
                }
            }
            else
            {
                return Serializer.Deserialize<T>(message.Body);
            }
        }

        /// <summary>
        /// Abstract template method that writes the actual body. Invoked from <see cref="M:Write"/>.
        /// </summary>
        /// <param name="content">The object to write to the HTTP message.</param>
        /// <param name="message">The HTTP message to write to.</param>
        /// <exception cref="HttpMessageNotWritableException">In case of conversion errors</exception>
        protected override void WriteInternal(object content, IHttpOutputMessage message)
        {
            if(content == null)
            {
                return;
            }

            var enumerable = content as IEnumerable;

            if (enumerable != null)
            {

                Type dataType = null;
                Type genericEnumerableType = content.GetType().GetInterface("IEnumerable`1");
                if (genericEnumerableType != null)
                {
                    dataType = genericEnumerableType.GetGenericArguments()[0];
                }
                else
                {
                    // don't support non-generic collections
                    throw new NotSupportedException("non-generic collections are not supported");
                }

                // do reflection to call Protobuf, because we can't call a generic method here
                var methodInfo = typeof(Serializer).GetMethods()
                    .Where(m => 
                        {
                            var parameters = m.GetParameters();
                            return m.Name.Equals("SerializeWithLengthPrefix")
                                && parameters.Length == 3 
                                && parameters[0].ParameterType.Equals(typeof(Stream))
                                && parameters[2].ParameterType.Equals(typeof(PrefixStyle));

                        })
                    .First();
                var genericMethod = methodInfo.MakeGenericMethod(dataType);

                message.Headers.ContentType = PROTO_MEDIATYPE;
                message.Body = (Stream s) => 
                    {
                        foreach (var item in enumerable)
                        {
                            genericMethod.Invoke(null, new object[] { s, item, PrefixStyle.Base128 });
                        }
                    };            
            }
            else
            {
                message.Headers.ContentType = PROTO_MEDIATYPE;
                message.Body = (Stream s) =>
                {
                    Serializer.Serialize(s, content);
                };            
            }
        }
    }
}