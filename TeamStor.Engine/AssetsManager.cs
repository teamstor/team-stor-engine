using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using TeamStor.Engine.Graphics;
using Microsoft.Xna.Framework;
using TeamStor.Engine.Graphics;
using System.Threading;

namespace TeamStor.Engine
{
	/// <summary>
	/// Manages asset loading and unloading.
	/// </summary>
	public class AssetsManager : IDisposable
	{
        private FileSystemWatcher _watcher;

		private struct LoadedAsset
		{
			public LoadedAsset(IDisposable asset, string path, bool keepAfterStateChange)
			{
				Asset = asset;
                Path = path;
				KeepAfterStateChange = keepAfterStateChange;
			}
			
			public IDisposable Asset;
            public string Path;
			public bool KeepAfterStateChange;
		}
		
		private Dictionary<string, LoadedAsset> _loadedAssets = new Dictionary<string, LoadedAsset>();

		/// <summary>
		/// Game class.
		/// </summary>
		public Game Game
		{
			get;
			private set;
		}
		
		/// <summary>
		/// The directory to load assets from.
		/// </summary>
		public string Directory
		{
			get;
			private set;
		}

		/// <summary>
		/// Amount of loaded assets.
		/// </summary>
		public int LoadedAssets
		{
			get { return _loadedAssets.Count; }
		}
		
		/// <summary>
		/// Amount of state-specific loaded assets.
		/// </summary>
		public int StateLoadedAssets
		{
			get { return _loadedAssets.Count(asset => !asset.Value.KeepAfterStateChange); }
		}

        public class AssetChangedEventArgs : EventArgs
        {
            public string Name;
            public IDisposable Asset;
        }

        public event EventHandler<AssetChangedEventArgs> AssetChanged;
		
		/// <param name="game">Game class.</param>
		/// <param name="dir">The directory to load assets from.</param>
		public AssetsManager(Game game, string dir)
		{
			Game = game;
			Directory = dir;

			Game.OnStateChange += OnStateChange;

            _watcher = new FileSystemWatcher(Directory);
            _watcher.EnableRaisingEvents = true;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.IncludeSubdirectories = true;
            _watcher.Changed += OnAssetChanged;
		}

        private void OnAssetChanged(object sender, FileSystemEventArgs e)
        {
            if(e.ChangeType == WatcherChangeTypes.Changed)
            {
                bool canOpen = false;
                for(int i = 0; i < 5; i++)
                {
                    try
                    {
                        FileStream stream = new FileStream(e.FullPath, FileMode.Open);
                        stream.Dispose();
                        canOpen = true;
                    }
                    catch
                    {
                        canOpen = false;
                        Thread.Sleep(500 * (i + 1));
                    }
                }

                if(!canOpen)
                    return;

                foreach(KeyValuePair<string, LoadedAsset> pair in _loadedAssets)
                {
                    if(Path.GetFullPath(e.FullPath) == Path.GetFullPath(Directory + "/" + pair.Value.Path))
                    {
                        ReloadAsset(pair.Value.Path);
                        if(AssetChanged != null)
                            AssetChanged(this, new AssetChangedEventArgs() {
                                Name = pair.Value.Path,
                                Asset = _loadedAssets[pair.Value.Path].Asset
                            });
                        break;
                    }
                }
            }
        }

        public void Dispose()
		{
			foreach(KeyValuePair<string, LoadedAsset> asset in _loadedAssets)
				asset.Value.Asset.Dispose();
			
			_loadedAssets.Clear();

            Game.OnStateChange -= OnStateChange;

            _watcher.Dispose();
        }

        /// <summary>
        /// Loads or gets an asset with the specified name and type.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        /// <typeparam name="T">The type of the asset (can be Texture2D, Font, SoundEffect, Song or Effect)</typeparam>
        /// <returns>The asset</returns>
        public T Get<T>(string name, bool keepAfterStateChange = false) where T : class, IDisposable
		{
			T asset = null;
			if(TryLoadAsset<T>(name, out asset, keepAfterStateChange))
				return asset;
			
			throw new Exception("Asset with name \"" + name + "\" couldn't be loaded.");
		}

