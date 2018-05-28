using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binarysharp.MemoryManagement;
using Binarysharp.MemoryManagement.Assembly.CallingConvention;

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
				//memorySharpProvider.Assembly.Inject(new string[1] { "JMP 0x774EBE" }, (IntPtr)(0x00774EA0 + 0x11));
				//memorySharpProvider.Assembly.Inject(new string[1] { "JMP 0x774FD3" }, (IntPtr)(0x00774EA0 + 0x3B)); //short circuit ARC4 with jump
				//memorySharpProvider.Assembly.Inject(new string[1] { "JMP 0x775025" }, (IntPtr)(0x00774EA0 + 0x138));

				//memorySharpProvider.Assembly.Inject(new string[2] { "push ebp", "mov [ebp + 0x14], [ebp + 0x10]" }, (IntPtr)(0x00774EA0));

				//memorySharpProvider.Assembly.Inject(new string[] {"JMP 0x632BFB"}, (IntPtr)0x632B50 + 0x9D);
				//memorySharpProvider.Assembly.Inject(new string[] { "JMP 0x632BFB" }, (IntPtr)0x632B50 + 0x1E);
				//memorySharpProvider.Assembly.Inject(new string[] { "JMP 0x632BFD" }, (IntPtr)0x632B50 + 0xD);
				//memorySharpProvider.Assembly.Inject(new string[] { "JMP 0x466C05" }, (IntPtr)0x466BF0 + 0x13);

				//memorySharpProvider.Assembly.Inject(new string[] { "JMP 0x406F79" }, (IntPtr)0x406F40 + 0xA);

				//
				//memorySharpProvider.Assembly.Inject(new string[] { "JMP 0x632BFD" }, (IntPtr)0x632B50 + 0x9D);

				//memorySharpProvider.Assembly.Inject(new string[] { "JMP short 0x4678CA" }, (IntPtr)0x4675F0 + 0x2AE);
				IntPtr sub_6B0970Result = memorySharpProvider.Read<IntPtr>((IntPtr)0x00C79CF4, false);
				IntPtr sessionKeyAddress = sub_6B0970Result + 0x508;
				byte[] key = memorySharpProvider.Read<byte>(sessionKeyAddress, 40, false);
				Console.WriteLine($"{nameof(sub_6B0970Result)}: {sub_6B0970Result}");

				Console.WriteLine($"Key: {key.Aggregate("", (s, b) => $"{s} {b}")}");
			}

			Console.WriteLine("Patched. Press any key.");
			Console.ReadKey();
		}
	}
}
