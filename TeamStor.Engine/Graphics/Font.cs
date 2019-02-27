using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Utilities;
using SharpFont;

namespace TeamStor.Engine.Graphics
{
	/// <summary>
	/// Font character/glyph.
	/// </summary>
	public struct FontGlyph
	{
		/// <summary>
		/// The texture that this glyph is in.
		/// </summary>
		public int Texture;

		/// <summary>
		/// The rectangle inside of the texture that this glyph is in.
		/// </summary>
		public Rectangle CropInTexture;

		/// <summary>
		/// The amount to advance when drawing this glyph.
		/// </summary>
		public Vector2 Advance;

		/// <summary>
		/// The top-left of the actual texture inside of the crop rectangle.
		/// </summary>
		public Vector2 TextureOffset;

		/// <summary>
		/// Kerning for each character.
		/// </summary>
		public Dictionary<char, float> Kerning;
	}
	
	/// <summary>
	/// Font from .ttf and .otf files.
	/// </summary>
	public class Font : IDisposable
	{
		private static Library _sharpFontLibrary;
		private static int _sharpFontRefs;
		
		private Face _face;

		private const int TEXTURE_SIZE = 512;

		/// <summary>
		/// A loaded size of a font.
		/// </summary>
		public class LoadedSize
		{
			/// <summary>
			/// Textures that contain glyph textures.
			/// </summary>
			public List<Texture2D> Textures;
			
			/// <summary>
			/// The amount to advance when making a new line.
			/// </summary>
			public int LineHeight;

			/// <summary>
			/// Loaded glyphs for this size.
			/// </summary>
			public Dictionary<char, FontGlyph> Glyphs
			{
				get;
			} = new Dictionary<char, FontGlyph>();
		}

		private struct DrawData
		{
			public Point Position;
			public Rectangle CropInTexture;
			public Texture2D Texture;
		}

		private Dictionary<uint, LoadedSize> _loadedSizes
		{
			get;
		} = new Dictionary<uint, LoadedSize>();

		private GraphicsDevice _device;
		
		/// <summary>
		/// The default character map.
		/// </summary>
		public static readonly char[] DEFAULT_CHAR_MAP = new char[]
		{ 
			'A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f', 'G', 'g', 'H', 'h', 'I', 'i', 
			'J', 'j', 'K', 'k', 'L', 'l', 'M', 'm', 'N', 'n','O', 'o', 'P', 'p', 'Q', 'q', 'R', 'r',
			'S', 's', 'T', 't', 'U', 'u', 'V', 'v', 'W', 'w', 'X', 'x', 'Y', 'y','Z', 'z', 'Å', 'å', 
			'Ä', 'ä', 'Ö', 'ö', 'Ø', 'Æ',
			'1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
			'§', '½', '¶', '|', '<', '>', '!', '"', '@', '#', '£', '¤', '$', '%', '&', '/', '{', '(',
			'[', ']', ')', '}', '=', '+', '?', '\\', '´', '`', '¨', '^', '~', '\'', '*', '€', ',', ';', 
			'.', ':', '-', '_', ' ',
			'\n', '\r'
		};

		/// <summary>
		/// If kerning should be used when drawing.
		/// </summary>
		public bool UseKerning = true;

		/// <summary>
		/// The characters that will be generated for each size.
		/// </summary>
		public char[] CharacterMap
		{
			get;
			private set;
		}
		
		public Font(GraphicsDevice device, string filename) : this(device, filename, DEFAULT_CHAR_MAP)
		{
		}

		public Font(GraphicsDevice device, string filename, char[] charMap)
		{
			CharacterMap = charMap;
			
			if(_sharpFontRefs == 0)
				_sharpFontLibrary = new Library();
			_sharpFontRefs++;
			
			_face = new Face(_sharpFontLibrary, filename);
			_device = device;
		}

		public void Dispose()
		{
			foreach(uint size in _loadedSizes.Keys.ToArray())
				UnloadSize(size);
			
			_face.Dispose();
			
			_sharpFontRefs--;
			if(_sharpFontRefs == 0)
				_sharpFontLibrary.Dispose();
		}

