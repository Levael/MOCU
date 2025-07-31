using System;
using UnityEngine;
using System.Collections;

using MoogModule;
using UnityEngine.InputSystem.XR;
using Unity.VisualScripting;
using System.Threading;


namespace RacistExperiment
{
    public class RacistHandler : MonoBehaviour
    {
        private RacistParameters _parameters;
        private RacistExperiment _experiment;
        private RacistResponseHandler _input;
        private RacistSound _sound;

        private RacistTrialState _state;
        private DofParameters _cameraStartPosition;
        private DofParameters _whereCameraShouldBe;
        private RacistTrial _currentTrial;
        private Transform _camera;
        private TrajectoryManager _trajectoryManager;


        private void Awake()
        {
            _parameters = new RacistParameters { TrialsNumber = 4, StimulusType = RacistStimulusType.VisualVisual };
            _experiment = new RacistExperiment(_parameters);
            _input = GetComponent<RacistResponseHandler>();
            _sound = GetComponent<RacistSound>();

            _state = RacistTrialState.None;
            _cameraStartPosition = new DofParameters { Surge = _parameters.StartPosition.Surge, Heave = 1.7f, Sway = 0 };
            _whereCameraShouldBe = _cameraStartPosition;
            _camera = GameObject.Find("Camera Offset").transform;
            _currentTrial = null;

            _input.GotAnswer_Up += HandleInput_Up;
            _input.GotAnswer_Down += HandleInput_Down;
            _input.GotSignal_Start += HandleInput_Start;
        }

        private void Start()
        {
            StartCoroutine(Loop());
        }

        private void Update()
        {
            if (_state == RacistTrialState.FirstInterval)
                _whereCameraShouldBe.Surge -= 0.01f;
            
            if (_state == RacistTrialState.SecondInterval)
                _whereCameraShouldBe.Surge -= 0.01f;

            if (_state == RacistTrialState.Returning)
                _whereCameraShouldBe.Surge += 0.01f;


            _camera.position = new Vector3 { z = _whereCameraShouldBe.Surge, y = _whereCameraShouldBe.Heave, x = _whereCameraShouldBe.Sway };
        }

        private IEnumerator Loop()
        {
            Debug.Log("Experiment started");

            while (!_experiment.HasFinished)
            {
                Debug.Log("Trial started");

                yield return Initializing();
                // here waiting for 'start btn' event
                yield return WaitingToStartSignal();
                yield return FirstInterval();
                yield return InterIntervalPause();
                yield return SecondInterval();
                yield return AnswerPhase();
                yield return PreReturningPause();
                yield return Returning();
                yield return Analyzation();

                Debug.Log("Trial finished");
            }

            Debug.Log("Experiment finished");
        }

        private IEnumerator Initializing()
        {
            _state = RacistTrialState.Initializing;
            _experiment.GenerateTrials();
            yield return new WaitForSeconds((float)_parameters.DelayBeforeStartSound.TotalSeconds);

            _sound.PlaySound_Start();
            _state = RacistTrialState.ReadyToStart;
        }

        private IEnumerator WaitingToStartSignal()
        {
            while (_state != RacistTrialState.PreFirstIntervalPause)
                yield return null;

            yield return new WaitForSeconds((float)_parameters.DelayAfterStartSignal.TotalSeconds);
            _currentTrial = _experiment.StartTrial();
            _state = RacistTrialState.FirstInterval;
        }

        private IEnumerator FirstInterval()
        {
            // here
            yield return new WaitForSeconds((float)_parameters.FirstMovementDuration.TotalSeconds);
            _state = RacistTrialState.InterIntervalPause;
        }

        private IEnumerator InterIntervalPause()
        {
            yield return new WaitForSeconds((float)_parameters.PauseBetweenIntervalsDuration.TotalSeconds);
            _state = RacistTrialState.SecondInterval;
        }

        private IEnumerator SecondInterval()
        {
            // here
            yield return new WaitForSeconds((float)_parameters.SecondMovementDuration.TotalSeconds);
            _state = RacistTrialState.AnswerPhase;
        }

        private IEnumerator AnswerPhase()
        {
            float timeout = (float)_parameters.TimeToAnswer.TotalSeconds;
            float elapsed = 0f;

            while (_currentTrial.ReceivedAnswer == RacistAnswer.None && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_currentTrial.ReceivedAnswer == RacistAnswer.None && elapsed >= timeout)
            {
                _experiment.SetParticipantAnswer(RacistAnswer.Late);
                _sound.PlaySound_AnswerIsLate();
            }

            _state = RacistTrialState.PreReturningPause;
        }

        private IEnumerator PreReturningPause()
        {
            yield return new WaitForSeconds((float)_parameters.DelayAfterAnswerSignal.TotalSeconds);
            _state = RacistTrialState.Returning;
        }

        private IEnumerator Returning()
        {
            yield return new WaitForSeconds((float)_parameters.BackwardMovementDuration.TotalSeconds);
            _state = RacistTrialState.Analyzation;
        }

        private IEnumerator Analyzation()
        {
            _experiment.FinishTrial();
            yield return null;
        }

        /*private void Update()
        {
            switch (_state)
            {
                case RacistTrialState.None:
                    break;

                case RacistTrialState.Initializing:
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
                    _state = RacistTrialState.PreFirstIntervalPause;
                    break;

                case RacistTrialState.PreFirstIntervalPause:
                    break;

                case RacistTrialState.FirstInterval:
                    var position = _trajectoryManager.GetNextPosition();
                    if (position == null)
                        _state = RacistTrialState.InterIntervalPause;
                    break;

                case RacistTrialState.InterIntervalPause:
                    break;

                case RacistTrialState.SecondInterval:
                    break;

                case RacistTrialState.AnswerPhase:
                    break;

                case RacistTrialState.Returning:
                    break;

                case RacistTrialState.Analyzation:
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
            if (_state != RacistTrialState.AnswerPhase) return;

            _experiment.SetParticipantAnswer(RacistAnswer.SecondWasLonger);
            _sound.PlaySound_GotAnswer();
        }

        private void HandleInput_Down()
        {
            if (_state != RacistTrialState.AnswerPhase) return;

            _experiment.SetParticipantAnswer(RacistAnswer.FirstWasLonger);
            _sound.PlaySound_GotAnswer();
        }

        private void HandleInput_Start()
        {
            if (_state != RacistTrialState.ReadyToStart) return;

            _state = RacistTrialState.PreFirstIntervalPause;
        }

        // ................

        /*private void RecalculateCameraPosition()
        {
            _cameraStartPosition = new Vector3 { z = _parameters.StartPosition.Surge, y = 1.7f, x = 0 };
            _camera.position = _cameraStartPosition;
        }*/

        /*private void SetCameraControlMode(bool fullyManual)
        {
            var tpd = Camera.main.GetComponent<TrackedPoseDriver>();

            if (fullyManual)
            {
                tpd.trackingType = TrackedPoseDriver.TrackingType.RotationOnly;
                _camera.position = new Vector3 { z = _parameters.StartPosition.Surge, y = 1.7f, x = 0 };
            }
            else
            {
                tpd.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
                _camera.position = new Vector3 { z = _parameters.StartPosition.Surge, y = 0, x = 0 };
            }
        }*/
    }
}