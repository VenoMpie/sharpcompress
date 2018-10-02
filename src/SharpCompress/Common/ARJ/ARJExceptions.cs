using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpCompress.Common.ARJ
{
    public class BadHeaderException : Exception
    {
        public override string Message { get { return "Bad Header"; } }
    }
    public class FileDataError : Exception
    {
        public override string Message { get { return "Bad File Data"; } }
    }
    public class HeaderCRCError : Exception
    {
        public override string Message { get { return "Header CRC Error"; } }
    }
}