		/// <param name="size">The size to check for.</param>
		/// <returns>True if the size is already generated.</returns>
		public bool IsSizeGenerated(uint size)
		{
			if(size == 0)
				throw new Exception("Size cannot be 0");
			
			return _loadedSizes.ContainsKey(size);
		}

		/// <param name="c">The character to check for.</param>
		/// <returns>True if the character is in the character map and can be drawn.</returns>
		public bool CanDrawCharacter(char c)
		{
			return CharacterMap.Contains(c);
		}
		
		private void RenderIntoColorData(Color[] data, FTBitmap bitmap, Vector2 pos)
		{			
			unsafe
			{
				byte* colors = (byte*)bitmap.Buffer;
				
				for(int y = 0; y < Math.Min(bitmap.Rows, TEXTURE_SIZE); y++)
				{
					for(int x = 0; x < Math.Min(bitmap.Width, TEXTURE_SIZE); x++)
					{
						if((int)(((y + pos.Y) * TEXTURE_SIZE) + (x + pos.X)) < data.Length)
							data[(int)(((y + pos.Y) * TEXTURE_SIZE) + (x + pos.X))] = 
								Color.White * (colors[(y * bitmap.Pitch) + x] / 255f);
					}
				}
			}
		}

		/// <summary>
		/// Generates a size for this font.
		/// </summary>
		/// <param name="size">The size to generate.</param>
		/// <returns>True if a new size was generated</returns>
		public bool GenerateSize(uint size)
		{
			if(size == 0)
				throw new Exception("Size cannot be 0");
			if(IsSizeGenerated(size))
				return false;
			
			LoadedSize loadedSize = new LoadedSize();
			loadedSize.LineHeight = (int)size;
			loadedSize.Textures = new List<Texture2D>();
						
			loadedSize.Textures.Add(new Texture2D(_device, TEXTURE_SIZE, TEXTURE_SIZE));
			
			Color[] data = new Color[TEXTURE_SIZE * TEXTURE_SIZE];

			int x = 0, y = 0;
			
			_face.SetPixelSizes(size, size);

			foreach(char c in CharacterMap)
			{
				FontGlyph glyph;

				uint index = _face.GetCharIndex(c);
				
				_face.LoadGlyph(index, LoadFlags.Color | LoadFlags.NoHinting | LoadFlags.NoAutohint, LoadTarget.Normal);
				_face.Glyph.RenderGlyph(RenderMode.Normal);
				
				glyph.CropInTexture = new Rectangle(x, y, _face.Glyph.Bitmap.Width, _face.Glyph.Bitmap.Rows);
				x += _face.Glyph.Bitmap.Width;

				if(x >= TEXTURE_SIZE)
				{
					x = 0;
					y += (int)(loadedSize.LineHeight * 1.5f);

					if(y + glyph.CropInTexture.Height >= TEXTURE_SIZE)
					{
						loadedSize.Textures.Last().SetData(data);
						loadedSize.Textures.Add(new Texture2D(_device, TEXTURE_SIZE, TEXTURE_SIZE));
						x = y = 0;
					}

					glyph.CropInTexture.X = x;
					glyph.CropInTexture.Y = y;
					
					x += _face.Glyph.Bitmap.Width;
				}
				
				glyph.Advance = new Vector2(_face.Glyph.Metrics.HorizontalAdvance.ToInt32(), _face.Glyph.Metrics.Height.ToInt32());
				glyph.TextureOffset = new Vector2(_face.Glyph.BitmapLeft, _face.Glyph.BitmapTop);
				glyph.Texture = loadedSize.Textures.Count - 1;
				glyph.Kerning = new Dictionary<char, float>();

				if(_face.HasKerning && UseKerning)
				{
					foreach(char c2 in CharacterMap)
					{
						if(c != c2)
						{
							int kern = _face.GetKerning(index, _face.GetCharIndex(c2), KerningMode.Default).X.ToInt32();
							glyph.Kerning.Add(c2, kern);
						}
					}
				}

				RenderIntoColorData(data, _face.Glyph.Bitmap,
					new Vector2(glyph.CropInTexture.Left, glyph.CropInTexture.Top));
				loadedSize.Glyphs.Add(c, glyph);
			}
			
			loadedSize.Textures.Last().SetData(data);
			_loadedSizes.Add(size, loadedSize);
			return true;
		}

