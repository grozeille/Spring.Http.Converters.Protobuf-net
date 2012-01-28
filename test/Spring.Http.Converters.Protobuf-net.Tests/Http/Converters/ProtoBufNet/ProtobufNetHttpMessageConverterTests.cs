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

using System.IO;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;
using ProtoBuf;

namespace Spring.Http.Converters.ProtobufNet
{
    [TestFixture]
    public class ProtobufNetHttpMessageConverterTests
    {
        private ProtobufNetHttpMessageConverter converter;

        [SetUp]
        public void SetUp()
        {
            converter = new ProtobufNetHttpMessageConverter();
        }

        [Test]
        public void CanRead()
        {
            Assert.IsTrue(converter.CanRead(typeof(People), ProtobufNetHttpMessageConverter.PROTO_MEDIATYPE));
            Assert.IsFalse(converter.CanRead(typeof(string), ProtobufNetHttpMessageConverter.PROTO_MEDIATYPE));
            Assert.IsFalse(converter.CanRead(typeof(People), new MediaType("text", "plain")));
        }

        [Test]
        public void CanWrite()
        {
            Assert.IsTrue(converter.CanWrite(typeof(People), ProtobufNetHttpMessageConverter.PROTO_MEDIATYPE));
            Assert.IsFalse(converter.CanWrite(typeof(string), ProtobufNetHttpMessageConverter.PROTO_MEDIATYPE));
            Assert.IsFalse(converter.CanWrite(typeof(People), new MediaType("text", "plain")));
        }

        public void GenerateSerializedVersions()
        {
            People[] peoples = new People[] 
            { 
                new People{ Id= 1, Firstname = "chuck", Lastname ="norris"},
                new People{ Id= 2, Firstname = "jackie", Lastname ="chan"},
                new People{ Id= 3, Firstname = "jean-claude", Lastname ="van damme"}
            };

            using(var fs = new FileStream("peopleList.data", FileMode.Create))
            {
                foreach(var item in peoples)
                {
                    Serializer.SerializeWithLengthPrefix(fs, item, PrefixStyle.Base128);
                }
            }

            using (var fs = new FileStream("people.data", FileMode.Create))
            {
                var chuck = new People { Id = 1, Firstname = "chuck", Lastname = "norris" };

                Serializer.Serialize<People>(fs, chuck);
            }
        }

        [Test]
        public void SerializeObject()
        {           
            var chuck = new People { Id = 1, Firstname = "chuck", Lastname = "norris" };
            
            var outputMessage = new StubHttpOutputMessage();

            converter.Write(chuck, ProtobufNetHttpMessageConverter.PROTO_MEDIATYPE, outputMessage);

            Assert.NotNull(outputMessage.Body);
            
            MemoryStream actual = new MemoryStream();
            byte[] expected = File.ReadAllBytes("people.data");
          
            outputMessage.Body(actual);

            Assert.AreEqual(expected, actual.ToArray());
        }

        [Test]
        public void SerializeObjectList()
        {
            People[] peoples = new People[] 
            { 
                new People{ Id= 1, Firstname = "chuck", Lastname ="norris"},
                new People{ Id= 2, Firstname = "jackie", Lastname ="chan"},
                new People{ Id= 3, Firstname = "jean-claude", Lastname ="van damme"}
            };

            var outputMessage = new StubHttpOutputMessage();

            converter.Write(new LinkedList<People>(peoples.ToList()), ProtobufNetHttpMessageConverter.PROTO_MEDIATYPE, outputMessage);

            Assert.NotNull(outputMessage.Body);

            MemoryStream actual = new MemoryStream();
            byte[] expected = File.ReadAllBytes("peopleList.data");

            outputMessage.Body(actual);

            Assert.AreEqual(expected, actual.ToArray());
        }

        [Test]
        public void SerializeObjectArray()
        {
            People[] peoples = new People[] 
            { 
                new People{ Id= 1, Firstname = "chuck", Lastname ="norris"},
                new People{ Id= 2, Firstname = "jackie", Lastname ="chan"},
                new People{ Id= 3, Firstname = "jean-claude", Lastname ="van damme"}
            };

            var outputMessage = new StubHttpOutputMessage();

            converter.Write(peoples, ProtobufNetHttpMessageConverter.PROTO_MEDIATYPE, outputMessage);

            Assert.NotNull(outputMessage.Body);

            MemoryStream actual = new MemoryStream();
            byte[] expected = File.ReadAllBytes("peopleList.data");

            outputMessage.Body(actual);

            Assert.AreEqual(expected, actual.ToArray());
        }

        [Test]
        public void DeserializeObject()
        {
            var chuck = new People { Id = 1, Firstname = "chuck", Lastname = "norris" };

            var inputMessage = new StubHttpInputMessage();
            inputMessage.Body = new MemoryStream(File.ReadAllBytes("people.data"));

            People actual = converter.Read<People>(inputMessage);

            Assert.NotNull(actual);

            Assert.AreEqual(chuck, actual);
        }

        [Test]
        public void DeserializeObjectList()
        {
            People[] peoples = new People[] 
            { 
                new People{ Id= 1, Firstname = "chuck", Lastname ="norris"},
                new People{ Id= 2, Firstname = "jackie", Lastname ="chan"},
                new People{ Id= 3, Firstname = "jean-claude", Lastname ="van damme"}
            };

            var inputMessage = new StubHttpInputMessage();
            inputMessage.Body = new MemoryStream(File.ReadAllBytes("peopleList.data"));

            var actual = converter.Read<List<People>>(inputMessage);

            Assert.NotNull(actual);
            CollectionAssert.AreEqual(peoples, actual);
        }

        [Test]
        public void DeserializeObjectArray()
        {
            People[] peoples = new People[] 
            { 
                new People{ Id= 1, Firstname = "chuck", Lastname ="norris"},
                new People{ Id= 2, Firstname = "jackie", Lastname ="chan"},
                new People{ Id= 3, Firstname = "jean-claude", Lastname ="van damme"}
            };

            var inputMessage = new StubHttpInputMessage();
            inputMessage.Body = new MemoryStream(File.ReadAllBytes("peopleList.data"));

            var actual = converter.Read<People[]>(inputMessage);

            Assert.NotNull(actual);
            CollectionAssert.AreEqual(peoples, actual);
        }
    }
}
