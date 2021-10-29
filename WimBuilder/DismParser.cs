using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WimBuilder
{
    class DismParser
    {
        public DismParser () { }
        public static List<Wim> ParseWimInfo(string[] dismOutput)
        {
            List<Wim> vec = new();

            for (int i = 0; i < dismOutput.Length; i++)
            {
                if (dismOutput[i].Contains("Index : "))
                {
                    vec.Add(
                            new Wim
                            {
                                Index = Convert.ToInt32(dismOutput[i].Split(": ")[1]),
                                Name = dismOutput[i + 1].Split(": ")[1],
                                Description = dismOutput[i + 2].Split(": ")[1],
                                Size = dismOutput[i + 3].Split(": ")[1]
                            }
                        );
                }
            }
            return vec;
        }
    }
}