using Microsoft.Xna.Framework;

namespace TeamStor.Engine.Tween
{
	/// <summary>
	/// Value that can be tweened to other values.
	/// Based on https://github.com/danro/jquery-easing/blob/master/jquery.easing.js
	/// </summary>
	public class TweenedRectangle
	{
		private TweenedDouble _x, _y, _w, _h;
		
		/// <summary>
		/// Current value.
		/// </summary>
		public Rectangle Value
		{
			get
			{
				return new Rectangle((int)_x, (int)_y, (int)_w, (int)_h);
			}
		}

		/// <summary>
		/// Target value.
		/// </summary>
		public Rectangle TargetValue
		{
			get
			{
				return new Rectangle((int)_x.TargetValue, (int)_y.TargetValue, (int)_w.TargetValue, (int)_h.TargetValue);
			}
		}

		/// <summary>
		/// Time when the tween will complete.
		/// </summary>
		public double CompletionTime
		{
			get { return _x.CompletionTime; }
		}

		/// <summary>
		/// Current ease type.
		/// </summary>
		public TweenEaseType EaseType
		{
			get { return _x.EaseType; }
		}
		
		/// <summary>
		/// If the current tween is completed.
		/// </summary>
		public bool IsComplete
		{
			get { return _x.IsComplete; }
		}
		
		public TweenedRectangle(Game game, Rectangle initialValue)
		{
			_x = new TweenedDouble(game, initialValue.X);
			_y = new TweenedDouble(game, initialValue.Y);
			_w = new TweenedDouble(game, initialValue.Width);
			_h = new TweenedDouble(game, initialValue.Height);
		}
		
		/// <summary>
		/// Tweens the value to the new value.
		/// </summary>
		public void TweenTo(Rectangle newValue, TweenEaseType easeType, double duration = 1.0f)
		{
			_x.TweenTo(newValue.X, easeType, duration);
			_y.TweenTo(newValue.Y, easeType, duration);
			_w.TweenTo(newValue.Width, easeType, duration);
			_h.TweenTo(newValue.Height, easeType, duration);
		}

		/// <summary>
		/// Eases with the specified ease type.
		/// </summary>
		public static Rectangle EaseWithType(TweenEaseType easeType, double amount, Rectangle startValue, Rectangle change)
		{
			return new Rectangle(
				(int)TweenedDouble.EaseWithType(easeType, amount, startValue.X, change.X), 
				(int)TweenedDouble.EaseWithType(easeType, amount, startValue.Y, change.Y), 
				(int)TweenedDouble.EaseWithType(easeType, amount, startValue.Width, change.Height),
				(int)TweenedDouble.EaseWithType(easeType, amount, startValue.Height, change.Height));
		}
		
		public static implicit operator Rectangle(TweenedRectangle v)
		{
			return v.Value;
		}
	}
}