#region

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace CodeClassifier
{
    public class Serializer
    {
        public static void SerializeObject(string filename, object objectToSerialize)
        {
            using (Stream stream = File.Create(filename))
            {
                var bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, objectToSerialize);
            }
        }

        public static object DeSerializeObject(string filename)
        {
            using (Stream stream = File.Open(filename, FileMode.Open))
            {
                var bFormatter = new BinaryFormatter();
                var objectToSerialize = bFormatter.Deserialize(stream);
                return objectToSerialize;
            }
        }
    }
}