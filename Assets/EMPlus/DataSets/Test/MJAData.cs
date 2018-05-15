using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MJAData {
	public enum Field {
		ConcertGroup,
        Concert,
        Image,
        ImageTag,
        Person,
		Undefined
	};
	public static MontreuxJazzArchive archive = null;
	public static int fontResolution = 16;
	public static float textScale = 0.5f;
	public static float lineSpacing = 1.0f;
    public static float mainImageSize = 2.0f;
    public static int maxLines = 5;
    public static byte[] defaultImageData;
}
