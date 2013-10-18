using System;
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
			if (i >= this.Length) {
				return this.Length - 1;
			} else if (i < 0) {
				return 0;
			} else {
				return i;
			}
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

