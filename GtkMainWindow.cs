using System;
using System.Collections.Generic;
using System.Threading;
using Gtk;

namespace SomeImageSlideshow {
	public class SomeImageWindow : Window {
		private Gdk.GC sitGC;
		private Gdk.Pixbuf image;
		private Gdk.Pixbuf imager;
		private int imageIndex;
		private Random RND = new Random ();
		List<string> imagepaths;
		private SizeSwitch imageZooms;
		public SomeImageWindow (List<string> imagepaths, int startingindex = -909) : base(WindowType.Toplevel) {
			this.imagepaths = imagepaths;
			this.AddEvents ((int)(Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask | Gdk.EventMask.ScrollMask | Gdk.EventMask.PointerMotionMask));
			this.ButtonPressEvent += ButtonPressCallback;
			this.ButtonReleaseEvent += ButtonReleaseCallback;
			this.ScrollEvent += ScrollCallback;
			this.Realized += InitializationCallback;
			this.KeyPressEvent += KeyCallback;
			this.ExposeEvent += ExposeCallback;
			this.DeleteEvent += CloseCallback;
			this.SizeAllocated += ResizeCallback;
			this.MotionNotifyEvent += PointerMotionCallback;
			this.GetNewImage (startingindex);
			Gdk.Rectangle bSize = this.GetBestFit (new Gdk.Rectangle (0, 0, Gdk.Screen.Default.Width, Gdk.Screen.Default.Height));
			this.DefaultSize = new Gdk.Size (bSize.Width, bSize.Height);
			this.SetSizeRequest (160, 90);
			this.UpdateImage ();
		}

		private void UpdateImage () {
			Gdk.Size zoomedImage = this.imageZooms.Current ();
			if (zoomedImage.Width == image.Width && zoomedImage.Height == image.Height) {
				imager = image;
			} else {
				imager = image.ScaleSimple (zoomedImage.Width, zoomedImage.Height, Gdk.InterpType.Bilinear);
			}
			imageZooms.CalculateOffsetMaximums (this.Allocation.Width, this.Allocation.Height);
		}
		private void RedrawImage() {
			Gdk.Rectangle bounds = this.Allocation;
			Gdk.Pixmap todraw = new Gdk.Pixmap (this.GdkWindow, bounds.Width, bounds.Height);
			todraw.DrawRectangle (this.sitGC, true, bounds);
			int minWidth = Math.Min (imager.Width, bounds.Width);
			int minHeight = Math.Min (imager.Height, bounds.Height);
			int centeredX = (bounds.Width - minWidth) / 2;
			int centeredY = (bounds.Height - minHeight) / 2;
			Gdk.Point offsets = imageZooms.Offsets;
			todraw.DrawPixbuf (this.Style.BlackGC, imager, offsets.X, offsets.Y, Math.Max(0, centeredX - offsets.X), Math.Max(0, centeredY - offsets.Y), Math.Min(minWidth + offsets.X, bounds.Width), Math.Min(minHeight + offsets.Y, bounds.Height), Gdk.RgbDither.None, 0, 0);
			this.GdkWindow.BeginPaintRect (bounds);
			this.GdkWindow.DrawDrawable (this.sitGC, todraw, 0, 0, 0, 0, bounds.Width, bounds.Height);
			this.GdkWindow.EndPaint ();
		}

		private void UpdateImageZooms(bool reset = false) {
			if (this.imageZooms == null || reset) {
				this.imageZooms = new SizeSwitch (this.GetZoomArray ());
			} else {
				this.imageZooms.SetNewSizes (this.GetZoomArray ());
			}
		}

