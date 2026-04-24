using System;

namespace CupkekGames.Core
{
  public static class EnumHelper
  {

    // returns the index of the enum value
    public static int GetIndexOfEnum<T>(T value) where T : System.Enum
    {
      return Array.IndexOf(Enum.GetValues(value.GetType()), value);
    }

    // returns the enum value of the index
    public static T GetEnumFromIndex<T>(int index) where T : System.Enum
    {
      return (T)Enum.GetValues(typeof(T)).GetValue(index);
    }
  }
}