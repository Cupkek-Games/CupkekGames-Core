using UnityEngine;

namespace CupkekGames.Core
{
    public class MultiLineHeaderAttribute : PropertyAttribute
    {
        public readonly string headerText;

        public MultiLineHeaderAttribute(string headerText)
        {
            this.headerText = headerText;
        }
    }
}