		private Gdk.Size[] GetZoomArray () {
			Gdk.Rectangle bestFit;
			if (this.IsRealized) {
				Gdk.Rectangle alloc = this.Allocation;
				bestFit = GetBestFit (alloc);
			} else {
				Gdk.Rectangle scn = new Gdk.Rectangle (0, 0, Gdk.Screen.Default.Width, Gdk.Screen.Default.Height);
				bestFit = GetBestFit (scn);
			} 
			List<Gdk.Size> zoomArray = new List<Gdk.Size> ();
			zoomArray.Add (bestFit.Size);
			int divDeltaW = image.Width - bestFit.Width;
			int divDeltaH = image.Height - bestFit.Height;
			int divNum = Math.Max(ImgHelper.PowersOfNum(image.Width, 8), ImgHelper.PowersOfNum(image.Height, 8));
			float divAmmtW = ((float)divDeltaW) / divNum;
			float divAmmtH = ((float)divDeltaH) / divNum;
			for (int i = 1; i < divNum; i++) {
				zoomArray.Add (new Gdk.Size((int)(bestFit.Width + (i * divAmmtW)),(int)(bestFit.Height + (i * divAmmtH))));
			}
			zoomArray.Add (new Gdk.Size(image.Width, image.Height));
			return zoomArray.ToArray();
		}

		private Gdk.Rectangle GetBestFit (Gdk.Rectangle frame) {
			float aspectImage = ((float)image.Width) / image.Height;
			float aspectFrame = ((float)frame.Width) / frame.Height;
			Gdk.Rectangle fit;
			if (aspectImage > aspectFrame) {
				if (image.Width >= frame.Width) {
					fit.Width = frame.Width;
					fit.Height = (int)(frame.Width / aspectImage);
					fit.X = 0;
					fit.Y = (frame.Height - fit.Height) / 2;
				} else {
					fit.Width = image.Width;
					fit.Height = image.Height;
					fit.X = (frame.Width - image.Width) / 2;
					fit.Y = (frame.Height - image.Height) / 2;
				}
			} else if (aspectImage < aspectFrame) {
				if (image.Height >= frame.Height) {
					fit.Width = (int)(frame.Height * aspectImage);
					fit.Height = frame.Height;
					fit.X = (frame.Width - fit.Width) / 2;
					fit.Y = 0;
				} else {
					fit.Width = image.Width;
					fit.Height = image.Height;
					fit.X = (frame.Width - image.Width) / 2;
					fit.Y = (frame.Height - image.Height) / 2;
				}
			} else {
				if (image.Width >= frame.Width) {
					fit.Width = frame.Width;
					fit.Height = frame.Height;
					fit.X = 0;
					fit.Y = 0;
				} else {
					fit.Width = image.Width;
					fit.Height = image.Height;
					fit.X = (frame.Width - image.Width) / 2;
					fit.Y = (frame.Height - image.Height) / 2;
				}
			}
			return fit;
		}

		private void GetNewImage (int index = -909) {
			int imgindex;
			if (index == -909) {
				imgindex = RND.Next (imagepaths.Count);
			} else if (index < 0) {
				imgindex = imagepaths.Count - 1;
			} else if (index >= imagepaths.Count) {
				imgindex = 0;
			} else {
				imgindex = index;
			}
			try {
				this.image = new Gdk.Pixbuf (this.imagepaths [imgindex]);
				this.imageIndex = imgindex;
				this.UpdateImageZooms (true);
				this.Title = String.Format("SiShow - \"{0}\"", this.imagepaths[imgindex]);
			} catch {
				Console.WriteLine ("Image at index {0} failed to load and will be removed from the image list: \"{1}\"", imgindex, imagepaths [imgindex]);
				this.imagepaths.RemoveAt (imgindex);
				GetNewImage (index);
			}
		}

		public void Cycle (int index = -909) {
			this.GetNewImage (index);
			this.UpdateImage ();
			this.autoCycleCur = this.autoCycleTime;
			this.QueueDraw ();
		}
		private Thread autoCycle;
		public void SetAutoCycle(int seconds) {
			if (autoCycle == null) {
				autoCycle = new Thread(new ThreadStart(AutoCycleRun));
			}
			autoCycleTime = seconds;
			autoCycleCur = seconds;
			if (!autoCycle.IsAlive) {
				autoCycle.Start ();
			}
		}
		public void StopAutoCycle() {
			if (autoCycle != null && this.autoCycle.IsAlive) {
				autoCycle.Abort ();
			}
		}
		private int autoCycleTime = 30;
		private int autoCycleCur = 30;
		private void AutoCycleRun() {
			while (true) {
				Thread.Sleep (1000);
				autoCycleCur -= 1;
				if (autoCycleCur <= 0) {
					this.Cycle ();
				}
			}
		}
		private bool dragging = false;
		private void ButtonPressCallback (object o, ButtonPressEventArgs e) {
			switch(e.Event.Button) {
			case 1:
				dragging = true;
				break;
			case 2:
				this.Cycle ();
				break;
			}
		}
		private void ButtonReleaseCallback (object o, ButtonReleaseEventArgs e) {
			if (e.Event.Button == 1) {
				dragging = false;
			}
		}

