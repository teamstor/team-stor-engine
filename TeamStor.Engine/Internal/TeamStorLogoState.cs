using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TeamStor.Engine.Coroutine;
using TeamStor.Engine.Graphics;
using TeamStor.Engine.Tween;
using SpriteBatch = TeamStor.Engine.Graphics.SpriteBatch;

namespace TeamStor.Engine.Internal
{
    public class TeamStorLogoState : GameState
    {
        private static string _currentScreen = "screen1";
        private TweenedDouble _fade;

        private GameState _initialState;

        public TeamStorLogoState(GameState initialState)
        {
            _initialState = initialState;
        }

        public override void OnEnter(GameState previousState)
        {
            _fade = new TweenedDouble(Game, 0);
            //MediaPlayer.Play(Assets.Get<Song>("engine/intro/halo.ogg"));
            Coroutine.Start(LogoCoroutine);
        }

        public override void OnLeave(GameState nextState)
        {
            MediaPlayer.Stop();
        }

        private IEnumerator<ICoroutineOperation> LogoCoroutine()
        {
            yield return Wait.Seconds(Game, 0.35);
            _fade.TweenTo(1, TweenEaseType.Linear, 0.3f);
            yield return Wait.Seconds(Game, 3);
            _fade.TweenTo(0, TweenEaseType.Linear, 0.3f);
            yield return Wait.Seconds(Game, 1);
            
            _currentScreen = "screen2";
            yield return Wait.Seconds(Game, 0.25);
            _fade.TweenTo(1, TweenEaseType.Linear, 0.3f);
            yield return Wait.Seconds(Game, 3);
            _fade.TweenTo(0, TweenEaseType.Linear, 0.3f);
            yield return Wait.Seconds(Game, 1);

            yield return Wait.Seconds(Game, 0.5);
            Game.CurrentState = _initialState;
        }

        public override void Update(double deltaTime, double totalTime, long count)
        {
            if(Input.KeyPressed(Keys.Enter))
                Game.CurrentState = _initialState;
        }

        public override void FixedUpdate(long count)
        {
        }

        public override void Draw(Graphics.SpriteBatch batch, Vector2 screenSize)
        {
            batch.Rectangle(new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y), Color.Black);
            Rectangle rect = new Rectangle(0, 0, (int) (screenSize.Y * (16.0f / 9.0f)), (int) screenSize.Y);
            
            rect.X += (int)(screenSize.X / 2 - rect.Width / 2);
            rect.Y += (int)(screenSize.Y / 2 - rect.Height / 2);

            batch.Texture(rect, Assets.Get<Texture2D>("engine/intro/" + _currentScreen + ".png"), Color.White * _fade);
        }
    }
}
