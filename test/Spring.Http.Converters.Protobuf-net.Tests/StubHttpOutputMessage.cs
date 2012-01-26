using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Spring.Http.Converters.ProtobufNet
{
    public class StubHttpOutputMessage : IHttpOutputMessage
    {
        public StubHttpOutputMessage()
        {
            this.Headers = new HttpHeaders();
        }

        public Action<Stream> Body
        {
            get; 
            set;
        }

        public HttpHeaders Headers
        {
            get;
            set;
        }
    }
}