		/// <summary>
		/// Unloads/disposes a generated size.
		/// </summary>
		/// <param name="size">The size to unload.</param>
		/// <returns>True if the size was actually unloaded.</returns>
		public bool UnloadSize(uint size)
		{
			if(size == 0)
				throw new Exception("Size cannot be 0");
			if(!IsSizeGenerated(size))
				return false;

			foreach(Texture2D texture in _loadedSizes[size].Textures)
				texture.Dispose();

			_loadedSizes.Remove(size);
			return true;
		}

        private List<DrawData> _drawDatasCache = new List<DrawData>();

		private List<DrawData> PositionText(uint size, string text, float lineMult = 1.25f, float spacing = 1)
		{
            if(_drawDatasCache.Capacity < 200)
                _drawDatasCache.Capacity = 200;
            if(_drawDatasCache.Capacity < text.Length)
                _drawDatasCache.Capacity = text.Length;

            _drawDatasCache.Clear();

			LoadedSize loadedSize;
			if(!_loadedSizes.TryGetValue(size, out loadedSize))
			{
				if(!GenerateSize(size))
					return _drawDatasCache;

				loadedSize = _loadedSizes[size];
			}

			float x = 0;
			float y = 0;
			
			for(int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				
				FontGlyph glyph;
				if(loadedSize.Glyphs.TryGetValue(c, out glyph))
				{
					if(c == '\n')
					{
						x = 0;
						y += loadedSize.LineHeight * lineMult;
					}
					else
					{
                        _drawDatasCache.Add(new DrawData()
                        {
                            Texture = loadedSize.Textures[glyph.Texture],
                            CropInTexture = glyph.CropInTexture,
                            Position = new Point(
                                (int)(x + glyph.TextureOffset.X), 
                                (int)(y + (loadedSize.LineHeight - glyph.TextureOffset.Y)))
                        });

						x += glyph.Advance.X * spacing;
						
						float kern;
						if(i != text.Length - 1 && UseKerning && glyph.Kerning.TryGetValue(text[i + 1], out kern))
							x += kern * spacing;
					}
				}
			}

			return _drawDatasCache;
		}

		/// <summary>
		/// Draws the text with the specified size.
		/// </summary>
		/// <param name="batch">SpriteBatch to draw with.</param>
		/// <param name="size">The font size to use.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="pos">The position to draw the text at.</param>
		/// <param name="color">The color to use when drawing the text.</param>
		/// <param name="lineMult">The mulitplier to use for line height.</param>
		/// <param name="spacing">The multiplier for the space between each letter.</param>
		public void Draw(SpriteBatch batch, uint size, string text, Vector2 pos, Color color, float lineMult = 1.25f, float spacing = 1f)
		{
			if(batch.Device != _device)
				return;

			foreach(DrawData data in PositionText(size, text, lineMult, spacing))
			{
				if(data.Texture != null)
					batch.Texture(new Vector2(data.Position.X, data.Position.Y) + pos, data.Texture, color, null, data.CropInTexture);
			}
		}
		
		/// <summary>
		/// Measures the size of the specified text.
		/// </summary>
		/// <param name="size">The font size to use.</param>
		/// <param name="text">The text to measure.</param>
		/// <param name="lineMult">The mulitplier to use for line height.</param>
		/// <param name="spacing">The multiplier for the space between each letter.</param>
		public Vector2 Measure(uint size, string text, float lineMult = 1.25f, float spacing = 1f)
		{
			Vector2 vec = new Vector2(0, 0);
			
			foreach(DrawData data in PositionText(size, text, lineMult, spacing))
			{
				vec.X = MathHelper.Max(vec.X, data.Position.X + data.CropInTexture.Width);
				vec.Y = MathHelper.Max(vec.Y, data.Position.Y + data.CropInTexture.Height);
			}

			return vec;
		}
	}
}