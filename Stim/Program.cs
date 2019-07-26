using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hybridizer.Runtime.CUDAImports;
using Stim.Controls;

namespace Stim
{
	public class Program
    {
        [STAThread]
		static void Main()
		{
            //H3.InstanciateNativeLibrary();
            //ulong h3HQ1 = H3.StringToH3("8f2830828052d25");
            //ulong h3HQ2 = H3.StringToH3("8f283082a30e623");
            //GeoCoord[] geoHQ1 = H3.H3ToGeoBoundary(h3HQ1);
            //GeoCoord[] geoHQ2 = H3.H3ToGeoBoundary(h3HQ2);

            ////h3ToGeo(h3HQ2, &geoHQ2);
            //Console.WriteLine(String.Format("origin: (%lf, %lf)\ndestination: (%lf, %lf)\ngrid distance:",
            //	H3.RadToDeg(geoHQ1[0].latitude),
            //	H3.RadToDeg(geoHQ1[0].longitude),
            //	H3.RadToDeg(geoHQ2[0].latitude),
            //	H3.RadToDeg(geoHQ2[0].longitude)));

            //TestCuda();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new WorldView());
            Application.Run(new WorldForm());

        }

        public static void TestCuda()
        {
            const int N = 1024 * 1024 * 32;
            float[] a = Enumerable.Range(0, N).Select(i => (float)i).ToArray();
            float[] b = Enumerable.Range(0, N).Select(i => 2.0F).ToArray();

            // Run
            HybRunner.Cuda().Wrap(new Program()).Add(a, b, N);

            cuda.DeviceSynchronize();


            // Assert
            for (int i = 0; i < N; ++i)
            {
                if (a[i] != (float)i + 3.0F)
                {
                    Console.Error.WriteLine("Error at {0} : {1} != {2}", i, a[i], (float)i + 3.0F);
                    Environment.Exit(6); // abort
                }
            }

            Console.Out.WriteLine("OK");

        }

        [EntryPoint]
        public static void Add(float[] a, float[] b, int N)
        {
            Parallel.For(0, N, i => a[i] += (b[i] + 1.0F));
        }

    }
}
