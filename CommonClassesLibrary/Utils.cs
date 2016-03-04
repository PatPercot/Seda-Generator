using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommonClassesLibrary {
    public class Utils {
        /*
         * Donne un affichage lisible du contenu d'un tableau d'octets
         * */
        public static String getReadableByteArray(byte[] array) {
            StringBuilder strArray = new StringBuilder();
            int i;
            for (i = 0; i < array.Length; i++) {
                strArray.Append(String.Format("{0:X2}", array[i]));
            }
            return strArray.ToString();
        }

        /*
         * Calcule le hash sha1 d'un fichier dont on précise le nom
         * Susceptible de lever une exception si le fichier ou le réperroire n'existe pas
         * */
        public static String computeSha1Hash(String filename) {
            SHA1 sha1 = SHA1CryptoServiceProvider.Create();
            // Create a fileStream for the file.
            byte[] hashValue;
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            // Be sure it's positioned to the beginning of the stream.
            fileStream.Position = 0;
            // Compute the hash of the fileStream.
            hashValue = sha1.ComputeHash(fileStream);
            String dataSha1 = Utils.getReadableByteArray(hashValue).ToLower();
            fileStream.Close();
            return dataSha1;
        }

        /*
         * Calcule le hash sha256 d'un fichier dont on précise le nom
         * Susceptible de lever une exception si le fichier ou le réperroire n'existe pas
         * */
        public static String computeSha256Hash(String filename) {
            SHA256 sha256 = SHA256CryptoServiceProvider.Create();
            // Create a fileStream for the file.
            byte[] hashValue;
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            // Be sure it's positioned to the beginning of the stream.
            fileStream.Position = 0;
            // Compute the hash of the fileStream.
            hashValue = sha256.ComputeHash(fileStream);
            String dataSha256 = Utils.getReadableByteArray(hashValue).ToLower();
            fileStream.Close();
            return dataSha256;
        }

        /*
         * Compte le nombre d'occurrences d'un caractère dans une chaîne
         * */
        public static int nbOccur(char ch, String str) {
            int nbChars = 0;
            foreach (char caractere in str) {
                if (caractere == ch)
                    ++nbChars;
            }
            return nbChars;
        }
    }
}
