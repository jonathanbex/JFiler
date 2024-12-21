using System.Text;
using System.Security.Cryptography;
namespace JFiler.Domain.Utilities
{
  public static class CryptoUtility
  {
    public static string ComputeSHA256Hash(string input)
    {
      // Create a new instance of the MD5CryptoServiceProvider object.
      var hasher = new SHA256CryptoServiceProvider();

      // Convert the input string to a byte array and compute the hash.
      byte[] data = hasher.ComputeHash(Encoding.Default.GetBytes(input));

      // Create a new Stringbuilder to collect the bytes
      // and create a string.
      var sBuilder = new StringBuilder();

      // Loop through each byte of the hashed data 
      // and format each one as a hexadecimal string.
      for (int i = 0; i < data.Length; i++)
      {
        sBuilder.Append(data[i].ToString("x2"));
      }

      // Return the hexadecimal string.
      return sBuilder.ToString();
    }

    public static bool VerifySHA256Hash(string input, string hashandsalt)
    {
      // Hash the input.
      string hashOfInput = ComputeSHA256Hash(input);

      // Create a StringComparer an compare the hashes.
      StringComparer comparer = StringComparer.OrdinalIgnoreCase;

      if (0 == comparer.Compare(hashOfInput, hashandsalt))
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public static string GetSalt()
    {
      var gen = new System.Security.Cryptography.RNGCryptoServiceProvider();
      var salt = new Byte[24];
      gen.GetBytes(salt);
      return Convert.ToBase64String(salt);
    }
  }
}