		private void ScrollCallback(object o, ScrollEventArgs e) {
			switch (e.Event.Direction) {
			case Gdk.ScrollDirection.Up:
				this.imageZooms.Increment ();
				this.UpdateImage ();
				this.QueueDraw ();
				break;
			case Gdk.ScrollDirection.Down:
				this.imageZooms.Decrement ();
				this.UpdateImage ();
				this.QueueDraw ();
				break;
			case Gdk.ScrollDirection.Left:
				this.Cycle (this.imageIndex - 1);
				break;
			case Gdk.ScrollDirection.Right:
				this.Cycle (this.imageIndex + 1);
				break;
			}
		}

		private void InitializationCallback (object o, EventArgs e) {
			Gdk.GCValues sitGCV = new Gdk.GCValues ();
			sitGCV.Background = new Gdk.Color (0x00, 0x00, 0x00);
			sitGCV.Foreground = new Gdk.Color (0x00, 0x00, 0x00);
			sitGCV.Fill = Gdk.Fill.Solid;
			this.sitGC = new Gdk.GC (this.GdkWindow, sitGCV, Gdk.GCValuesMask.Background | Gdk.GCValuesMask.Foreground | Gdk.GCValuesMask.Fill);
			this.GdkWindow.Background = new Gdk.Color (0x00, 0x00, 0x00);
			recAlloc = this.Allocation;
			this.imageZooms.CalculateOffsetMaximums (recAlloc.Width, recAlloc.Height);
		}

		private Gdk.Rectangle recAlloc;

		private void ResizeCallback (object o, SizeAllocatedArgs e) {
			if (e.Allocation == recAlloc) {
				this.UpdateImageZooms ();
				this.UpdateImage ();
				if ((e.Allocation.Width <= recAlloc.Width || e.Allocation.Height <= recAlloc.Height)) {
					this.GdkWindow.InvalidateRect (this.Allocation, true);
				}
			}
			recAlloc = e.Allocation;
		}

		private void ExposeCallback (object o, ExposeEventArgs e) {
			//this.UpdateImage ();
			this.RedrawImage ();
		}

		private void KeyCallback (object o, KeyPressEventArgs e) {
			switch (e.Event.Key) {
			case Gdk.Key.bracketright:
			case Gdk.Key.Right:
				this.Cycle (this.imageIndex + 1);
				break;
			case Gdk.Key.bracketleft:
			case Gdk.Key.Left:
				this.Cycle (this.imageIndex - 1);
				break;
			case Gdk.Key.R:
			case Gdk.Key.r:
				this.Cycle ();
				break;
			case Gdk.Key.Escape:
				this.CloseCallback (this, new DeleteEventArgs ());
				break;
			}
		}
		private Gdk.Point prevMouse = new Gdk.Point (0, 0);
		private Gdk.Point curMouse = new Gdk.Point (0, 0);
		private void PointerMotionCallback(object o, MotionNotifyEventArgs e) {
			curMouse = new Gdk.Point ((int)e.Event.X, (int)e.Event.Y);
			if (this.dragging) {
				this.imageZooms.DeltaOffsets (prevMouse.X - curMouse.X, prevMouse.Y - curMouse.Y);
				this.QueueDraw ();
			}
			prevMouse = curMouse;
		}

		private void CloseCallback (object o, DeleteEventArgs e) {
			this.StopAutoCycle ();
			Application.Quit ();
		}
	}
}

