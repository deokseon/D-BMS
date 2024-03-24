﻿using Cysharp.Threading.Tasks;
using DaVikingCode.RectanglePacking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace DaVikingCode.AssetPacker {

	public class AssetPacker : MonoBehaviour {

		public UnityEvent OnProcessCompleted;
		public float pixelsPerUnit = 100.0f;

		public bool useCache = false;
		public string cacheName = "";
		public int cacheVersion = 1;
		public bool deletePreviousCacheVersion = true;

		protected Dictionary<string, Sprite> mSprites;
		protected List<TextureToPack>[] itemsToRaster;

		private int currentIndex = 0;

		protected bool allow4096Textures = false;

		public void AddTextureToPack(string file, string customID = null)
		{
			string fileName = Path.GetFileNameWithoutExtension(file);
			if (fileName.StartsWith("key1") || fileName.StartsWith("key2") || fileName.CompareTo("barline") == 0 || fileName.CompareTo("panel-bg") == 0 ||
					 fileName.StartsWith("longnote") || fileName.CompareTo("keyfeedback") == 0 || fileName.StartsWith("panelbottom-") || fileName.CompareTo("verticalline") == 0)
			{
				itemsToRaster[0].Add(new TextureToPack(file, customID != null ? customID : fileName));
			}
			else
            {
				itemsToRaster[1].Add(new TextureToPack(file, customID != null ? customID : fileName));
			}
		}

		public void AddTexturesToPack(string[] files) {
			int listCapacity = files.Length;
			itemsToRaster = new List<TextureToPack>[2];
			for (int i = 0; i < 2; i++)
			{
				itemsToRaster[i] = new List<TextureToPack>(i == 0 ? 14 : listCapacity - 14);
			}
			mSprites = new Dictionary<string, Sprite>(listCapacity);

			foreach (string file in files)
				AddTextureToPack(file);
		}

		public void Process(bool allow4096Textures = false) {

			this.allow4096Textures = allow4096Textures;

			/*if (useCache) {

				if (cacheName == "")
					throw new Exception("No cache name specified");

				string path = Application.persistentDataPath + "/AssetPacker/" + cacheName + "/" + cacheVersion + "/";

				bool cacheExist = Directory.Exists(path);

				if (!cacheExist)
					StartCoroutine(createPack(path));
				else
					StartCoroutine(loadPack(path));
				
			} else*/
			_ = createPack(0);
			_ = createPack(1);
		}

		private Texture2D FlipTexture(Texture2D originalTexture)
		{
			Texture2D rotatedTex = new Texture2D(originalTexture.width, originalTexture.height);

			Color[] pixels = originalTexture.GetPixels();
			Array.Reverse(pixels);
			rotatedTex.SetPixels(pixels);

			rotatedTex.Apply();
			return rotatedTex;
		}

		protected async UniTask createPack(int itemIndex, string savePath = "")
        {
			var token = this.GetCancellationTokenOnDestroy();
			BMSGameManager bmsGameManager = FindObjectOfType<BMSGameManager>();
			if (savePath != "")
			{

				if (deletePreviousCacheVersion && Directory.Exists(Application.persistentDataPath + "/AssetPacker/" + cacheName + "/"))
					foreach (string dirPath in Directory.GetDirectories(Application.persistentDataPath + "/AssetPacker/" + cacheName + "/", "*", SearchOption.AllDirectories))
						Directory.Delete(dirPath, true);

				Directory.CreateDirectory(savePath);
			}

			List<Texture2D> textures = new List<Texture2D>();
			List<string> images = new List<string>();

			foreach (TextureToPack itemToRaster in itemsToRaster[itemIndex])
			{
				var uwr = await UnityWebRequestTexture.GetTexture("file:///" + itemToRaster.file).SendWebRequest().WithCancellation(cancellationToken: token);

				if (itemToRaster.id.CompareTo("longnotestart1") == 0 || itemToRaster.id.CompareTo("longnotestart2") == 0)
				{
					Texture2D longNoteBottomTexture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
					textures.Add(longNoteBottomTexture);
					images.Add("longnotebottom" + itemToRaster.id[13]);

					Texture2D longNoteTopTexture = FlipTexture(longNoteBottomTexture);
					textures.Add(longNoteTopTexture);
					images.Add("longnotetop" + itemToRaster.id[13]);
				}
				else
				{
					textures.Add(((DownloadHandlerTexture)uwr.downloadHandler).texture);
					images.Add(itemToRaster.id);
				}

				if (bmsGameManager != null)
				{
					lock (bmsGameManager.threadLock)
					{
						BMSGameManager.currentLoading++;
					}
				}
			}

			await UniTask.WaitUntil(() => itemIndex == 0 ? GameUIManager.isCreateReady : (currentIndex == 1), cancellationToken: token);

			int textureSize = allow4096Textures ? 4096 : 2048;

			List<Rect> rectangles = new List<Rect>();
			for (int i = 0; i < textures.Count; i++)
				if (textures[i].width > textureSize || textures[i].height > textureSize)
					throw new Exception("A texture size is bigger than the sprite sheet size!");
				else
					rectangles.Add(new Rect(0, 0, textures[i].width, textures[i].height));

			const int padding = 8;

			int numSpriteSheet = 0;
			while (rectangles.Count > 0)
			{

				Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, itemIndex == 0 ? false : true);
				texture.filterMode = itemIndex == 0 ? FilterMode.Point : FilterMode.Trilinear;
				Color32[] fillColor = texture.GetPixels32();
				for (int i = 0; i < fillColor.Length; ++i)
					fillColor[i] = Color.clear;

				RectanglePacker packer = new RectanglePacker(texture.width, texture.height, padding);

				for (int i = 0; i < rectangles.Count; i++)
					packer.insertRectangle((int)rectangles[i].width, (int)rectangles[i].height, i);

				packer.packRectangles();

				if (packer.rectangleCount > 0)
				{

					texture.SetPixels32(fillColor);
					IntegerRectangle rect = new IntegerRectangle();
					List<TextureAsset> textureAssets = new List<TextureAsset>();

					List<Rect> garbageRect = new List<Rect>();
					List<Texture2D> garabeTextures = new List<Texture2D>();
					List<string> garbageImages = new List<string>();

					for (int j = 0; j < packer.rectangleCount; j++)
					{

						rect = packer.getRectangle(j, rect);

						int index = packer.getRectangleId(j);

						texture.SetPixels32(rect.x, rect.y, rect.width, rect.height, textures[index].GetPixels32());

						TextureAsset textureAsset = new TextureAsset();
						textureAsset.x = rect.x;
						textureAsset.y = rect.y;
						textureAsset.width = rect.width;
						textureAsset.height = rect.height;
						textureAsset.name = images[index];

						textureAssets.Add(textureAsset);

						garbageRect.Add(rectangles[index]);
						garabeTextures.Add(textures[index]);
						garbageImages.Add(images[index]);
					}

					foreach (Rect garbage in garbageRect)
						rectangles.Remove(garbage);

					foreach (Texture2D garbage in garabeTextures)
						textures.Remove(garbage);

					foreach (string garbage in garbageImages)
						images.Remove(garbage);

					texture.Apply(itemIndex == 0 ? false : true);

					if (savePath != "")
					{

						File.WriteAllBytes(savePath + "/data" + numSpriteSheet + ".png", texture.EncodeToPNG());
						File.WriteAllText(savePath + "/data" + numSpriteSheet + ".json", JsonUtility.ToJson(new TextureAssets(textureAssets.ToArray())));
						++numSpriteSheet;
					}

					foreach (TextureAsset textureAsset in textureAssets)
					{
						mSprites.Add(textureAsset.name, Sprite.Create(texture, new Rect(textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height), GetSpritePivot(textureAsset.name), pixelsPerUnit, 0, SpriteMeshType.FullRect));
					}
				}

			}

			if (itemIndex == 0)
			{
				currentIndex++;
			}
			else
			{
				OnProcessCompleted.Invoke();
			}
		}

		private Vector2 GetSpritePivot(string name)
        {
			Vector2 pivot;
			//(name.Length >= 6 && name.Substring(0, 6).CompareTo("hpbar-") == 0)
			if (name.CompareTo("panel-left") == 0 || name.CompareTo("longnotebodyverticalline") == 0)  // pivot - right
			{
				pivot = new Vector2(1.0f, 0.5f);
			}
			else if (name.CompareTo("panel-right") == 0)  // pivot - left
			{
				pivot = new Vector2(0.0f, 0.5f);
			}
			else if (name.CompareTo("panel-bg") == 0 || name.StartsWith("hpbar-") ||
					 name.CompareTo("keyfeedback") == 0 || name.CompareTo("judgeline") == 0 ||
					 name.CompareTo("note1") == 0 || name.CompareTo("note2") == 0 || name.CompareTo("ReplayNote") == 0 ||
					 name.CompareTo("longnotebottom1") == 0 || name.CompareTo("longnotebottom2") == 0 ||
					 name.CompareTo("longnotetop1") == 0 || name.CompareTo("longnotetop2") == 0)  // pivot - bottom
			{
				pivot = new Vector2(0.5f, 0.0f);
			}
			else if (name.StartsWith("key1") || name.StartsWith("key2") || name.CompareTo("inputblock") == 0 ||
					 name.CompareTo("longnotebody1") == 0 || name.CompareTo("longnotebody2") == 0 || name.StartsWith("panelbottom-"))  // pivot - top
			{
				pivot = new Vector2(0.5f, 1.0f);
			}
			else // pivot - center
            {
				pivot = new Vector2(0.5f, 0.5f);
			}

			return pivot;
        }

		/*protected IEnumerator loadPack(string savePath) {
			
			int numFiles = Directory.GetFiles(savePath).Length;

			for (int i = 0; i < numFiles / 2; ++i) {

				WWW loaderTexture = new WWW("file:///" + savePath + "/data" + i + ".png");
				yield return loaderTexture;

				WWW loaderJSON = new WWW("file:///" + savePath + "/data" + i + ".json");
				yield return loaderJSON;

				TextureAssets textureAssets = JsonUtility.FromJson<TextureAssets> (loaderJSON.text);

				Texture2D t = loaderTexture.texture; // prevent creating a new Texture2D each time.
				foreach (TextureAsset textureAsset in textureAssets.assets)
					mSprites.Add(textureAsset.name, Sprite.Create(t, new Rect(textureAsset.x, textureAsset.y, textureAsset.width, textureAsset.height), Vector2.zero, pixelsPerUnit, 0, SpriteMeshType.FullRect));
			}

			yield return null;

			OnProcessCompleted.Invoke();
		}*/

		public void Dispose() {

			foreach (var asset in mSprites)
				Destroy(asset.Value.texture);

			mSprites.Clear();
		}

		public Sprite GetSprite(string id) {
			Sprite sprite = null;

			mSprites.TryGetValue (id, out sprite);

			return sprite;
		}

		public Sprite[] GetSprites(string prefix) {

			List<string> spriteNames = new List<string>();
			foreach (var asset in mSprites)
				if (asset.Key.StartsWith(prefix))
					spriteNames.Add(asset.Key);

			spriteNames.Sort(StringComparer.Ordinal);

			List<Sprite> sprites = new List<Sprite>();
			Sprite sprite;
			for (int i = 0; i < spriteNames.Count; ++i) {

				mSprites.TryGetValue(spriteNames[i], out sprite);

				sprites.Add(sprite);
			}

			return sprites.ToArray();
		}
	}
}
