using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUxmlElements
{

    // TODO: remake it to normal one later
    public class CustomSlider : Slider
    {
        public new class UxmlFactory : UxmlFactory<CustomSlider, Slider.UxmlTraits> { }


        public CustomSlider()
        {
            var dragger = this.Q<VisualElement>("unity-dragger");

            if (dragger != null)
            {
                VisualElement fillElement = new VisualElement();
                fillElement.AddToClassList("slider-filler");
                dragger.Add(fillElement);
            }

            //RegisterCallback<GeometryChangedEvent>(e => OnGeometryChanged(e));
        }

        /*private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(e => OnGeometryChanged(e));

            var dragger = this.Q<VisualElement>("unity-dragger");

            if (dragger != null)
            {
                VisualElement fillElement = new VisualElement();
                fillElement.AddToClassList("slider-filler");
                dragger.Add(fillElement);
            }
        }*/
    }

}