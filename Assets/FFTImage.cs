using UnityEngine;
using System.Collections;
using FFTWSharp;
using System.Runtime.InteropServices;

public class FFTImage : MonoBehaviour {
	private Texture2D _origTex;
	private Texture2D _spectrumTex;

	void OnEnable() {
		_origTex = (Texture2D)renderer.sharedMaterial.mainTexture;
		_spectrumTex = new Texture2D(_origTex.width, _origTex.width, TextureFormat.RGB24, false);
		_spectrumTex.filterMode = _origTex.filterMode;
		_spectrumTex.anisoLevel = _origTex.anisoLevel;
		renderer.sharedMaterial.mainTexture = _spectrumTex;
	}
	void OnDisable() {
		renderer.sharedMaterial.mainTexture = _origTex;
	}

	// Use this for initialization
	void Start () {
		var N = _origTex.width;
		var NbyN = N * N;

		var colors = _origTex.GetPixels();
		var dataInSpace = new float[2 * NbyN];
		for (var i = 0; i < NbyN; i++)
			dataInSpace[2 * i] = colors[i].r;

		var dataInFreq = new float[2 * NbyN];
		FFT(N, dataInSpace, dataInFreq, fftw_direction.Forward);
		FFT(N, dataInFreq, dataInSpace, fftw_direction.Backward);

		for (var i = 0; i < NbyN; i++) {
			var sx = dataInSpace[2 * i] / NbyN;
			var sy = dataInSpace[2 * i + 1];
			colors[i] = new Color(sx, sx, sx, 1f);
		}
		_spectrumTex.SetPixels(colors);
		_spectrumTex.Apply();
	}
	
	void FFT(int N, float[] dataIn, float[] dataOut, fftw_direction direction) {
		var NbyN = N * N;
		var bufFftIn = fftwf.malloc(8 * NbyN);
		var bufFftOut = fftwf.malloc(8 * NbyN);
		try {
			var plan = fftwf.dft_2d(N, N, bufFftIn, bufFftOut, direction, fftw_flags.Estimate);
			Marshal.Copy(dataIn, 0, bufFftIn, dataIn.Length);
			fftwf.execute(plan);
			Marshal.Copy(bufFftOut, dataOut, 0, dataOut.Length);
		} finally {
			fftwf.free(bufFftIn);
			fftwf.free(bufFftOut);
		}
	}
}
