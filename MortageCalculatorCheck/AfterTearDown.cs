using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MortageCalculatorCheck
{
    public  class AfterTearDown:IDisposable
    {
        public AfterTearDown()
        {
            Dispose();
        }

        public void Dispose()
        {

        }
    }
}
