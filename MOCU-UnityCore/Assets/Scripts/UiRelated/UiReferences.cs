using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UiReferences : MonoBehaviour
{
    private Dictionary<string, VisualElement> _elements;
    private VisualElement _root;
    public Dictionary<string, string> _tab_body_relations;


    void Awake ()
    {
        _elements = new();
        _tab_body_relations = new();

        _root = GetComponent<UIDocument>().rootVisualElement;

        SetupTabBodyRelations();
        SetupReferences();
    }


    public void SetupReferences()
    {
        _elements.Add("root-element", _root);

        // HEADER
        _elements.Add("experiment-tab", _root.Q<VisualElement>("experiment-tab"));
        _elements.Add("debug-tab", _root.Q<VisualElement>("debug-tab"));
        _elements.Add("data-tab", _root.Q<VisualElement>("data-tab"));
        _elements.Add("info-tab", _root.Q<VisualElement>("info-tab"));
        _elements.Add("cameras-tab", _root.Q<VisualElement>("cameras-tab"));
        _elements.Add("settings-tab", _root.Q<VisualElement>("settings-tab"));

        _elements.Add("minimize-game-btn", _root.Q<VisualElement>("minimize-game-btn"));
        _elements.Add("close-game-btn", _root.Q<VisualElement>("close-game-btn"));


        // COMMON
        _elements.Add("main-body", _root.Q<VisualElement>("main-body"));


        // Experiment
        _elements.Add("experiment-body-right-part-monitors-eeg", _root.Q<VisualElement>("experiment-body-right-part-monitors-eeg"));

        _elements.Add("modal-windows", _root.Q<VisualElement>("modal-windows"));
        _elements.Add("exit-confirmation-modal-window", _root.Q<VisualElement>("exit-confirmation-modal-window"));
        _elements.Add("exit-confirm-btn", _root.Q<VisualElement>("exit-confirm-btn"));
        _elements.Add("exit-cancel-btn", _root.Q<VisualElement>("exit-cancel-btn"));

        _elements.Add("experiment-body", _root.Q<VisualElement>("experiment-body"));
        _elements.Add("debug-body", _root.Q<VisualElement>("debug-body"));
        _elements.Add("data-body", _root.Q<VisualElement>("data-body"));
        _elements.Add("info-body", _root.Q<VisualElement>("info-body"));
        _elements.Add("cameras-body", _root.Q<VisualElement>("cameras-body"));
        _elements.Add("settings-body", _root.Q<VisualElement>("settings-body"));

        _elements.Add("moog-connect-btn", _root.Q<VisualElement>("moog-connect-btn"));
        _elements.Add("moog-engage-btn", _root.Q<VisualElement>("moog-engage-btn"));
        _elements.Add("moog-park-btn", _root.Q<VisualElement>("moog-park-btn"));
        _elements.Add("trials-generate-btn", _root.Q<VisualElement>("trials-generate-btn"));
        _elements.Add("main-test-btn", _root.Q<VisualElement>("main-test-btn"));
        _elements.Add("test-trials-generate-btn", _root.Q<VisualElement>("test-trials-generate-btn"));
        _elements.Add("experiment-start-btn", _root.Q<VisualElement>("experiment-start-btn"));
        _elements.Add("experiment-pause-btn", _root.Q<VisualElement>("experiment-pause-btn"));
        _elements.Add("experiment-resume-btn", _root.Q<VisualElement>("experiment-resume-btn"));
        _elements.Add("experiment-stop-btn", _root.Q<VisualElement>("experiment-stop-btn"));

        _elements.Add("controls-intercom-part", _root.Q<VisualElement>("controls-intercom-part"));

        _elements.Add("controller-up-btn", _root.Q<VisualElement>("controller-up-btn"));
        _elements.Add("controller-left-btn", _root.Q<VisualElement>("controller-left-btn"));
        _elements.Add("controller-center-btn", _root.Q<VisualElement>("controller-center-btn"));
        _elements.Add("controller-right-btn", _root.Q<VisualElement>("controller-right-btn"));
        _elements.Add("controller-down-btn", _root.Q<VisualElement>("controller-down-btn"));

        _elements.Add("moog-status-block", _root.Q<VisualElement>("moog-status-block"));
        _elements.Add("oculus-status-block", _root.Q<VisualElement>("oculus-status-block"));
        _elements.Add("cedrus-status-block", _root.Q<VisualElement>("cedrus-status-block"));
        _elements.Add("gamepad-status-block", _root.Q<VisualElement>("gamepad-status-block"));
        _elements.Add("audio-status-block", _root.Q<VisualElement>("audio-status-block"));
        _elements.Add("eeg-status-block", _root.Q<VisualElement>("eeg-status-block"));
        _elements.Add("trials-status-block", _root.Q<VisualElement>("trials-status-block"));
        _elements.Add("running-status-block", _root.Q<VisualElement>("running-status-block"));

        _elements.Add("protocol-dropdownk", _root.Q<VisualElement>("protocol-dropdown"));
        _elements.Add("protocol-browse-btn", _root.Q<VisualElement>("protocol-browse-btn"));
        _elements.Add("protocol-save-btn", _root.Q<VisualElement>("protocol-save-btn"));
        _elements.Add("researcher-name-input", _root.Q<VisualElement>("researcher-name-input"));
        _elements.Add("participant-name-input", _root.Q<VisualElement>("participant-name-input"));

        _elements.Add("info-module-textbox", _root.Q<VisualElement>("info-module-textbox"));
        _elements.Add("warnings-module-textbox", _root.Q<VisualElement>("warnings-module-textbox"));
        _elements.Add("answers-graph-module-window", _root.Q<VisualElement>("answers-graph-module-window"));


        // Debug
        _elements.Add("debug-console-module-textbox", _root.Q<VisualElement>("debug-console-module-textbox"));

        _elements.Add("experiment-timeline-data-total-time", _root.Q<VisualElement>("experiment-timeline-data-total-time"));
        _elements.Add("experiment-timeline-data-status", _root.Q<VisualElement>("experiment-timeline-data-status"));
        _elements.Add("experiment-timeline-data-total-trials", _root.Q<VisualElement>("experiment-timeline-data-total-trials"));
        _elements.Add("experiment-timeline-data-defective-trials", _root.Q<VisualElement>("experiment-timeline-data-defective-trials"));
        _elements.Add("experiment-timeline-data-dangerous-trials", _root.Q<VisualElement>("experiment-timeline-data-dangerous-trials"));

        _elements.Add("trial-data-average-unity-fps", _root.Q<VisualElement>("trial-data-average-unity-fps"));
        _elements.Add("trial-data-total-time-moog", _root.Q<VisualElement>("trial-data-total-time-moog"));
        _elements.Add("trial-data-total-time-oculus", _root.Q<VisualElement>("trial-data-total-time-oculus"));
        _elements.Add("trial-data-trial-status", _root.Q<VisualElement>("trial-data-trial-status"));
        _elements.Add("trial-data-answer-get-in", _root.Q<VisualElement>("trial-data-answer-get-in"));
        _elements.Add("trial-data-right-answer", _root.Q<VisualElement>("trial-data-right-answer"));
        _elements.Add("trial-data-received-answer", _root.Q<VisualElement>("trial-data-received-answer"));
        _elements.Add("trial-data-received-answer-is-correct", _root.Q<VisualElement>("trial-data-received-answer-is-correct"));

        _elements.Add("cpu-data-average-usage", _root.Q<VisualElement>("cpu-data-average-usage"));
        _elements.Add("memory-data-average-usage", _root.Q<VisualElement>("memory-data-average-usage"));


        // Settings
        _elements.Add("settings-devices-update-btn", _root.Q<VisualElement>("settings-devices-update-btn"));
        _elements.Add("settings-devices-back-btn", _root.Q<VisualElement>("settings-devices-back-btn"));

        _elements.Add("settings-device-box-microphone-researcher", _root.Q<VisualElement>("settings-device-box-microphone-researcher"));
        _elements.Add("settings-device-box-microphone-participant", _root.Q<VisualElement>("settings-device-box-microphone-participant"));
        _elements.Add("settings-device-box-speaker-researcher", _root.Q<VisualElement>("settings-device-box-speaker-researcher"));
        _elements.Add("settings-device-box-speaker-participant", _root.Q<VisualElement>("settings-device-box-speaker-participant"));
        _elements.Add("settings-device-box-camera-1", _root.Q<VisualElement>("settings-device-box-camera-1"));
        _elements.Add("settings-device-box-camera-2", _root.Q<VisualElement>("settings-device-box-camera-2"));
        _elements.Add("settings-device-box-controller", _root.Q<VisualElement>("settings-device-box-controller"));
        _elements.Add("settings-devices-choose-device-window", _root.Q<VisualElement>("settings-devices-choose-device-window"));


    }

    private void SetupTabBodyRelations()
    {
        _tab_body_relations.Add("experiment-tab", "experiment-body");
        _tab_body_relations.Add("debug-tab", "debug-body");
        _tab_body_relations.Add("data-tab", "data-body");
        _tab_body_relations.Add("info-tab", "info-body");
        _tab_body_relations.Add("cameras-tab", "cameras-body");
        _tab_body_relations.Add("settings-tab", "settings-body");
    }

    public VisualElement GetElement(string name)
    {
        return _elements.ContainsKey(name) ? _elements[name] : null;
    }

    public string GetTabBodyRelation(string tabName)
    {
        return _tab_body_relations.ContainsKey(tabName) ? _tab_body_relations[tabName] : null;
    }

    public List<VisualElement> GetHeaderTabs()
    {
        return _root.Query<VisualElement>().Class("tab").ToList();
    }

    public List<VisualElement> GetDevicesBoxes()
    {
        return _root.Query<VisualElement>().Class("settings-device-box").ToList();
    }
}
