using UnityEngine;
using System.Collections;
using FFTWSharp;
using System.Runtime.InteropServices;
using System.Text;

public class TestFftScale : MonoBehaviour {
	public const float TWO_PI = 2f * Mathf.PI;

	public int n = 32;
	public float freq = 4;

	void Start () {
		var fftin = fftwf.malloc(8 * n);
		var fftout = fftwf.malloc(8 * n);
		var planForward = fftwf.dft_1d(n, fftin, fftout, fftw_direction.Forward, fftw_flags.Estimate);

		var dataIn0 = new float[2 * n];
		var dw = freq * TWO_PI / n;
		for (var i = 0; i < n; i++) {
			var offset = 2 * i;
			dataIn0[offset] = Mathf.Cos(i * dw);
			dataIn0[offset + 1] = 0f;
		}
		Marshal.Copy(dataIn0, 0, fftin, dataIn0.Length);
		fftwf.execute(planForward);

		var dataOut = new float[2 * n];
		Marshal.Copy(fftout, dataOut, 0, dataOut.Length);

		var planBackward = fftwf.dft_1d (n, fftout, fftin, fftw_direction.Backward, fftw_flags.Estimate);
		fftwf.execute(planBackward);
		var dataIn1 = new float[dataIn0.Length];
		Marshal.Copy(fftin, dataIn1, 0, dataIn1.Length);

		var strBufScale = new StringBuilder("Scale : ");
		var strBufFreq = new StringBuilder("Freq : ");
		for (var i = 0; i < n; i++) {
			var i0 = 2 * i;
			strBufScale.AppendFormat("({0:f2},", dataIn0[i0] == 0f ? 0f : (dataIn1[i0] / dataIn0[i0]));
			strBufScale.AppendFormat(" {0:f2})\t", dataIn0[i0+1] == 0f ? 0f : (dataIn1[i0+1] / dataIn0[i0+1]));
			strBufFreq.AppendFormat("({0:f2}, {1:f2})\t", dataOut[i0], dataOut[i0+1]);
		}
		Debug.Log(strBufScale);
		Debug.Log(strBufFreq);
	}

	[DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
	public static extern void CopyMemory(System.IntPtr dest, System.IntPtr src, uint count);
}
