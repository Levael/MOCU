using UnityEngine;


namespace RacistExperiment
{
    public class RacistHandler : MonoBehaviour
    {
        private RacistParameters _parameters;
        private RacistExperiment _experiment;




        private void Awake()
        {
            _parameters = new RacistParameters { TrialsNumber = 8, StimulusType = RacistStimulusType.VisualVisual };
            _experiment = new RacistExperiment(_parameters);
        }

        private void Start()
        {
            _experiment.GenerateTrials();
            Debug.Log($"Trials init data: {_experiment.GetTrialsData()}");
        }
    }
}