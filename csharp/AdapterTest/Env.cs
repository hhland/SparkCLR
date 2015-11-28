using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterTest
{
   public  class Env
    {

        public static readonly string SPARK_MASTER_URL = ConfigurationManager.AppSettings["SPARK_MASTER_URL"];
    }
}
