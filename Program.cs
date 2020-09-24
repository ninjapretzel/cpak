using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace noheader {
	static class Program {
		private static string Filename(this string filepath) {
			return filepath.ForwardSlashPath().FromLast("/");
		}
		private static string Folder(this string filepath) {
			return filepath.UpToLast("/");
		}

		private static string UpToLast(this string str, string search) {
			if (str.Contains(search)) {
				int ind = str.LastIndexOf(search);
				return str.Substring(0, ind);
			}
			return str;
		}
		
		private static string ForwardSlashPath(this string path) { return path.Replace('\\', '/'); }
		private static string FromLast(this string str, string search) {
			if (str.Contains(search) && !str.EndsWith(search)) {
				int ind = str.LastIndexOf(search);

				return str.Substring(ind + 1);
			}
			return "";
		}

		private static string RelPath(this string filepath, string from) {
			return filepath.Replace(from, "").Replace(filepath.Filename(), "");
		}

		private static List<string> GetAllFiles(string dirPath, List<string> collector = null) {
			if (collector == null) { collector = new List<string>(); }


			collector.AddRange(Directory.GetFiles(dirPath).Select(it => it.ForwardSlashPath()));
			foreach (var subdir in Directory.GetDirectories(dirPath)) {
				GetAllFiles(subdir, collector);
			}

			return collector;
		}

		private static int Depth(this string str) { return str.Count((c)=>c=='/');	 }

		static void Main(string[] args) {

			string outfile = "cpak.c";
			if (args.Length > 0) {
				outfile = args[0];
			}
			outfile = Path.GetFullPath(outfile).ForwardSlashPath();

			var allFiles = GetAllFiles(Directory.GetCurrentDirectory());
			allFiles.Sort((a,b)=>{
				int depth = b.Depth() - a.Depth();
				return depth == 0 ? a.CompareTo(b) : depth;
			});
			
			// foreach (var file in files) { Console.WriteLine(file); }

			var headers = allFiles.Where(it => it != outfile && it.EndsWith(".h")).ToArray();
			var source = allFiles.Where(it => it != outfile && it.EndsWith(".c")).ToArray();
			StringBuilder str = new StringBuilder();
			str.Append("// Packed with cpak \n");
			str.Append("// A tool by ninjapretzel \n");

			void Add(string file, string kind) {
				string content = @$"
/////////////////////////////////////////////////////////////
// {kind}, Inserted from {file}
{File.ReadAllText(file)}

/////////////////////////////////////////////////////////////
";
				str.Append(content);
			}

			foreach (string f in headers) { Add(f, "Header"); }
			foreach (string f in source) { Add(f, "Source"); }

			File.WriteAllText(outfile, str.ToString());
			Console.WriteLine($"Processed {source.Length} source files and {headers.Length} header files into {outfile}");

		}

	}

}
