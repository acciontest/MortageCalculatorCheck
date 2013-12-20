using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlteryxGalleryAPIWrapper;

namespace MortageCalculatorCheck
{
    public class DisposeElement : IDisposable
    {
        
        Client objClient = new Client("https://gallery.alteryx.com/api/");
       // private MortageCalculatorCheckSteps ok = new MortageCalculatorCheckSteps();
        
        
        public void Dispose()
        {
            objClient.Dispose();

        }

        public void Dispose(string _appID)
        {
            objClient.DeleteApp(_appID);
            objClient.Dispose();

        }
    }
}
