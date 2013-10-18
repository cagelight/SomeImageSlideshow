using System;
using System.Collections.Generic;
using System.IO;
using Gtk;

namespace SomeImageSlideshow {
	public static class MAIN {
		public static int Main (string[] args) {
			Application.Init ();
			List<string> evalDirs = new List<string> ();
			bool recurse = false;
			int time = 0;
			foreach(string arg in args) {
				if (arg[0] == '-') {
					switch(arg[1]) {
					case 'R':
						recurse = true;
						break;
					case 'T':
						try {
							time = Convert.ToInt32 (arg.Substring (2));
						} catch {
							return Error ("The argument to -T was not a numeric value: {0}", arg.Substring(2));
						}
						break;
					}
				} else {
					evalDirs.Add (arg);
				}
			}
			List<string> imgpaths = new List<string> ();
			int stindex = -909;
			if (evalDirs.Count == 0) {
				foreach (string f in Directory.GetFiles(Environment.CurrentDirectory, "*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)) {
					imgpaths.Add (f);
				}
			} else {
				foreach (string eval in evalDirs) {
					Console.WriteLine (eval);
					try {
						string fullpath = eval [0] == Path.DirectorySeparatorChar ? eval : Path.Combine (Environment.CurrentDirectory, eval);
						FileAttributes attr = File.GetAttributes (fullpath);
						if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
							foreach (string f in Directory.GetFiles(fullpath, "*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)) {
								imgpaths.Add (f);
							}
						} else {
							string dirpath = Path.GetDirectoryName (fullpath);
							foreach (string f in Directory.GetFiles(dirpath)) {
								imgpaths.Add (f);
							}
							if (stindex == -909) {
								stindex = imgpaths.IndexOf (fullpath);
							}
						}
					} catch {
						return Error ("File not found, or you do not have appropriate permission to access the file.");
					}
				}
			}
			if (imgpaths.Count == 0) {
				return Error ("No valid images were obtained from the file, directory, or loadout you specified.");
			}
			SomeImageWindow windowMain = new SomeImageWindow (imgpaths, stindex);
			if (time != 0) {
				windowMain.SetAutoCycle (time);
			}
			windowMain.ShowAll ();
			Application.Run ();
			return 0;
		}

		private static int Usage () {
			Console.WriteLine ("The first parameter must be either a path to an image, or a path to a loadout (.sitl) file begging with -L.\nExamples:\nsishow \"/absolute/path/to/image.jpg\"\nsishow \"relative/path/to/image.jpg\"\nsishow -L\"cats.sitl\"");
			return -1;
		}

		private static int Error (string message, params object[] args) {
			Console.WriteLine (message, args);
			return -1;
		}
	}
}
