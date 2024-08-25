using System.Collections.Generic;
using UnityEngine.UIElements;


namespace CustomUxmlElements
{
    public class SettingsDeviceBox : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<SettingsDeviceBox, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _textAttribute = new UxmlStringAttributeDescription { name = "text", defaultValue = "" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var settingsDeviceBox = (SettingsDeviceBox)ve;
                settingsDeviceBox.DeviceAlias = _textAttribute.GetValueFromBag(bag, cc);
            }
        }
        

        private SettingsDeviceBox _container;
        private CustomSlider _slider;
        private CustomImage _image;
        private TextElement _label;

        private Dictionary<DeviceCardStatus, string> _deviceCardStatusToUssClassNameMap = new ()
        {
            { DeviceCardStatus.Ready,       "normal"},
            { DeviceCardStatus.Disabled,    "is-inactive"},
            { DeviceCardStatus.Connecting,  "has-warning"},
            { DeviceCardStatus.NotChosen,   "has-error"},
            { DeviceCardStatus.Error,       "has-error"},
        };

        public SettingsDeviceBox()
        {
            _container = this;
            _slider = new();
            _image = new();
            _label = new();

            _container.Add(_slider);
            _container.Add(_image);
            _container.Add(_label);
        }

        public string DeviceAlias
        {
            get => _label.text;
            set => _label.text = value;
        }

        public float SliderValue
        {
            get => _slider.value;
            set => _slider.value = value;
        }

        public string UiName
        {
            get => _container.name;
        }

        public void SetStatus(DeviceCardStatus status)
        {
            foreach (var style in _deviceCardStatusToUssClassNameMap.Values)
                _label.RemoveFromClassList(style);

            _label.AddToClassList(_deviceCardStatusToUssClassNameMap[status]);
        }
    }
}