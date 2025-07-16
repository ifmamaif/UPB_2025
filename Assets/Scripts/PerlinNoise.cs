using System.Linq;

public static class PerlinNoise
{
    #region Constants
    private static readonly int[] HashTableKenPerlin =
    {
        /// First values between [0,255]
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
        140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
        247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
        57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
        74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
        60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
        65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
        200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
        52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
        207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
        119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
        129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
        218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
        81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
        184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
        222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
        /// For optimization
        /// To remove the checking part when we index the hashTable we double the array
        /// Same 256 values as above
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
        140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
        247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
        57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
        74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
        60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
        65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
        200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
        52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
        207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
        119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
        129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
        218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
        81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
        184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
        222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
    };

    private static readonly int[,] Directions =
    {
        { 0, 1, 1 }, { 0, 1, -1 }, { 0, -1, 1 }, { 0, -1, -1 },
        { 1, 0, 1 }, { 1, 0, -1 }, { -1, 0, 1 }, { -1, 0, -1 },
        { 1, 1, 0 }, { 1, -1, 0 }, { -1, 1, 0 }, { -1, -1, 0 }
    };
    #endregion

    #region Helper Functions

    private static int Floor(float d) => d > 0 ? (int)d : (int)d - 1;
    private static float IeeeRemainder(float x) => x - Floor(x);
    private static float DocProduct(float x, float y) => x * y;
    private static float DocProduct(int[] v1, float[] v2) => v1.Zip(v2, (x, y) => x * y).Sum();

    private static float DocProduct(int[,] v1, int elem, float[] v2) => Enumerable.Range(0, Directions.GetLength(1))
        .Select(col => v1[elem, col])
        .ToArray().Zip(v2, (x, y) => x * y).Sum();
    private static float QuinticInterpolationCurve(float t) => t * t * t * (t * (t * 6 - 15) + 10);
    private static float CubicHermiteCurve(float t) => t * t * (3 - 2 * t);
    private static float Lerp(float a, float b, float weight) => a + weight * (b - a);
    
    #endregion

    public static float Noise1D(float x)
    {
        var intX = Floor(x);
        var fX = IeeeRemainder(x);
        intX &= 255;

        var gradient1 = HashTableKenPerlin[intX] % 12;
        var gradient2 = HashTableKenPerlin[intX + 1] % 12;

        return Lerp(DocProduct(Directions[gradient1, 0], fX),
                    DocProduct(Directions[gradient2, 0], fX - 1),
                CubicHermiteCurve(x));
    }

    public static float Noise2D(float x, float y)
    {
        var intX = Floor(x);
        var intY = Floor(y);

        var fx = IeeeRemainder(x);
        var fy = IeeeRemainder(y);

        intX &= 255;
        intY &= 255;

        var gradient1 = HashTableKenPerlin[intX + HashTableKenPerlin[intY]] % 12;
        var gradient2 = HashTableKenPerlin[intX + HashTableKenPerlin[intY + 1]] % 12;
        var gradient3 = HashTableKenPerlin[intX + 1 + HashTableKenPerlin[intY]] % 12;
        var gradient4 = HashTableKenPerlin[intX + 1 + HashTableKenPerlin[intY + 1]] % 12;

        var xMinim = fx - 1f;
        var yMinim = fy - 1f;
        
        var contributionNoise1 = DocProduct(Directions, gradient1, new[]{ fx, fy});
        var contributionNoise2 = DocProduct(Directions, gradient2, new[] { fx, yMinim});
        var contributionNoise3 = DocProduct(Directions, gradient3, new[] { xMinim, fy});
        var contributionNoise4 = DocProduct(Directions, gradient4, new[] { xMinim, yMinim});

        var faded1 = CubicHermiteCurve(fx);
        var faded2 = CubicHermiteCurve(fy);

        var noise1 = Lerp(contributionNoise1, contributionNoise3, faded1);
        var noise2 = Lerp(contributionNoise2, contributionNoise4, faded1);

        var noiseFinal = Lerp(noise1, noise2, faded2);

        return noiseFinal;
    }
}