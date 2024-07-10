using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UiReferences : MonoBehaviour
{
    private Dictionary<string, VisualElement> _elements;
    public VisualElement root;
    public Dictionary<string, string> _tab_body_relations;


    void Awake ()
    {
        _elements = new();
        _tab_body_relations = new();

        root = GetComponent<UIDocument>().rootVisualElement;

        SetupTabBodyRelations();
        SetupReferences();
    }


    public void SetupReferences()
    {
        _elements.Add("root-element", root);

        // HEADER
        _elements.Add("experiment-tab", root.Q<VisualElement>("experiment-tab"));
        _elements.Add("debug-tab", root.Q<VisualElement>("debug-tab"));
        _elements.Add("data-tab", root.Q<VisualElement>("data-tab"));
        _elements.Add("info-tab", root.Q<VisualElement>("info-tab"));
        _elements.Add("cameras-tab", root.Q<VisualElement>("cameras-tab"));
        _elements.Add("settings-tab", root.Q<VisualElement>("settings-tab"));

        _elements.Add("minimize-game-btn", root.Q<VisualElement>("minimize-game-btn"));
        _elements.Add("close-game-btn", root.Q<VisualElement>("close-game-btn"));


        // COMMON
        _elements.Add("main-body", root.Q<VisualElement>("main-body"));


        // Experiment
        _elements.Add("experiment-body-right-part-monitors-eeg", root.Q<VisualElement>("experiment-body-right-part-monitors-eeg"));

        _elements.Add("modal-windows", root.Q<VisualElement>("modal-windows"));
        _elements.Add("exit-confirmation-modal-window", root.Q<VisualElement>("exit-confirmation-modal-window"));
        _elements.Add("exit-confirm-btn", root.Q<VisualElement>("exit-confirm-btn"));
        _elements.Add("exit-cancel-btn", root.Q<VisualElement>("exit-cancel-btn"));

        _elements.Add("experiment-body", root.Q<VisualElement>("experiment-body"));
        _elements.Add("debug-body", root.Q<VisualElement>("debug-body"));
        _elements.Add("data-body", root.Q<VisualElement>("data-body"));
        _elements.Add("info-body", root.Q<VisualElement>("info-body"));
        _elements.Add("cameras-body", root.Q<VisualElement>("cameras-body"));
        _elements.Add("settings-body", root.Q<VisualElement>("settings-body"));

        _elements.Add("moog-connect-btn", root.Q<VisualElement>("moog-connect-btn"));
        _elements.Add("moog-engage-btn", root.Q<VisualElement>("moog-engage-btn"));
        _elements.Add("moog-park-btn", root.Q<VisualElement>("moog-park-btn"));
        _elements.Add("trials-generate-btn", root.Q<VisualElement>("trials-generate-btn"));
        _elements.Add("test-trials-generate-btn", root.Q<VisualElement>("test-trials-generate-btn"));
        _elements.Add("experiment-start-btn", root.Q<VisualElement>("experiment-start-btn"));
        _elements.Add("experiment-pause-btn", root.Q<VisualElement>("experiment-pause-btn"));
        _elements.Add("experiment-resume-btn", root.Q<VisualElement>("experiment-resume-btn"));
        _elements.Add("experiment-stop-btn", root.Q<VisualElement>("experiment-stop-btn"));

        _elements.Add("controls-intercom-part", root.Q<VisualElement>("controls-intercom-part"));

        _elements.Add("controller-up-btn", root.Q<VisualElement>("controller-up-btn"));
        _elements.Add("controller-left-btn", root.Q<VisualElement>("controller-left-btn"));
        _elements.Add("controller-center-btn", root.Q<VisualElement>("controller-center-btn"));
        _elements.Add("controller-right-btn", root.Q<VisualElement>("controller-right-btn"));
        _elements.Add("controller-down-btn", root.Q<VisualElement>("controller-down-btn"));

        _elements.Add("moog-status-block", root.Q<VisualElement>("moog-status-block"));
        _elements.Add("oculus-status-block", root.Q<VisualElement>("oculus-status-block"));
        _elements.Add("cedrus-status-block", root.Q<VisualElement>("cedrus-status-block"));
        _elements.Add("gamepad-status-block", root.Q<VisualElement>("gamepad-status-block"));
        _elements.Add("audio-status-block", root.Q<VisualElement>("audio-status-block"));
        _elements.Add("eeg-status-block", root.Q<VisualElement>("eeg-status-block"));
        _elements.Add("trials-status-block", root.Q<VisualElement>("trials-status-block"));
        _elements.Add("running-status-block", root.Q<VisualElement>("running-status-block"));

        _elements.Add("protocol-dropdownk", root.Q<VisualElement>("protocol-dropdown"));
        _elements.Add("protocol-browse-btn", root.Q<VisualElement>("protocol-browse-btn"));
        _elements.Add("protocol-save-btn", root.Q<VisualElement>("protocol-save-btn"));
        _elements.Add("researcher-name-input", root.Q<VisualElement>("researcher-name-input"));
        _elements.Add("participant-name-input", root.Q<VisualElement>("participant-name-input"));

        _elements.Add("info-module-textbox", root.Q<VisualElement>("info-module-textbox"));
        _elements.Add("warnings-module-textbox", root.Q<VisualElement>("warnings-module-textbox"));
        _elements.Add("answers-graph-module-window", root.Q<VisualElement>("answers-graph-module-window"));


        // Debug
        _elements.Add("debug-console-module-textbox", root.Q<VisualElement>("debug-console-module-textbox"));

        _elements.Add("experiment-timeline-data-total-time", root.Q<VisualElement>("experiment-timeline-data-total-time"));
        _elements.Add("experiment-timeline-data-status", root.Q<VisualElement>("experiment-timeline-data-status"));
        _elements.Add("experiment-timeline-data-total-trials", root.Q<VisualElement>("experiment-timeline-data-total-trials"));
        _elements.Add("experiment-timeline-data-defective-trials", root.Q<VisualElement>("experiment-timeline-data-defective-trials"));
        _elements.Add("experiment-timeline-data-dangerous-trials", root.Q<VisualElement>("experiment-timeline-data-dangerous-trials"));

        _elements.Add("trial-data-average-unity-fps", root.Q<VisualElement>("trial-data-average-unity-fps"));
        _elements.Add("trial-data-total-time-moog", root.Q<VisualElement>("trial-data-total-time-moog"));
        _elements.Add("trial-data-total-time-oculus", root.Q<VisualElement>("trial-data-total-time-oculus"));
        _elements.Add("trial-data-trial-status", root.Q<VisualElement>("trial-data-trial-status"));
        _elements.Add("trial-data-answer-get-in", root.Q<VisualElement>("trial-data-answer-get-in"));
        _elements.Add("trial-data-right-answer", root.Q<VisualElement>("trial-data-right-answer"));
        _elements.Add("trial-data-received-answer", root.Q<VisualElement>("trial-data-received-answer"));
        _elements.Add("trial-data-received-answer-is-correct", root.Q<VisualElement>("trial-data-received-answer-is-correct"));

        _elements.Add("debug-memory-value", root.Q<VisualElement>("debug-memory-value"));
        _elements.Add("debug-gc-value", root.Q<VisualElement>("debug-gc-value"));
        _elements.Add("debug-time-working-value", root.Q<VisualElement>("debug-time-working-value"));

        _elements.Add("debug-test-btn-1", root.Q<VisualElement>("debug-test-btn-1"));
        _elements.Add("debug-test-btn-2", root.Q<VisualElement>("debug-test-btn-2"));

        _elements.Add("debug-current-fps-value", root.Q<VisualElement>("debug-current-fps-value"));
        _elements.Add("debug-last-frame-value", root.Q<VisualElement>("debug-last-frame-value"));
        _elements.Add("debug-average-frame-value", root.Q<VisualElement>("debug-average-frame-value"));

        _elements.Add("number_of_running_daemons_label", root.Q<VisualElement>("number_of_running_daemons_label"));
        _elements.Add("debug-daemons-activities", root.Q<VisualElement>("debug-daemons-activities"));
        


        // Settings
        _elements.Add("settings-devices-update-btn", root.Q<VisualElement>("settings-devices-update-btn"));
        _elements.Add("settings-devices-back-btn", root.Q<VisualElement>("settings-devices-back-btn"));

        _elements.Add("settings-device-box-microphone-researcher", root.Q<VisualElement>("settings-device-box-microphone-researcher"));
        _elements.Add("settings-device-box-microphone-participant", root.Q<VisualElement>("settings-device-box-microphone-participant"));
        _elements.Add("settings-device-box-speaker-researcher", root.Q<VisualElement>("settings-device-box-speaker-researcher"));
        _elements.Add("settings-device-box-speaker-participant", root.Q<VisualElement>("settings-device-box-speaker-participant"));
        _elements.Add("settings-device-box-camera-1", root.Q<VisualElement>("settings-device-box-camera-1"));
        _elements.Add("settings-device-box-camera-2", root.Q<VisualElement>("settings-device-box-camera-2"));
        _elements.Add("settings-device-box-controller", root.Q<VisualElement>("settings-device-box-controller"));
        _elements.Add("settings-devices-choose-device-window", root.Q<VisualElement>("settings-devices-choose-device-window"));


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
        return root.Query<VisualElement>().Class("tab").ToList();
    }

    public List<VisualElement> GetDevicesBoxes()
    {
        return root.Query<VisualElement>().Class("settings-device-box").ToList();
    }
}
