using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TeamStor.Engine.Graphics
{
    /// <summary>
    /// Builds on top of MonoGame SpriteBatch.
    /// </summary>
    public class SpriteBatch
    {
        private Game _game;
        private Microsoft.Xna.Framework.Graphics.SpriteBatch _monoGameSpriteBatch;
        private Texture2D _emptyTexture;

        /// <summary>
        /// If the sprite batch should take the transform into account when rounding.
        /// </summary>
        public bool UseSmartRound { get; set; } = true;

        /// <summary>
        /// GraphicsDevice used by this SpriteBatch.
        /// </summary>
        public GraphicsDevice Device
        {
            get;
            private set;
        }

        /// <summary>
        /// If there are any items queued.
        /// </summary>
        public bool ItemsQueued
        {
            get;
            private set;
        }

        private SpriteSortMode _sortMode = SpriteSortMode.Deferred;
        
        /// <summary>
        /// MonoGame batch sort mode.
        /// </summary>
        public SpriteSortMode SortMode
        {
            get { return _sortMode; }
            set
            {
                if(value != _sortMode)
                {
                    bool itemsQueued = ItemsQueued;
                    if(itemsQueued)
                        End();
                    
                    _sortMode = value;
                    
                    if(itemsQueued)
                        ResetSpriteBatch();
                }
            }
        }
        
        private BlendState _blendState = BlendState.AlphaBlend;
        
        /// <summary>
        /// MonoGame batch blend state.
        /// </summary>
        public BlendState BlendState
        {
            get { return _blendState; }
            set
            {
                if(value != _blendState)
                {
                    bool itemsQueued = ItemsQueued;
                    if(itemsQueued)
                        End();
                    
                    _blendState = value;
                    
                    if(itemsQueued)
                        ResetSpriteBatch();
                }
            }
        }
        
        private SamplerState _samplerState = SamplerState.AnisotropicClamp;
        
        /// <summary>
        /// MonoGame batch sampler state.
        /// </summary>
        public SamplerState SamplerState
        {
            get { return _samplerState; }
            set
            {
                if(value != _samplerState)
                {            
                    bool itemsQueued = ItemsQueued;
                    if(itemsQueued)
                        End();
                    
                    _samplerState = value;
                    
                    if(itemsQueued)
                        ResetSpriteBatch();
                }
            }
        }
        
        private Effect _effect;
        
        /// <summary>
        /// Effect to use when drawing.
        /// Set to null to use the default effect.
        /// </summary>
        public Effect Effect
        {
            get { return _effect; }
            set
            {
                if(value != _effect)
                {
                    bool itemsQueued = ItemsQueued;
                    if(itemsQueued)
                        End();
                    
                    _effect = value;
                    
                    if(itemsQueued)
                        ResetSpriteBatch();
                }
            }
        }
        
        private Matrix _transform = Matrix.Identity;
        private Vector3 _decomposedTransformScale = new Vector3(1, 1, 1);
        
        /// <summary>
        /// Transformation to apply when drawing.
        /// </summary>
        public Matrix Transform
        {
            get { return _transform; }
            set
            {
                if(value != _transform)
                {
                    bool itemsQueued = ItemsQueued;
                    if(itemsQueued)
                        End();
                    
                    _transform = value;

                    Vector3 translation;
                    Quaternion rotation;

                    Transform.Decompose(out _decomposedTransformScale, out rotation, out translation);

                    if(itemsQueued)
                        ResetSpriteBatch();
                }
            }
        }

        private Rectangle? _scissor;
        
        /// <summary>
        /// Screen scissor to apply when drawing.
        /// </summary>
        public Rectangle? Scissor
        {
            get { return _scissor; }
            set
            {
                if(value != _scissor)
                {
                    bool itemsQueued = ItemsQueued;
                    if(itemsQueued)
                        End();
                    
                    _scissor = value;   
                    
                    if(itemsQueued)
                        ResetSpriteBatch();
                }
            }
        }
        
        private RenderTarget2D _renderTarget;
        
        /// <summary>
        /// Render target to draw to when drawing.
        /// </summary>
        public RenderTarget2D RenderTarget
        {
            get { return _renderTarget; }
            set
            {
                if(value != _renderTarget)
                {
                    bool itemsQueued = ItemsQueued;
                    if(itemsQueued)
                        End();
                    
                    _renderTarget = value;
                    
                    if(itemsQueued)
                        ResetSpriteBatch();
                }
            }
        }

        private RasterizerState _rastState = new RasterizerState();

        /// <summary>
        /// Sprite batch stats.
        /// </summary>
        public class BatchStats
        {
            /// <summary>
            /// Amount of drawn textures.
            /// </summary>
            public int DrawnTextures;
            
            /// <summary>
            /// Amount of times Start() was called on the MonoGame sprite batch.
            /// </summary>
            public int BatchStarts;
            
            /// <summary>
            /// Amount of times End() was called on the MonoGame sprite batch.
            /// </summary>
            public int BatchEnds;

            /// <summary>
            /// Amount of time spent in End() (GPU drawing).
            /// </summary>
            public double TimeInEnd;

            public void Reset()
            {
                DrawnTextures = BatchStarts = BatchEnds = 0;
                TimeInEnd = 0;
            }
        }

        /// <summary>
        /// Stats for this sprite batch.
        /// </summary>
        public BatchStats Stats
        {
            get;
        } = new BatchStats();

        public SpriteBatch(Game game)
        {
            _game = game;
            Device = game.GraphicsDevice;
            
            _monoGameSpriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(game.GraphicsDevice);
            _emptyTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            _emptyTexture.SetData(new Color[] { Color.White });

            _rastState.ScissorTestEnable = true;
            _rastState.MultiSampleAntiAlias = true;
        }

        private void ResetSpriteBatch()
        {
            if(ItemsQueued)
                End();

            Stats.BatchStarts++;
            _monoGameSpriteBatch.Begin(SortMode, BlendState, SamplerState, DepthStencilState.Default, _rastState, Effect, Transform);
            ItemsQueued = true;
        }

        /// <summary>
        /// Rounds a vector with integer precision based on the current transform.
        /// </summary>
        /// <param name="valueIn">Vector to round.</param>
        /// <returns>The rounded vector.</returns>
        public Vector2 SmartRound(Vector2 valueIn)
        {
            if(!UseSmartRound)
                return new Vector2((int)valueIn.X, (int)valueIn.Y);
            return new Vector2((int)(valueIn.X * _decomposedTransformScale.X) / _decomposedTransformScale.X, 
                (int)(valueIn.Y * _decomposedTransformScale.Y) / _decomposedTransformScale.Y);
        }
        
        /// <summary>
        /// Draws a texture at the specified position.
        /// </summary>
        /// <param name="pos">The position to draw the texture at.</param>
        /// <param name="texture">The texture to draw</param>
        /// <param name="scale">Amount to scale the texture by.</param>
        /// <param name="crop">The rectangle inside the texture to crop.</param>
        /// <param name="color">The color to tint the texture with.</param>
        /// <param name="rotation">The rotation to rotate the texture by.</param>
        /// <param name="rotationOrigin">The origin of the rotation inside the texture (0-1)</param>
        /// <param name="effect">The sprite effect to apply.</param>
        public void Texture(Vector2 pos, Texture2D texture, Color color = default(Color), Vector2? scale = null, Rectangle? crop = null, float rotation = 0f, Vector2? rotationOrigin = null, SpriteEffects effect = SpriteEffects.None)
        {
            if(!ItemsQueued)
                ResetSpriteBatch();

            Stats.DrawnTextures++;
            _monoGameSpriteBatch.Draw(texture, SmartRound(pos), crop, color, rotation, rotationOrigin.HasValue ? rotationOrigin.Value : Vector2.Zero, scale.HasValue ? scale.Value : Vector2.One, effect, 0f);
        }
        
        /// <summary>
        /// Draws a texture inside the specified rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to draw the texture in.</param>
        /// <param name="texture">The texture to draw</param>
        /// <param name="crop">The rectangle inside the texture to crop.</param>
        /// <param name="color">The color to tint the texture with.</param>
        /// <param name="rotation">The rotation to rotate the texture by.</param>
        /// <param name="rotationOrigin">The origin of the rotation inside the texture (0-1)</param>
        /// <param name="effect">The sprite effect to apply.</param>
        public void Texture(Rectangle rectangle, Texture2D texture, Color color = default(Color), Rectangle? crop = null, float rotation = 0f, Vector2? rotationOrigin = null, SpriteEffects effect = SpriteEffects.None)
        {
            if(!ItemsQueued)
                ResetSpriteBatch();

            Stats.DrawnTextures++;
            _monoGameSpriteBatch.Draw(texture, rectangle, crop, color, rotation, rotationOrigin.HasValue ? rotationOrigin.Value : Vector2.Zero, effect, 0f);
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to draw.</param>
        /// <param name="color">The color to draw the rectangle with.</param>
        /// <param name="rotation">The rotation to rotate the rectangle by.</param>
        /// <param name="rotationOrigin">The origin of the rotation inside the rectangle (0-1)</param>
        public void Rectangle(Rectangle rectangle, Color color, float rotation = 0f, Vector2? rotationOrigin = null)
        {
            Texture(rectangle, _emptyTexture, color, null, rotation, rotationOrigin);
        }
        
        /// <summary>
        /// Draws a point.
        /// </summary>
        /// <param name="pos">The position to draw the point at.</param>
        /// <param name="color">The color to draw the point with.</param>
        /// <param name="size">The size of the point.</param>
        public void Point(Vector2 pos, Color color, int size = 1)
        {
            Rectangle(new Rectangle((int)pos.X - (int)Math.Floor(size / 2f), (int)pos.Y - (int)Math.Floor(size / 2f), size, size), color);
        }
        
        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        /// <param name="color">The color to draw the line with.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public void Line(Vector2 start, Vector2 end, Color color, int thickness = 1)
        {
            // https://gamedev.stackexchange.com/questions/44015/how-can-i-draw-a-simple-2d-line-in-xna-without-using-3d-primitives-and-shders
            Vector2 edge = end - start;
            Rectangle(new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness), color, (float)Math.Atan2(edge.Y, edge.X));
        }
        
        /// <summary>
        /// Draws an outline around a rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to draw an outline around.</param>
        /// <param name="color">The color to draw the outline with.</param>
        /// <param name="thickness">The thickness of the outline.</param>
        /// <param name="inner">If the outline should be inside of the rectangle.</param>
        public void Outline(Rectangle rectangle, Color color, int thickness = 1, bool inner = true)
        {
            // -----
            // #   #
            // #   #
            // #####
            Rectangle(new Rectangle(rectangle.X - (inner ? 0 : thickness), rectangle.Y - (inner ? 0 : thickness), rectangle.Width + (inner ? 0 : thickness * 2), thickness), color);
            
            // -----
            // #   #
            // #   #
            // -----
            Rectangle(new Rectangle(rectangle.X - (inner ? 0 : thickness), rectangle.Y + rectangle.Height - (inner ? thickness : 0), rectangle.Width + (inner ? 0 : thickness * 2), thickness), color);
            
            // -----
            // -   #
            // -   #
            // -----
            Rectangle(new Rectangle(rectangle.X - (inner ? 0 : thickness), rectangle.Y + (inner ? thickness : 0), thickness, rectangle.Height - (inner ? thickness * 2 : 0)), color);
            
            // -----
            // -   -
            // -   -
            // -----
            Rectangle(new Rectangle(rectangle.X + rectangle.Width - (inner ? thickness : 0), rectangle.Y + (inner ? thickness : 0), thickness, rectangle.Height - (inner ? thickness * 2 : 0)), color);
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center">The center position of the circle.</param>
        /// <param name="size">The size of the circle.</param>
        /// <param name="color">The color of the circle.</param>
        /// <param name="thickness">The thickness of the circle.</param>
        public void Circle(Vector2 center, float size, Color color, int thickness = 1, float dividePrecision = 40)
        {
            for(float i = -1f; i <= 0.9f; i += 1 / dividePrecision)
            {
                Vector2 from = new Vector2(center.X + (float)Math.Sin(i * MathHelper.TwoPi) * size, center.Y + (float)Math.Cos(i * MathHelper.TwoPi) * size);
                Vector2 to = new Vector2(center.X + (float)Math.Sin((i + (1 / dividePrecision) + 0.015f) * MathHelper.TwoPi) * size, center.Y + (float)Math.Cos((i + (1 / dividePrecision) + 0.015f) * MathHelper.TwoPi) * size);
                
                Line(from, to, color, thickness);
            }
        }
        
        /// <summary>
        /// Draws text with the specified font.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="size">The font size.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="pos">The position to draw the text at.</param>
        /// <param name="color">The color to draw the text with.</param>
        /// <param name="lineMult">The line height multiplier.</param>
        /// <param name="spacing">The spacing multiplier.</param>
        public void Text(Font font, uint size, string text, Vector2 pos, Color color, float lineMult = 1.25f, float spacing = 1f)
        {
            font.Draw(this, size, text, pos, color, lineMult, spacing);
        }
        
        /// <summary>
        /// Mirrored by Game.DefaultFonts
        /// </summary>
        public enum FontStyle
        {
            Normal,
            Bold,
            Italic,
            ItalicBold,
            Mono,
            MonoBold
        }
        
        /// <summary>
        /// Draws text with the specified font style.
        /// </summary>
        /// <param name="fontStyle">The font style to use.</param>
        /// <param name="size">The font size.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="pos">The position to draw the text at.</param>
        /// <param name="color">The color to draw the text with.</param>
        /// <param name="lineMult">The line height multiplier.</param>
        /// <param name="spacing">The spacing multiplier.</param>
        public void Text(FontStyle fontStyle, uint size, string text, Vector2 pos, Color color, float lineMult = 1.25f, float spacing = 1f)
        {
            Font font = _game.DefaultFonts.Normal;

            switch(fontStyle)
            {
                case FontStyle.Bold:
                    font = _game.DefaultFonts.Bold;
                    break;
                    
                case FontStyle.Italic:
                    font = _game.DefaultFonts.Italic;
                    break;
                    
                case FontStyle.ItalicBold:
                    font = _game.DefaultFonts.ItalicBold;
                    break;
                    
                case FontStyle.Mono:
                    font = _game.DefaultFonts.Mono;
                    break;
                    
                case FontStyle.MonoBold:
                    font = _game.DefaultFonts.MonoBold;
                    break;
            }
            
            Text(font, size, text, pos, color, lineMult, spacing);
        }

        /// <summary>
        /// Draws all queued items.
        /// </summary>
        public void End()
        {
            if(ItemsQueued)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                
                if(Scissor.HasValue)
                    _game.GraphicsDevice.ScissorRectangle = Scissor.Value;
                if(RenderTarget != null)
                    _game.GraphicsDevice.SetRenderTarget(RenderTarget);

                Stats.BatchEnds++;
                _monoGameSpriteBatch.End();
                
                if(RenderTarget != null)
                    _game.GraphicsDevice.SetRenderTarget(null);
                
                _game.GraphicsDevice.ScissorRectangle = _game.GraphicsDevice.Viewport.Bounds;
                
                watch.Stop();
                Stats.TimeInEnd += watch.Elapsed.TotalMilliseconds;
            }

            ItemsQueued = false;
        }

        /// <summary>
        /// Resets the queue to its default state.
        /// </summary>
        public void Reset()
        {
            _sortMode = SpriteSortMode.Deferred;
            _blendState = BlendState.AlphaBlend;
            _samplerState = SamplerState.AnisotropicClamp;
            _effect = null;
            _transform = Matrix.Identity;
            _decomposedTransformScale = new Vector3(1, 1, 1);
            _scissor = null;
            _renderTarget = null;
            _rastState = new RasterizerState();
            _rastState.ScissorTestEnable = true;
            
            if(ItemsQueued)
                End();
        }
    }
}