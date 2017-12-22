using System;
using Eto.Drawing;
using Eto.Forms;
using Pablo.BGI;
using Pablo.Controls;

namespace Pablo.Formats.Character.Tools
{
	public abstract class CenterAspectTool : TwoPointTool
	{
		Rectangle? currentRect;

		protected bool IsSquare { get; private set; }

		protected bool IsCentered { get; private set; }

		protected abstract void UpdateWithLocation (Rectangle rect, Key modifiers, Point end);
		
		protected override Rectangle? CurrentRectangle {
			get {
				return currentRect ?? base.CurrentRectangle;
			}
		}
		
		protected Rectangle? ResolvedRectangle {
			get { return currentRect; }
			set { currentRect = value; }
		}
		
		public override void Cancel ()
		{
			base.Cancel ();
			currentRect = null;
		}
		
		protected override void Finish ()
		{
			base.Finish ();
			currentRect = null;
		}
		
		protected sealed override void Update (Point start, Point end, Key modifiers, Point location)
		{
			var rect = new Rectangle ();
			var aspect = Handler.CurrentPage.Font.Aspect / Handler.Aspect;
			var size = new Size(end - start);
			
			var shouldBeSquare = (!modifiers.HasFlag (Command.CommonModifier) && modifiers.HasFlag (Key.Shift)) ^ IsSquare;
			var shouldBeCentered = modifiers.HasFlag (Key.Alt) ^ IsCentered;
			
			
			if (shouldBeSquare) {
				int diameter = Math.Max (Math.Abs (size.Width), (int)Math.Round (Math.Abs (size.Height) / aspect));
				if (shouldBeCentered) {
					rect.Location = start - new Size (diameter * (size.Width >= 0 ? 1 : -1), (int)Math.Round (diameter * aspect) * (size.Height >= 0 ? 1 : -1));
					diameter *= 2;
				} else {
					rect.Location = start;
				}
				rect.Size = new Size (
					(diameter + ((size.Width >= 0) ? 1 : 0)) * (size.Width >= 0 ? 1 : -1),
					((int)Math.Round (diameter * aspect) + ((size.Height >= 0) ? 1 : 0)) * (size.Height >= 0 ? 1 : -1)
				);
				
			} else {
				if (shouldBeCentered) {
					rect.Location = start - size;
					size *= new Size (2, 2);
					if (size.Width >= 0)
						size.Width ++;
					if (size.Height >= 0)
						size.Height ++;
					rect.Size = size;
				} else {
					if (size.Width >= 0)
						size.Width ++;
					if (size.Height >= 0)
						size.Height ++;
					rect.Size = size;
					rect.Location = start;
				}
			}
			if (currentRect == null || currentRect.Value != rect) {
				UpdateWithLocation (rect, modifiers, end);
				currentRect = rect;
			}
			base.Update (start, end, modifiers, location);
		}
		
		Control SquareButton ()
		{
			var control = new ImageButton{
				Image = Bitmap.FromResource ("Pablo.Formats.Character.Icons.Square.png"),
				Toggle = true,
				Pressed = IsSquare,
#if DESKTOP
				ToolTip = "Square aspect (shift)"
#endif
			};
			
			control.Click += delegate {
				IsSquare = control.Pressed;
			};
			return control;
		}

		Control CenteredButton ()
		{
			var control = new ImageButton{
				Image = Bitmap.FromResource ("Pablo.Formats.Character.Icons.Centered.png"),
				Toggle = true,
				Pressed = IsCentered,
#if DESKTOP
				ToolTip = "Centered (alt)"
#endif
			};
			
			control.Click += delegate {
				IsCentered = control.Pressed;
			};
			return control;
		}
		
		public override Control GeneratePad ()
		{
			var layout = new DynamicLayout (Padding.Empty);
			
			layout.BeginVertical (Padding.Empty, Size.Empty);
			layout.AddRow (SquareButton (), CenteredButton ());
			layout.EndVertical ();
			//layout.Add (base.GeneratePad ());
			
			return layout;
		}
	}
}
