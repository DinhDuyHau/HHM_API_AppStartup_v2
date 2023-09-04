using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upload
{
    public class CustomException: Exception
    {
            public CustomException()
            {
                // Constructor mặc định
            }

            public CustomException(string message)
                : base(message)
            {
                // Constructor với thông điệp
            }

            public CustomException(string message, Exception innerException)
                : base(message, innerException)
            {
                // Constructor với thông điệp và ngoại lệ gốc
            }
        
    }
}
