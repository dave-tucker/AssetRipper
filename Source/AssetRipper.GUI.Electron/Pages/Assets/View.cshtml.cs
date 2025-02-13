using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Audio;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Export.UnityProjects.Shaders;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.GUI.Electron.Pages.Assets
{
	public class ViewModel : PageModel
	{
		private readonly ILogger<ViewModel> _logger;
		public IUnityObjectBase Asset { get; private set; } = default!;

		public string AudioSource
		{
			get
			{
				if (Asset is IAudioClip clip && AudioClipDecoder.TryGetDecodedAudioClipData(clip, out byte[]? decodedAudioData, out string? extension))
				{
					return $"data:audio/{extension};base64,{Convert.ToBase64String(decodedAudioData, Base64FormattingOptions.None)}";
				}
				return "";
			}
		}

		public DirectBitmap ImageBitmap
		{
			get
			{
				switch (Asset)
				{
					case ITexture2D texture:
						{
							if (TextureConverter.TryConvertToBitmap(texture, out DirectBitmap bitmap))
							{
								return bitmap;
							}
						}
						goto default;
					case ITerrainData terrainData:
						return TerrainHeatmapExporter.GetBitmap(terrainData);
					default:
						return default;
				}
			}
		}

		public string Text
		{
			get
			{
				return Asset switch
				{
					IShader shader => DumpShaderDataAsText(shader),
					IMonoScript monoScript => DecompileMonoScript(monoScript),
					ITextAsset textAsset => textAsset.Script_C49,
					_ => "",
				};
			}
		}

		public ViewModel(ILogger<ViewModel> logger)
		{
			_logger = logger;
		}

		public IActionResult OnGet(string? path)
		{
			if (string.IsNullOrEmpty(path))
			{
				_logger.LogError("Path is null");
				return Redirect("/");
			}
			else if (Program.Ripper.IsLoaded && Program.Ripper.GameStructure.FileCollection.TryGetAsset(AssetPath.FromJson(path), out IUnityObjectBase? asset))
			{
				Asset = asset;
				return Page();
			}
			else
			{
				return NotFound();
			}
		}

		private static string DumpShaderDataAsText(IShader shader)
		{
			MemoryStream stream = new();
			DummyShaderTextExporter.ExportShader(shader, stream);

			return Encoding.UTF8.GetString(GetStreamData(stream));

			static ReadOnlySpan<byte> GetStreamData(MemoryStream stream)
			{
				if (stream.TryGetBuffer(out ArraySegment<byte> buffer))
				{
					return buffer;
				}
				else
				{
					return stream.ToArray();
				}
			}
		}

		private static string DecompileMonoScript(IMonoScript monoScript)
		{
			return EmptyScript.GetContent(monoScript);//Placeholder until actual decompilation is implemented.
		}
	}
}