		/// <summary>
		/// Tries to load an asset with the specified name and type.
		/// </summary>
		/// <param name="name">The name of the asset.</param>
		/// <param name="asset">The returned asset, or null if it couldn't be loaded.</param>
		/// <typeparam name="T">The type of the asset (can be Texture2D, Font, SoundEffect, Song or Effect)</typeparam>
		/// <returns>true if the asset was loaded</returns>
		public bool TryLoadAsset<T>(string name, out T asset, bool keepAfterStateChange = false) where T : class, IDisposable
		{
            asset = null;

			if(_loadedAssets.ContainsKey(name))
			{
				if(_loadedAssets[name].Asset is T)
				{
					asset = (T)_loadedAssets[name].Asset;
					
					if(keepAfterStateChange && !_loadedAssets[name].KeepAfterStateChange)
					{
						LoadedAsset modAsset = _loadedAssets[name];
						modAsset.KeepAfterStateChange = true;
						_loadedAssets.Remove(name);
						_loadedAssets.Add(name, modAsset);
					}
					
					return true;
				}

				return false;
			}
			
			if(!File.Exists(Directory + "/" + name))
				return false;

			try
			{
				if(typeof(T) == typeof(Texture2D))
				{
					using(FileStream stream = new FileStream(Directory + "/" + name, FileMode.Open,
                                      FileAccess.Read,
                                      FileShare.ReadWrite))
					{
						Texture2D texture = Texture2D.FromStream(Game.GraphicsDevice, stream);
                        Color[] data = new Color[texture.Width * texture.Height];
                        texture.GetData(data);

                        for(int i = 0; i < data.Length; i++)
                            data[i] = Color.FromNonPremultiplied(data[i].ToVector4());

                        texture.SetData(data);

                        asset = texture as T;
						_loadedAssets.Add(name, new LoadedAsset(asset, name, keepAfterStateChange));
						return true;
					}
				}
				
				if(typeof(T) == typeof(Font))
				{
					asset = new Font(Game.GraphicsDevice, Directory + "/" + name) as T;
					_loadedAssets.Add(name, new LoadedAsset(asset, name, keepAfterStateChange));
					return true;
				}
				
				if(typeof(T) == typeof(SoundEffect))
				{
					using(FileStream stream = new FileStream(Directory + "/" + name, FileMode.Open,
                                      FileAccess.Read,
                                      FileShare.ReadWrite))
					{
						asset = SoundEffect.FromStream(stream) as T;
						_loadedAssets.Add(name, new LoadedAsset(asset, name, keepAfterStateChange));
						return true;
					}
				}
				
				if(typeof(T) == typeof(Song))
				{
					// https://stackoverflow.com/questions/5813657/xna-4-song-fromuri-containing-spaces/5829463#5829463
					ConstructorInfo ctor = typeof(Song).GetConstructor(
						BindingFlags.NonPublic | BindingFlags.Instance, null,
						new[] { typeof(string) }, null);
					asset = ctor.Invoke(new object[] { Directory + "/" + name }) as T;
					
					_loadedAssets.Add(name, new LoadedAsset(asset, name, keepAfterStateChange));
					return true;
				}
				
				if(typeof(T) == typeof(Effect))
				{
					asset = new Effect(Game.GraphicsDevice, File.ReadAllBytes(Directory + "/" + name)) as T;
					_loadedAssets.Add(name, new LoadedAsset(asset, name, keepAfterStateChange));
					return true;
				}

				return false;
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
		}

        /// <summary>
        /// Reloads an asset if it's loaded.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        /// <returns>True if an asset was actually reloaded.</returns>
        public bool ReloadAsset(string name)
        {
            if(!_loadedAssets.ContainsKey(name))
                return false;

            LoadedAsset oldAsset = _loadedAssets[name];
            UnloadAsset(name);

            Texture2D o1;
            Font o2;
            SoundEffect o3;
            Song o4;
            Effect o5;

            if(oldAsset.Asset is Texture2D)
                return TryLoadAsset<Texture2D>(name, out o1, oldAsset.KeepAfterStateChange);
            if(oldAsset.Asset is Font)
                return TryLoadAsset<Font>(name, out o2, oldAsset.KeepAfterStateChange);
            if(oldAsset.Asset is SoundEffect)
                return TryLoadAsset<SoundEffect>(name, out o3, oldAsset.KeepAfterStateChange);
            if(oldAsset.Asset is Song)
                return TryLoadAsset<Song>(name, out o4, oldAsset.KeepAfterStateChange);
            if(oldAsset.Asset is Effect)
                return TryLoadAsset<Effect>(name, out o5, oldAsset.KeepAfterStateChange);
            return false;
        }

        /// <summary>
        /// Unloads an asset if it's loaded.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        /// <returns>True if an asset was actually unloaded.</returns>
        public bool UnloadAsset(string name)
		{
			if(!_loadedAssets.ContainsKey(name))
				return false;

			_loadedAssets[name].Asset.Dispose();
            _loadedAssets.Remove(name);
			return true;
		}

		/// <summary>
		/// If this asset manager has the specified asset loaded.
		/// </summary>
		/// <param name="name">The name of the asset.</param>
		/// <returns>If the asset is loaded.</returns>
		public bool HasAssetLoaded(string name)
		{
			return _loadedAssets.ContainsKey(name);
		}
		
		/// <summary>
		/// If the specified asset will be kept after a state change.
		/// </summary>
		/// <param name="name">The name of the asset.</param>
		/// <returns>If the asset is loaded and will be kept after a state change.</returns>
		public bool IsAssetKeptAfterStateChange(string name)
		{
			return _loadedAssets.ContainsKey(name) && _loadedAssets[name].KeepAfterStateChange;
		}
		
		private void OnStateChange(object sender, Game.ChangeStateEventArgs e)
		{
			if(e.From != e.To)
			{
				List<string> toRemove = new List<string>();

				foreach(KeyValuePair<string, LoadedAsset> asset in _loadedAssets)
				{
					if(!asset.Value.KeepAfterStateChange)
					{
						asset.Value.Asset.Dispose();
						toRemove.Add(asset.Key);
					}
				}

				foreach(string s in toRemove)
					_loadedAssets.Remove(s);
			}
		}
	}
}