using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetsTools.NET.Texture;
using TexturePlugin;

namespace HelloWorld
{
    class Program
    {
        public static void AddToBundleFile(string filePathTo, string filePathFrom, bool autoloadtexture)
        {
            var manager = new AssetsManager();

            var bunInstTo = manager.LoadBundleFile(filePathTo, true);
            AssetsFileInstance afileInstTo = manager.LoadAssetsFileFromBundle(bunInstTo, 0, false);
            AssetsFile afileTo = afileInstTo.file;


            var bunInstFrom = manager.LoadBundleFile(filePathFrom, true);
            AssetsFileInstance afileInstFrom = manager.LoadAssetsFileFromBundle(bunInstFrom, 0, false);
            AssetsFile afileFrom = afileInstFrom.file;
            foreach (var assetInfo in afileFrom.GetAssetsOfType(AssetClassID.Texture2D))
            {
                var assetTypeValueField = manager.GetBaseField(afileInstFrom, assetInfo);
                var newAsset = AssetFileInfo.Create(afileTo, assetInfo.PathId, assetInfo.TypeId, null, false);
                if (autoloadtexture)
                {
                    Console.Out.WriteLine("Importing: " + assetTypeValueField["m_Name"].AsString + ".png");
                    ImportTexture(assetTypeValueField["m_Name"].AsString + ".png", afileInstTo, assetTypeValueField);
                }
                newAsset.SetNewData(assetTypeValueField);
                afileTo.Metadata.AddAssetInfo(newAsset);
                
            }
            foreach (var assetInfo in afileFrom.GetAssetsOfType(AssetClassID.Sprite))
            {
                var assetTypeValueField = manager.GetBaseField(afileInstFrom, assetInfo);
                var newAsset = AssetFileInfo.Create(afileTo, assetInfo.PathId, assetInfo.TypeId, null, false);
                newAsset.SetNewData(assetTypeValueField);
                afileTo.Metadata.AddAssetInfo(newAsset);
            }
            bunInstTo.file.BlockAndDirInfo.DirectoryInfos[0].SetNewData(afileTo);
            using (AssetsFileWriter writer = new AssetsFileWriter(filePathTo + "moduncom"))
            {
                bunInstTo.file.Write(writer, -1);
            }
            var newUncompressedBundle = new AssetBundleFile();
            newUncompressedBundle.Read(new AssetsFileReader(File.OpenRead(filePathTo + "moduncom")));

            using (AssetsFileWriter writer = new AssetsFileWriter(filePathTo + "mod"))
            {
                newUncompressedBundle.Pack(writer, AssetBundleCompressionType.LZ4);
            }

            newUncompressedBundle.Close();
        }

        public static void ImportTexture(String selectedFilePath, AssetsFileInstance FileInstance, AssetTypeValueField baseField)
        {
            TextureFormat fmt = (TextureFormat)baseField["m_TextureFormat"].AsInt;

            byte[] platformBlob = TextureHelper.GetPlatformBlob(baseField);
            uint platform = FileInstance.file.Metadata.TargetPlatform;

            int mips = 1;
            if (!baseField["m_MipCount"].IsDummy)
                mips = baseField["m_MipCount"].AsInt;

            byte[] encImageBytes = TextureImportExport.Import(selectedFilePath, fmt, out int width, out int height, ref mips, platform, platformBlob);

            if (encImageBytes == null)
            {
                Console.Out.WriteLine($"Failed to encode texture format {fmt}");
            return;
            }

            AssetTypeValueField m_StreamData = baseField["m_StreamData"];
            m_StreamData["offset"].AsInt = 0;
            m_StreamData["size"].AsInt = 0;
            m_StreamData["path"].AsString = "";

            if (!baseField["m_MipCount"].IsDummy)
                baseField["m_MipCount"].AsInt = mips;

            baseField["m_TextureFormat"].AsInt = (int)fmt;
            // todo: size for multi image textures
            baseField["m_CompleteImageSize"].AsInt = encImageBytes.Length;

            baseField["m_Width"].AsInt = width;
            baseField["m_Height"].AsInt = height;

            AssetTypeValueField image_data = baseField["image data"];
            image_data.Value.ValueType = AssetValueType.ByteArray;
            image_data.TemplateField.ValueType = AssetValueType.ByteArray;
            image_data.AsByteArray = encImageBytes;
        }

        public static void ReadFile(string filePath)
        {
            var manager = new AssetsManager();

            var bunInst = manager.LoadBundleFile(filePath, true);
            AssetsFileInstance afileInst = manager.LoadAssetsFileFromBundle(bunInst, 0, false);
            AssetsFile afile = afileInst.file;

            foreach (var texInfo in afile.GetAssetsOfType(AssetClassID.Texture2D))
            {
                var texBase = manager.GetBaseField(afileInst, texInfo);
                var name = texBase["m_Name"].AsString;
                var width = texBase["m_Width"].AsInt;
                var height = texBase["m_Height"].AsInt;
                Console.WriteLine($"Texture {name} is sized {width}x{height}");
            }
            foreach (var texInfo in afile.GetAssetsOfType(AssetClassID.Sprite))
            {
                var texBase = manager.GetBaseField(afileInst, texInfo);
                var name = texBase["m_Name"].AsString;
                Console.WriteLine($"Sprite {name}");
            }
        }
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("need a file argument");
                return;
            }
            if (args.Length == 1)
            {
                Console.WriteLine("Using '__data' as first argument...");
                args = new string[] { "__data", args[0] };
            }
            bool autoload = args.Length == 3 && args[2] == "true";
            AddToBundleFile(args[0], args[1], autoload);
            //ReadFile(args[0] + "mod");
        }
    }
}