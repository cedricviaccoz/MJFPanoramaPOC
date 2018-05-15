using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadTextureDXT : Object {
    public struct FormatDescription {
        public TextureFormat format;
        public int texWidth, texHeight;
        public int imgPosX, imgPosY, imgWidth, imgHeight;
        public int originalWidth, originalHeight;
    }

    public static Texture2D LoadWithSize(string path, int width, int height, TextureFormat format = TextureFormat.DXT1, bool verbose = false)
    {
        if (!File.Exists(path))
        {
            if (verbose)
                Debug.Log("Compressed image not found: " + path);
            return null;
        }
        Texture2D tex = new Texture2D(width, height, format, false);
#if (!UNITY_WSA) || UNITY_EDITOR
        tex.LoadRawTextureData(File.ReadAllBytes(path));
#else
        tex.LoadRawTextureData(UnityEngine.Windows.File.ReadAllBytes(path));
#endif
        tex.Apply();
        if (verbose)
            Debug.Log("Compressed image loaded: " + path + " size: " + width + "x" + height);
        return tex;
    }
    public static Texture2D Load(string path, bool verbose = false)
    {
        FormatDescription descr = new FormatDescription();
        return Load(path, ref descr, verbose);
    }

    public static Texture2D Load(string path, ref FormatDescription description,  bool verbose = false)
    {
        if (!File.Exists(path))
        {
            if (verbose)
                Debug.Log("Compressed image not found: " + path);
            return null;
        }
        string formatPath = path + ".format";
        if (!File.Exists(formatPath))
        {
            if (verbose)
                Debug.Log("Compressed image format file not found: " + formatPath);
            return null;
        }
        string[] formatLines = File.ReadAllLines(formatPath);
        if (formatLines.Length < 2)
        {
            if (verbose)
                Debug.Log("Compressed image format file invalid: " + formatPath);
            return null;
        }

        string[] texInfo = formatLines[0].Split(';');
        description.format      = (texInfo[0] == "DXT1" ? TextureFormat.DXT1 : TextureFormat.DXT5);
        description.texWidth    = int.Parse(texInfo[1]);
        description.texHeight   = int.Parse(texInfo[2]);

        Texture2D tex = new Texture2D(description.texWidth, description.texHeight, description.format, false);
#if (!UNITY_WSA) || UNITY_EDITOR
        tex.LoadRawTextureData(File.ReadAllBytes(path));
#else
        tex.LoadRawTextureData(UnityEngine.Windows.File.ReadAllBytes(path));
#endif
        tex.Apply();
        if (verbose)
            Debug.Log("Compressed image loaded: " + path + " size: " + description.texWidth + "x" + description.texHeight);
        return tex;
    }
}
