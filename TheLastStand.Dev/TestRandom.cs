using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Dev;

public class TestRandom : MonoBehaviour
{
	[SerializeField]
	private int pixWidth = 128;

	[SerializeField]
	private int pixHeight = 128;

	[SerializeField]
	private bool usePerlin = true;

	private Texture2D noiseTex;

	private Color[] pix;

	private Renderer rend;

	private void Start()
	{
		rend = GetComponent<Renderer>();
		InitTexture();
	}

	[ContextMenu("InitTexture")]
	private void InitTexture()
	{
		noiseTex = new Texture2D(pixWidth, pixHeight);
		noiseTex.filterMode = (usePerlin ? FilterMode.Bilinear : FilterMode.Point);
		pix = new Color[noiseTex.width * noiseTex.height];
		rend.material.mainTexture = noiseTex;
	}

	private void CalcNoise()
	{
		int i = 0;
		float num = -1f;
		for (; i < noiseTex.height; i++)
		{
			for (int j = 0; j < noiseTex.width; j++)
			{
				num = RandomManager.GetHashedWhiteNoise(j, i);
				pix[i * noiseTex.width + j] = new Color(num, num, num);
			}
		}
		noiseTex.SetPixels(pix);
		noiseTex.Apply();
	}

	private void Update()
	{
		CalcNoise();
	}
}
