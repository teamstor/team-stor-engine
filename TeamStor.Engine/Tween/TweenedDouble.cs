using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStor.Engine.Tween
{
    public enum TweenEaseType
    {
        Linear,

        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,

        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,

        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,

        EaseInSine,
        EaseOutSine,
        EaseInOutSine,

        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,

        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc
    }

    /// <summary>
    /// Value that can be tweened to other values.
    /// Based on https://github.com/danro/jquery-easing/blob/master/jquery.easing.js
    /// </summary>
    public class TweenedDouble
    {
        private Game _game;
        private double _sourceValue;
        private double _duration;

        /// <summary>
        /// Current value.
        /// </summary>
        public double Value
        {
            get
            {
                if(_duration == 0)
                    return TargetValue;
                return EaseWithType(EaseType, MathHelper.Clamp(1.0f - (float)((CompletionTime - _game.Time) / _duration), 0f, 1f), _sourceValue, TargetValue - _sourceValue);
            }
        }

        /// <summary>
        /// Target value.
        /// </summary>
        public double TargetValue
        {
            get; private set;
        }

        /// <summary>
        /// Time when the tween will complete.
        /// </summary>
        public double CompletionTime
        {
            get; private set;
        }

        /// <summary>
        /// Current ease type.
        /// </summary>
        public TweenEaseType EaseType { get; private set; }

        /// <summary>
        /// If the current tween is completed.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return _game.Time >= CompletionTime;
            }
        }
        
        public TweenedDouble(Game game, double initialValue)
        {
            _game = game;
            CompletionTime = game.Time;
            _duration = 1.0f;

            _sourceValue = initialValue;
            TargetValue = initialValue;
            
            EaseType = TweenEaseType.Linear;
        }

        /// <summary>
        /// Tweens the value to the new value.
        /// </summary>
        public void TweenTo(double newValue, TweenEaseType easeType, double duration = 1.0f)
        {
            _sourceValue = Value;
            TargetValue = newValue;

            CompletionTime = _game.Time + duration;

            _duration = duration;
            EaseType = easeType;
        }

        /// <summary>
        /// Eases with the specified ease type.
        /// </summary>
        public static double EaseWithType(TweenEaseType easeType, double amount, double startValue, double change)
        {                
            switch(easeType)
            {
                case TweenEaseType.Linear:
                    return MathHelper.Lerp((float)startValue, (float)startValue + (float)change, (float)amount);

                // t: current time, b: begInnIng value, c: change In value, d: duration

                case TweenEaseType.EaseInQuad:
                    return change * amount * amount + startValue;
                case TweenEaseType.EaseOutQuad:
                    return -change * amount * (amount - 2) + startValue;
                case TweenEaseType.EaseInOutQuad:
                    if ((amount *= 2) < 1) return change/2 * amount * amount + startValue;
                    return -change / 2 * --amount * (amount-2 - 1) + amount;
                    
                case TweenEaseType.EaseInCubic:
                    return change * amount * amount * amount + startValue;
                case TweenEaseType.EaseOutCubic:
                    return change * ((amount = amount-1) * amount * amount + 1) + startValue;
                case TweenEaseType.EaseInOutCubic:
                    if ((amount *= 2) < 1) return change / 2 * amount * amount * amount + startValue;
                    return change / 2 * ((amount-=2) * amount * amount + 2) + startValue;
                    
                case TweenEaseType.EaseInQuart:
                   return change * amount * amount * amount * amount + startValue;
                case TweenEaseType.EaseOutQuart:
                    return -change * (amount = amount-1) * amount * amount * amount + startValue;
                case TweenEaseType.EaseInOutQuart:
                    if ((amount * 2) < 1) return change/2 * amount * amount * amount * amount + startValue;
                    return -change/2 * (amount -= 2 * amount * amount * amount - 2) + startValue;
                    
                case TweenEaseType.EaseInSine:
                    return -change * Math.Cos(amount * (Math.PI / 2)) + change + startValue;
                case TweenEaseType.EaseOutSine:
                    return change * Math.Sin(amount * (Math.PI / 2)) + startValue;
                case TweenEaseType.EaseInOutSine:
                    return -change / 2 * (Math.Cos(Math.PI * amount) - 1) + startValue;
                    
                case TweenEaseType.EaseInExpo:
                    return amount == 0 ? startValue : change * Math.Pow(2, 10 * (amount - 1)) + startValue;
                case TweenEaseType.EaseOutExpo:
                    return amount == 1 ? startValue + change : change * (-Math.Pow(2, -10 * amount) + 1) + startValue;
                case TweenEaseType.EaseInOutExpo:
                    if(amount == 0) return startValue;
                    if(amount == 1) return startValue + change;
                    if((amount *= 2) < 1) return change / 2 * Math.Pow(2, 10 * (amount - 1)) + startValue;
                    return change / 2 * (-Math.Pow(2, -10 * --amount) + 2) + startValue;
                    
                case TweenEaseType.EaseInCirc:
                    return -change * (Math.Sqrt(1 - amount * amount) - 1) + startValue;
                case TweenEaseType.EaseOutCirc:
                    return change * (Math.Sqrt(1 - (amount -= 1) * amount) - 1) + startValue;
                case TweenEaseType.EaseInOutCirc:
                    if ((amount *= 2) < 1) return -change / 2 * (Math.Sqrt(1 - amount * amount) - 1) + startValue;
                    return change / 2 * (Math.Sqrt(1 - (amount -= 2) * amount) + 1) + startValue;
            }
            return startValue + change;
        }
        
        public static implicit operator float(TweenedDouble v)
        {
            return (float)v.Value;
        }
        
        public static implicit operator double(TweenedDouble v)
        {
            return v.Value;
        }
    }
}
