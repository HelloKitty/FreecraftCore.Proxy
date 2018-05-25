using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binarysharp.MemoryManagement;

namespace FreecraftCore.Proxy.WarcraftClientPatcher.Application
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("Enter Warcraft ProcessId: ");
			string processId = Console.ReadLine();

			//TODO: Move this into a provider or something. We want to support several clients/offsets
			using(MemorySharp memorySharpProvider = new MemorySharp(int.Parse(processId)))
			{
				//This returns out of the ARC4_Process functions by jumping accross the ARC4 logic into the final routine
				memorySharpProvider.Assembly.Inject(new string[1] { "JMP 0x774EBE" }, (IntPtr)(0x00774EA0 + 0x11));
				memorySharpProvider.Assembly.Inject(new string[1] { "JMP 0x774FD3" }, (IntPtr)(0x00774EA0 + 0x3B)); //short circuit ARC4 with jump
				memorySharpProvider.Assembly.Inject(new string[1] { "JMP 0x775025" }, (IntPtr)(0x00774EA0 + 0x138));
			}

			Console.WriteLine("Patched. Press any key.");
			Console.ReadKey();
		}
	}
}
