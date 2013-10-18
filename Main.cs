using System;
using System.Collections.Generic;
using System.IO;
using Gtk;

namespace SomeImageSlideshow {
	public static class MAIN {
		public static int Main (string[] args) {
			Application.Init ();
			List<string> imgpaths = new List<string> ();
			List<string> evalDirs = new List<string> ();
			bool recurse = false;
			int time = 0;
			bool loadfiledir = true;
			foreach(string arg in args) {
				if (arg[0] == '-') {
					switch(arg[1]) {
					case 'r':
					case 'R':
						recurse = true;
						break;
					case 't':
					case 'T':
						try {
							time = Convert.ToInt32 (arg.Substring (2));
						} catch {
							return Error ("The argument to -T was not a numeric value: {0}", arg.Substring(2));
						}
						break;
					case 'l':
					case 'L':
						string loadoutpath = arg.Substring (2);
						if (!File.Exists (loadoutpath)) {
							return Error ("The loadout could not be found at this directory.");
						}
						Loadout ld = new Loadout (loadoutpath);
						foreach(string file in ld.GetFiles()) {
							imgpaths.Add (file);
						}
						break;
					case 'n':
					case 'N':
						loadfiledir = false;
						break;
					case 'h':
					case 'H':
						return Usage ();
					}
				} else {
					evalDirs.Add (arg);
				}
			}
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
							if (loadfiledir) {
								foreach (string f in Directory.GetFiles(dirpath)) {
									imgpaths.Add (f);
								}
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
			Console.WriteLine ("Options are divided by spaces.");
			Console.WriteLine ("An option that does not have a switch \"-\" is considered to be a path to an image or directory. You can have as many of these as you want.");
			Console.WriteLine ("Valid switches:");
			Console.WriteLine ("-L<file> : Loads the loadout file located at <file>.");
			Console.WriteLine ("-N : Do not add all files in the same directory for files specified in the arguments. Does not affect loadouts.");
			Console.WriteLine ("-R : Recurse through subdirectories on all directories specified in the arguments. Does not affect loadouts.");
			Console.WriteLine ("-T<num> : Activates slideshow mode, automatically picking a new image every <num> seconds.");
			return -1;
		}

		private static int Error (string message, params object[] args) {
			Console.WriteLine (message, args);
			return -1;
		}
	}
}
