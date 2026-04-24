#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace CupkekGames.Core.Editor
{
  [CustomEditor(typeof(FadeableMono), true)]
  public class FadeableMonoEditor : UnityEditor.Editor
  {
    public override VisualElement CreateInspectorGUI()
    {
      // Create a container for the UI elements
      var container = new VisualElement();

      InspectorElement.FillDefaultInspector(container, serializedObject, this);

      // Reference to the target script
      FadeableMono myScript = (FadeableMono)target;

      // Create and add the "Fade In" button
      var fadeInButton = new Button(() => myScript.Fadeable.FadeIn())
      {
        text = "Fade In"
      };
      fadeInButton.style.flexGrow = 1;
      container.Add(fadeInButton);

      // Create and add the "Fade Out" button
      var fadeOutButton = new Button(() => myScript.Fadeable.FadeOut())
      {
        text = "Fade Out"
      };
      fadeOutButton.style.flexGrow = 1;
      container.Add(fadeOutButton);

      return container;
    }
  }
}
#endif