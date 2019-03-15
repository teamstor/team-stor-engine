using Microsoft.Xna.Framework;

namespace TeamStor.Engine.Tween
{
	/// <summary>
	/// Value that can be tweened to other values.
	/// Based on https://github.com/danro/jquery-easing/blob/master/jquery.easing.js
	/// </summary>
	public class TweenedVector2
	{
		private TweenedDouble _x, _y;
		
		/// <summary>
		/// Current value.
		/// </summary>
		public Vector2 Value
		{
			get
			{
				return new Vector2(_x, _y);
			}
		}

		/// <summary>
		/// Target value.
		/// </summary>
		public Vector2 TargetValue
		{
			get
			{
				return new Vector2((float)_x.TargetValue, (float)_y.TargetValue);
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
		
		public TweenedVector2(Game game, Vector2 initialValue)
		{
			_x = new TweenedDouble(game, initialValue.X);
			_y = new TweenedDouble(game, initialValue.Y);
		}
		
		/// <summary>
		/// Tweens the value to the new value.
		/// </summary>
		public void TweenTo(Vector2 newValue, TweenEaseType easeType, double duration = 1.0f)
		{
			_x.TweenTo(newValue.X, easeType, duration);
			_y.TweenTo(newValue.Y, easeType, duration);
		}

		/// <summary>
		/// Eases with the specified ease type.
		/// </summary>
		public static Vector2 EaseWithType(TweenEaseType easeType, double amount, Vector2 startValue, Vector2 change)
		{
			return new Vector2((float)TweenedDouble.EaseWithType(easeType, amount, startValue.X, change.X), (float)TweenedDouble.EaseWithType(easeType, amount, startValue.Y, change.Y));
		}
		
		public static implicit operator Vector2(TweenedVector2 v)
		{
			return v.Value;
		}
	}
}