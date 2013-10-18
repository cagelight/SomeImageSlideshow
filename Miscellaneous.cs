using System;
using System.Collections.Generic;
using System.IO;
using Gdk;

namespace SomeImageSlideshow {
	public class SizeSwitch {
		private Size[] sizes;
		private int index = 0;
		private Point offsets;
		public Point Offsets {get {ReEvaluateOffsets(); return this.offsets;}}
		private Point maximums = new Point(0, 0);
		public SizeSwitch(params Size[] sizes) {
			this.sizes = sizes;
			offsets = new Point (0, 0);
		}
		public int Length {
			get {
				return this.sizes.Length;
			}
		}
		public void SetNewSizes(params Size[] newsizes) {
			this.sizes = newsizes;
			this.index = SizeCap (index);
		}
		public Size Increment() {
			index = SizeCap (index + 1);
			return this.sizes [index];
		}
		public Size Decrement() {
			index = SizeCap (index - 1);
			return this.sizes [index];
		}
		public Size Current() {
			return this.sizes [index];
		}
		public void DeltaOffsets(int dX, int dY) {
			this.offsets.X += dX;
			this.offsets.Y += dY;
		}
		public void CalculateOffsetMaximums(int frameWidth, int frameHeight) {
			this.maximums.X = Math.Max(this.Current ().Width - frameWidth, 0);
			this.maximums.Y = Math.Max(this.Current ().Height - frameHeight, 0);
		}
		public void ReEvaluateOffsets() {
			this.offsets.X = Math.Max(Math.Min(this.offsets.X, maximums.X), 0);
			this.offsets.Y = Math.Max(Math.Min(this.offsets.Y, maximums.Y), 0);
		}
		public Size this[int i] {
			get {
				return this.sizes [SizeCap (i)];
			}
		}
		private int SizeCap(int i) {
			return Math.Max (Math.Min (i, this.Length - 1), 0);
		}
	}

	public class Loadout {
		public enum AssetType {File, Directory, Subdirectories}
		Dictionary<AssetType, List<string>> loadoutDict = new Dictionary<AssetType, List<string>> ();
		public Loadout(string ldpath) {
			loadoutDict.Add (AssetType.File, new List<string> ());
			loadoutDict.Add (AssetType.Directory, new List<string> ());
			loadoutDict.Add (AssetType.Subdirectories, new List<string> ());
			foreach(string line in File.ReadAllLines(ldpath)) {
				char AT = line [0];
				string path = line.Substring (2);
				switch(AT) {
				case 'F':
					loadoutDict [AssetType.File].Add (path);
					break;
				case 'D':
					loadoutDict [AssetType.Directory].Add (path);
					break;
				case 'S':
					loadoutDict [AssetType.Subdirectories].Add (path);
					break;
				}
			}
		}
		public string[] GetFiles () {
			List<string> imgpaths = new List<string> ();
			foreach(string file in loadoutDict[AssetType.File]) {
				imgpaths.Add (file);
			}
			foreach(string directory in loadoutDict[AssetType.Directory]) {
				foreach(string file in Directory.GetFiles(directory)) {
					imgpaths.Add (file);
				}
			}
			foreach(string directory in loadoutDict[AssetType.Subdirectories]) {
				foreach(string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories)) {
					imgpaths.Add (file);
				}
			}
			return imgpaths.ToArray ();
		}
	}

	public static class ImgHelper {
		public static int PowersOfTwo(int num) {
			int rnum = 0;
			int inum = 2;
			while (true) {
				if (num >= inum) {
					rnum++;
					inum *= 2;
				} else {
					return rnum;
				}
			}
		}
		public static int PowersOfNum(int num, int pow) {
			int rnum = 0;
			int inum = pow;
			while (true) {
				if (num >= inum) {
					rnum++;
					inum *= pow;
				} else {
					return rnum;
				}
			}
		}
	}
}

