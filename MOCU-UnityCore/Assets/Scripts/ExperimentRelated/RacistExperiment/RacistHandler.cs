using System;
using UnityEngine;
using System.Collections;

using MoogModule;


namespace RacistExperiment
{
    public class RacistHandler : MonoBehaviour
    {
        private RacistParameters _parameters;
        private RacistExperiment _experiment;
        private RacistResponseHandler _input;

        private RacistExperimentState _state;
        private Vector3 _cameraPosition;
        private RacistTrial _currentTrial;
        private Transform _camera;
        private TrajectoryManager _trajectoryManager;


        private void Awake()
        {
            _parameters = new RacistParameters { TrialsNumber = 8, StimulusType = RacistStimulusType.VisualVisual };
            _experiment = new RacistExperiment(_parameters);
            _input = GetComponent<RacistResponseHandler>();

            _state = RacistExperimentState.None;
            _cameraPosition = new Vector3 { z = _parameters.StartPosition.Surge, y = 1.7f, x = 0 };
            _camera = GameObject.Find("VrHelmetCamera").transform;
            _camera.position = _cameraPosition;
            _currentTrial = null;

            _input.GotAnswer_Up += HandleInput_Up;
            _input.GotAnswer_Down += HandleInput_Down;
            _input.GotSignal_Start += HandleInput_Start;
        }

        private void Start()
        {
            StartCoroutine(Loop());
        }

        private IEnumerator Loop()
        {
            while (!_experiment.HasFinished)
            {
                yield return Init();
                yield return WaitToStart();
                yield return MoveFirstInterval();
                yield return InterIntervalsWait();
                yield return MoveSecondInterval();
                yield return WaitForAnswer();
                yield return Returning();
            }
        }

        private IEnumerator Init()
        {
            _state = RacistExperimentState.Initializing;
            _experiment.GenerateTrials();
            yield return null;
        }

        private IEnumerator WaitToStart()
        {
            _state = RacistExperimentState.ReadyToStart;

            while (_state != RacistExperimentState.FirstInterval)
                yield return null;
        }

        private IEnumerator MoveFirstInterval()
        {
            // temp
            return null;
        }

        private IEnumerator InterIntervalsWait()
        {
            yield return new WaitForSeconds((float)_parameters.PauseBetweenIntervalsDuration.TotalSeconds);
            _state = RacistExperimentState.SecondInterval;
        }

        private IEnumerator MoveSecondInterval()
        {
            // temp
            return null;
        }

        private IEnumerator WaitForAnswer()
        {
            float timeout = (float)_parameters.TimeToAnswer.TotalSeconds;
            float elapsed = 0f;

            Debug.Log("Ожидание ответа...");

            while (_state != RacistExperimentState.Returning && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_state != RacistExperimentState.Returning)
            {
                _experiment.SetParticipantAnswer(RacistAnswer.Late);
                _state = RacistExperimentState.Returning;
            }
        }

        private IEnumerator Returning()
        {
            // temp
            return null;
        }

        /*private void Update()
        {
            switch (_state)
            {
                case RacistExperimentState.None:
                    break;

                case RacistExperimentState.Initializing:
                    _trajectoryManager = new TrajectoryManager(new MoveByTrajectoryParameters
                    {
                        StartPoint = new DofParameters { Surge = 0 },
                        EndPoint = new DofParameters { Surge = _currentTrial.FirstInterval.Distance },
                        MovementDuration = _currentTrial.FirstInterval.Duration,
                        TrajectoryType = TrajectoryType.Linear,
                        TrajectoryProfile = TrajectoryProfile.CDF,
                        DelayHandling = DelayCompensationStrategy.Ignore,
                        TrajectoryTypeSettings = new TrajectoryTypeSettings { Linear = new TrajectoryTypeSettings_Linear { } },
                        TrajectoryProfileSettings = new TrajectoryProfileSettings { CDF = new TrajectoryProfileSettings_CDF { Sigmas = 3 } }
                    });
                    _state = RacistExperimentState.ReadyToStart;
                    break;

                case RacistExperimentState.ReadyToStart:
                    break;

                case RacistExperimentState.FirstInterval:
                    var position = _trajectoryManager.GetNextPosition();
                    if (position == null)
                        _state = RacistExperimentState.InterIntervalPause;
                    break;

                case RacistExperimentState.InterIntervalPause:
                    break;

                case RacistExperimentState.SecondInterval:
                    break;

                case RacistExperimentState.AnswerPhase:
                    break;

                case RacistExperimentState.Returning:
                    break;

                case RacistExperimentState.Finished:
                    break;

                default:
                    break;
            }

            _camera.position = new Vector3 { z = _parameters.StartPosition.Surge, y = 1.7f, x = 0 };
            //Debug.Log($"state: {_state}");
        }*/

        // ................

        private void HandleInput_Up()
        {
            if (_state != RacistExperimentState.AnswerPhase) return;

            _experiment.SetParticipantAnswer(RacistAnswer.SecondWasLonger);

            _state = RacistExperimentState.Returning;
        }

        private void HandleInput_Down()
        {
            if (_state != RacistExperimentState.AnswerPhase) return;

            _experiment.SetParticipantAnswer(RacistAnswer.FirstWasLonger);

            _state = RacistExperimentState.Returning;
        }

        private void HandleInput_Start()
        {
            if (_state != RacistExperimentState.ReadyToStart) return;

            _currentTrial = _experiment.StartTrial();
            _state = RacistExperimentState.FirstInterval;
        }

        // ................

        /*private void RecalculateCameraPosition()
        {
            _cameraPosition = new Vector3 { z = _parameters.StartPosition.Surge, y = 1.7f, x = 0 };
            _camera.position = _cameraPosition;
        }*/
    }
}