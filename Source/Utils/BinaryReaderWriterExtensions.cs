using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr
{
   static class BinaryReaderWriterExtensions
   {
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Extension method.")]
      public static void WriteNullableString(this BinaryWriter writer, string value)
      {
         writer.Write(value != null);
         if (value != null)
            writer.Write(value);
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
      public static string ReadNullableString(this BinaryReader reader)
      {
         if (reader.ReadBoolean())
            return reader.ReadString();
         else
            return null;
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Extension method.")]
      public static Guid ReadGuid(this BinaryReader reader)
      {
         return new Guid(reader.ReadBytes(16));
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Extension method.")]
      public static void Write(this BinaryWriter writer, Guid guid)
      {
         writer.Write(guid.ToByteArray());
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Extension method.")]
      public static void Write(this BinaryWriter writer, bool? value)
      {
         writer.Write(value.HasValue);
         if (value.HasValue)
            writer.Write(value.Value);
      }

      public static bool? ReadNullableBool(this BinaryReader reader)
      {
         if (reader.ReadBoolean())
            return reader.ReadBoolean();
         else
            return null;
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Extension method.")]
      public static void Write(this BinaryWriter writer, Guid? guid)
      {
         writer.Write(guid.HasValue);
         if (guid.HasValue)
            writer.Write(guid.Value);
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Extension method.")]
      public static Guid? ReadNullableGuid(this BinaryReader reader)
      {
         if (reader.ReadBoolean())
            return reader.ReadGuid();
         else
            return null;
      }



      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Extension method.")]
      public static void Write(this BinaryWriter writer, int? value)
      {
         writer.Write(value.HasValue);
         if (value.HasValue)
            writer.Write(value.Value);
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Extension method.")]
      public static int? ReadNullableInt32(this BinaryReader reader)
      {
         bool hasValue = reader.ReadBoolean();
         if (hasValue)
            return reader.ReadInt32();
         else
            return null;
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Extension method.")]
      public static void Write(this BinaryWriter writer, long? value)
      {
         writer.Write(value.HasValue);
         if (value.HasValue)
            writer.Write(value.Value);
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Extension method.")]
      public static long? ReadNullableInt64(this BinaryReader reader)
      {
         bool hasValue = reader.ReadBoolean();
         if (hasValue)
            return reader.ReadInt64();
         else
            return null;
      }

   }
}
