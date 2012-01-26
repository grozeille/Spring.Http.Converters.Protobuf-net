using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Spring.Http.Converters.ProtobufNet
{
    public class StubHttpInputMessage : IHttpInputMessage
    {
        public StubHttpInputMessage()
        {
            this.Headers = new HttpHeaders();
        }

        public Stream Body
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
