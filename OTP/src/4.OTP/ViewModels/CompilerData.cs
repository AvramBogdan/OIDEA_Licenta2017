using System.Collections.Generic;

namespace OTP.ViewModels
{
    public class CompilerData
    {
        public Dictionary<string,string> Inputs { get; set; }
        public string FirstFile { get; set; }
        public string currentProject { get; set; }
        public double AvailableSpace { get; set; }
    }
}
