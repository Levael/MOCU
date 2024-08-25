using UnityEngine.UIElements;
using UnityEngine;


namespace CustomUxmlElements
{
    // TODO: remake it to normal one later
    // add GET and SET for easier data picking from code
    public class CustomSlider : Slider
    {
        public new class UxmlFactory : UxmlFactory<CustomSlider, Slider.UxmlTraits> { }

        public CustomSlider()
        {
            var dragger = this.Q<VisualElement>("unity-dragger");

            if (dragger == null)
            {
                Debug.Log("CustomSlider > dragger == null ");
                return;
            }

            var fillElement = new VisualElement();
            fillElement.AddToClassList("slider-filler");
            dragger.Add(fillElement);

            this.lowValue = 0;      // min value
            this.highValue = 100;   // max value
            this.pageSize = 0;      // any int except 0 will lead to 'jump' changes
            this.value = 0;         // default value
        }
    }
}