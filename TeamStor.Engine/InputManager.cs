using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TeamStor.Engine
{
	public enum MouseButton
	{
		Left,
		Middle,
		Right
	}
	
	/// <summary>
	/// Handles input between frames.
	/// </summary>
	public class InputManager
	{
		private class InputState
		{
			public InputState()
			{
				Mouse = new MouseState();
				Keyboard = new KeyboardState();
                Gamepads = new GamePadState[4];

            }
			
			public MouseState Mouse;
			public KeyboardState Keyboard;
            public GamePadState[] Gamepads;
		}

		private InputState _lastState = new InputState();
		private InputState _currentState = new InputState();
		
		private InputState _fixedUpdateLastState = new InputState();
		private InputState _fixedUpdateCurrentState = new InputState();
		
		/// <summary>
		/// If the input manager is in fixed update mode (during FixedUpdate()).
		/// </summary>
		public bool FixedUpdateMode;

		/// <summary>
		/// Current position of the mouse.
		/// </summary>
		public Vector2 MousePosition
		{
			get
			{
				Point result = _currentState.Mouse.Position;
				if(FixedUpdateMode)
					result = _fixedUpdateCurrentState.Mouse.Position;
				
				return new Vector2(result.X, result.Y);
			}
		}

        /// <summary>
        /// Last position of the mouse.
        /// </summary>
        public Vector2 PreviousMousePosition
        {
            get
            {
                Point result = _lastState.Mouse.Position;
                if(FixedUpdateMode)
                    result = _fixedUpdateLastState.Mouse.Position;

                return new Vector2(result.X, result.Y);
            }
        }

        /// <summary>
        /// The amount the mouse moved since the last frame.
        /// </summary>
        public Vector2 MouseDelta
		{
			get
			{
				Point result = _currentState.Mouse.Position - _lastState.Mouse.Position;
				if(FixedUpdateMode)
					result = _fixedUpdateCurrentState.Mouse.Position - _fixedUpdateLastState.Mouse.Position;

				return new Vector2(result.X, result.Y);
			}
		}

		/// <summary>
		/// Amount the mouse scroll since last frame.
		/// </summary>
		public int MouseScroll
		{
			get
			{
				if(FixedUpdateMode)
					return _fixedUpdateCurrentState.Mouse.ScrollWheelValue - _fixedUpdateLastState.Mouse.ScrollWheelValue;
				
				return _currentState.Mouse.ScrollWheelValue - _lastState.Mouse.ScrollWheelValue;
			}
		}

		public InputManager(Game game)
		{
			game.OnUpdateBeforeState += OnUpdateBeforeState;
			game.OnFixedUpdateBeforeState += OnFixedUpdateBeforeState;
			game.OnFixedUpdateAfterState += OnFixedUpdateAfterState;
		}

		private void OnUpdateBeforeState(object sender, Game.UpdateEventArgs e)
		{
			_lastState = _currentState;
			
			_currentState = new InputState();
			_currentState.Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
			_currentState.Keyboard = Keyboard.GetState();

            for(int i = 0; i < 4; i++)
                _currentState.Gamepads[i] = GamePad.GetState(i);
		}
		
		private void OnFixedUpdateBeforeState(object sender, Game.FixedUpdateEventArgs e)
		{
			FixedUpdateMode = true;
			
			_fixedUpdateLastState = _fixedUpdateCurrentState;

			_fixedUpdateCurrentState = new InputState();
			_fixedUpdateCurrentState.Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
			_fixedUpdateCurrentState.Keyboard = Keyboard.GetState();

            for(int i = 0; i < 4; i++)
                _fixedUpdateCurrentState.Gamepads[i] = GamePad.GetState(i);
        }

        private void OnFixedUpdateAfterState(object sender, Game.FixedUpdateEventArgs e)
		{
			FixedUpdateMode = false;
		}

		/// <param name="key">The key to check for.</param>
		/// <returns>True if the key is held down.</returns>
		public bool Key(Keys key)
		{
			if(FixedUpdateMode)
				return _fixedUpdateCurrentState.Keyboard.IsKeyDown(key);
			return _currentState.Keyboard.IsKeyDown(key);
		}
		
		/// <param name="key">The key to check for.</param>
		/// <returns>True if the key was pressed this frame.</returns>
		public bool KeyPressed(Keys key)
		{
			if(FixedUpdateMode)
				return _fixedUpdateCurrentState.Keyboard.IsKeyDown(key) &&
					_fixedUpdateLastState.Keyboard.IsKeyUp(key);
			return _currentState.Keyboard.IsKeyDown(key) &&
				_lastState.Keyboard.IsKeyUp(key);
		}
		
		/// <param name="key">The key to check for.</param>
		/// <returns>True if the key was released this frame.</returns>
		public bool KeyReleased(Keys key)
		{
			if(FixedUpdateMode)
				return _fixedUpdateCurrentState.Keyboard.IsKeyUp(key) &&
				       _fixedUpdateLastState.Keyboard.IsKeyDown(key);
			return _currentState.Keyboard.IsKeyUp(key) &&
			       _lastState.Keyboard.IsKeyDown(key);
		}

		/// <param name="button">The button to check for.</param>
		/// <returns>True if the button is held down.</returns>
		public bool Mouse(MouseButton button)
		{
			InputState input = _currentState;
			if(FixedUpdateMode)
				input = _fixedUpdateCurrentState;

			switch(button)
			{
				case MouseButton.Left:
					return input.Mouse.LeftButton == ButtonState.Pressed;
					
				case MouseButton.Middle:
					return input.Mouse.MiddleButton == ButtonState.Pressed;
					
				case MouseButton.Right:
					return input.Mouse.RightButton == ButtonState.Pressed;
			}

			return false;
		}
		
		/// <param name="button">The button to check for.</param>
		/// <returns>True if the button was pressed this frame.</returns>
		public bool MousePressed(MouseButton button)
		{
			InputState input = _currentState;
			if(FixedUpdateMode)
				input = _fixedUpdateCurrentState;
			
			InputState lastInput = _lastState;
			if(FixedUpdateMode)
				lastInput = _fixedUpdateLastState;

			switch(button)
			{
				case MouseButton.Left:
					return input.Mouse.LeftButton == ButtonState.Pressed &&
						lastInput.Mouse.LeftButton == ButtonState.Released;
					
				case MouseButton.Middle:
					return input.Mouse.MiddleButton == ButtonState.Pressed &&
					   lastInput.Mouse.MiddleButton == ButtonState.Released;	
					
				case MouseButton.Right:
					return input.Mouse.RightButton == ButtonState.Pressed &&
					   lastInput.Mouse.RightButton == ButtonState.Released;	
			}

			return false;
		}
		
		/// <param name="button">The button to check for.</param>
		/// <returns>True if the button was released this frame.</returns>
		public bool MouseReleased(MouseButton button)
		{
			InputState input = _currentState;
			if(FixedUpdateMode)
				input = _fixedUpdateCurrentState;
			
			InputState lastInput = _lastState;
			if(FixedUpdateMode)
				lastInput = _fixedUpdateLastState;

			switch(button)
			{
				case MouseButton.Left:
					return input.Mouse.LeftButton == ButtonState.Released &&
					       lastInput.Mouse.LeftButton == ButtonState.Pressed;
					
				case MouseButton.Middle:
					return input.Mouse.MiddleButton == ButtonState.Released &&
					       lastInput.Mouse.MiddleButton == ButtonState.Pressed;	
					
				case MouseButton.Right:
					return input.Mouse.RightButton == ButtonState.Released &&
					       lastInput.Mouse.RightButton == ButtonState.Pressed;	
			}

			return false;
		}

        /// <param name="gamepad">0-3, the gamepad to select</param>
        /// <returns>Current state of the specified gamepad.</returns>
        public GamePadState Gamepad(int gamepad)
        {
            InputState input = _currentState;
            if(FixedUpdateMode)
                input = _fixedUpdateCurrentState;

            return input.Gamepads[gamepad];
        }

        /// <param name="gamepad">0-3, the gamepad to select</param>
        /// <returns>Current state of the specified gamepad last frame.</returns>
        public GamePadState LastGamepad(int gamepad)
        {
            InputState input = _lastState;
            if(FixedUpdateMode)
                input = _fixedUpdateLastState;

            return input.Gamepads[gamepad];
        }
    }
}
